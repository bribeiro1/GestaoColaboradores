/* app.js - infraestrutura compartilhada pelas duas telas: token antiforgery,
   leitura do ProblemDetails, formatação pt-BR, escape de HTML e notificações. */

window.app = (function ($) {
    'use strict';

    // A API trafega valores crus; a formatação acontece na apresentação,
    // para o mesmo endpoint servir tela, relatório e integração.
    var formatadorMoeda = new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    var formatadorData = new Intl.DateTimeFormat('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    });

    function formatarMoeda(valor) {
        if (valor === null || valor === undefined || isNaN(valor)) {
            return '-';
        }
        return formatadorMoeda.format(valor);
    }

    function formatarData(valorIso) {
        if (!valorIso) {
            return '-';
        }
        var data = new Date(valorIso);
        return isNaN(data.getTime()) ? '-' : formatadorData.format(data);
    }

    /** "1.234,56" -> 1234.56. JSON é sempre invariante: vírgula decimal
        geraria um texto que o desserializador rejeita. */
    function converterParaNumero(texto) {
        if (typeof texto === 'number') {
            return texto;
        }
        if (!texto) {
            return NaN;
        }
        var limpo = String(texto)
            .replace(/\s/g, '')
            .replace(/R\$/g, '')
            .replace(/\./g, '')
            .replace(',', '.');
        return parseFloat(limpo);
    }

    /** Formata um numero para exibicao em campo de texto: 1234.5 -> "1.234,50". */
    function formatarNumeroParaCampo(valor) {
        if (valor === null || valor === undefined || isNaN(valor)) {
            return '';
        }
        return Number(valor).toLocaleString('pt-BR', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    // -----------------------------------------------------------------
    // Seguranca
    // -----------------------------------------------------------------

    /** Escapa HTML antes de inserir no DOM. O nome é texto livre vindo do
        banco: sem isso, um nome contendo <script> seria executado por quem
        abrisse a listagem - XSS armazenado. */
    function escaparHtml(valor) {
        if (valor === null || valor === undefined) {
            return '';
        }
        return String(valor)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    /** Le o token antiforgery renderizado pelo layout. */
    function obterTokenAntiforgery() {
        return $('input[name="__RequestVerificationToken"]').first().val();
    }

    // -----------------------------------------------------------------
    // Comunicacao com a API
    // -----------------------------------------------------------------

    /** Wrapper único sobre jQuery.ajax. Centraliza o que precisa valer para
        todas as chamadas: header antiforgery nos verbos que alteram estado,
        contentType JSON e timeout. */
    function requisitar(opcoes) {
        var configuracao = {
            url: opcoes.url,
            type: opcoes.metodo || 'GET',
            dataType: 'json',
            timeout: opcoes.timeout || 30000,
            headers: {}
        };

        if (configuracao.type !== 'GET') {
            configuracao.headers['RequestVerificationToken'] = obterTokenAntiforgery();
        }

        if (opcoes.corpo !== undefined) {
            configuracao.contentType = 'application/json; charset=utf-8';
            configuracao.data = JSON.stringify(opcoes.corpo);
        } else if (opcoes.parametros) {
            configuracao.data = opcoes.parametros;
        }

        return $.ajax(configuracao);
    }

    /** Traduz a resposta de erro em mensagem. O servidor responde em
        ProblemDetails; falhas de validação trazem o dicionário "errors" por
        campo, usado pelo formulário para marcar o campo exato. */
    function interpretarErro(jqXHR) {
        var resultado = {
            mensagem: 'Não foi possível concluir a operação.',
            porCampo: null,
            status: jqXHR ? jqXHR.status : 0
        };

        if (!jqXHR) {
            return resultado;
        }

        if (jqXHR.statusText === 'timeout') {
            resultado.mensagem = 'O servidor demorou para responder. Tente novamente.';
            return resultado;
        }

        if (jqXHR.status === 0) {
            resultado.mensagem = 'Sem conexão com o servidor.';
            return resultado;
        }

        var corpo = jqXHR.responseJSON;

        if (!corpo && jqXHR.responseText) {
            try {
                corpo = JSON.parse(jqXHR.responseText);
            } catch (e) {
                corpo = null;
            }
        }

        if (corpo) {
            if (corpo.errors) {
                resultado.porCampo = corpo.errors;
                var primeiroCampo = Object.keys(corpo.errors)[0];
                if (primeiroCampo && corpo.errors[primeiroCampo].length) {
                    resultado.mensagem = corpo.errors[primeiroCampo][0];
                }
                return resultado;
            }
            if (corpo.detail) {
                resultado.mensagem = corpo.detail;
                return resultado;
            }
            if (corpo.title) {
                resultado.mensagem = corpo.title;
                return resultado;
            }
        }

        if (jqXHR.status >= 500) {
            resultado.mensagem = 'Erro interno no servidor. Tente novamente em instantes.';
        }

        return resultado;
    }

    // -----------------------------------------------------------------
    // Notificacoes
    // -----------------------------------------------------------------

    var estilosToast = {
        sucesso: { classe: 'text-bg-success', icone: 'bi-check-circle-fill' },
        erro: { classe: 'text-bg-danger', icone: 'bi-exclamation-octagon-fill' },
        alerta: { classe: 'text-bg-warning', icone: 'bi-exclamation-triangle-fill' },
        info: { classe: 'text-bg-secondary', icone: 'bi-info-circle-fill' }
    };

    /** Notificação não bloqueante. alert() congela a aba e quebra a
        percepção de que a página não recarregou. */
    function toast(mensagem, tipo) {
        var estilo = estilosToast[tipo] || estilosToast.info;

        var elemento = $(
            '<div class="toast align-items-center ' + estilo.classe + ' border-0" ' +
            'role="alert" aria-live="assertive" aria-atomic="true">' +
                '<div class="d-flex">' +
                    '<div class="toast-body">' +
                        '<i class="bi ' + estilo.icone + '"></i>' +
                        '<span>' + escaparHtml(mensagem) + '</span>' +
                    '</div>' +
                    '<button type="button" class="btn-close btn-close-white me-2 m-auto" ' +
                    'data-bs-dismiss="toast" aria-label="Fechar"></button>' +
                '</div>' +
            '</div>');

        $('#areaToasts').append(elemento);

        var instancia = new bootstrap.Toast(elemento[0], {
            delay: tipo === 'erro' ? 7000 : 4000
        });

        elemento.on('hidden.bs.toast', function () {
            elemento.remove();
        });

        instancia.show();
    }

    // -----------------------------------------------------------------
    // Estado de botao durante requisicao
    // -----------------------------------------------------------------

    /** Bloqueia o botão durante a chamada. Duplo clique é a causa mais comum
        de registro duplicado em formulário AJAX. */
    function ocuparBotao($botao, textoOcupado) {
        if ($botao.data('ocupado')) {
            return false;
        }

        $botao.data('ocupado', true);
        $botao.data('conteudo-original', $botao.html());
        $botao.prop('disabled', true).html(
            '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>' +
            escaparHtml(textoOcupado || 'Aguarde...'));

        return true;
    }

    function liberarBotao($botao) {
        $botao.data('ocupado', false);
        $botao.prop('disabled', false).html($botao.data('conteudo-original'));
    }

    return {
        requisitar: requisitar,
        interpretarErro: interpretarErro,
        toast: toast,
        escaparHtml: escaparHtml,
        formatarMoeda: formatarMoeda,
        formatarData: formatarData,
        formatarNumeroParaCampo: formatarNumeroParaCampo,
        converterParaNumero: converterParaNumero,
        ocuparBotao: ocuparBotao,
        liberarBotao: liberarBotao
    };
})(jQuery);
