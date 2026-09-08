/* Tela 2: cadastrar e editar. POST para criar, PUT para editar; os erros do
   servidor são aplicados nos campos correspondentes. */

(function ($, app) {
    'use strict';

    var URL_API = '/api/usuarios';

    var $formulario = $('#formUsuario');
    var $botaoSalvar = $('#btnSalvar');
    var $campoValor = $('#campoValorHora');
    var $resumoErros = $('#resumoErros');

    // Os validadores number/range do jquery.validate assumem ponto decimal.
    // Sem esta adaptação, "1.234,56" seria reprovado no navegador.

    function paraNumeroPtBr(valor) {
        return app.converterParaNumero(valor);
    }

    if ($.validator) {
        $.validator.methods.number = function (valor, elemento) {
            return this.optional(elemento) || !isNaN(paraNumeroPtBr(valor));
        };

        $.validator.methods.range = function (valor, elemento, parametros) {
            var numero = paraNumeroPtBr(valor);
            return this.optional(elemento) ||
                (!isNaN(numero) && numero >= parametros[0] && numero <= parametros[1]);
        };
    }

    // -----------------------------------------------------------------
    // Campo de valor: formatacao ao sair do campo
    // -----------------------------------------------------------------

    function normalizarCampoValor() {
        var numero = app.converterParaNumero($campoValor.val());

        if (!isNaN(numero)) {
            $campoValor.val(app.formatarNumeroParaCampo(numero));
        }
    }

    // -----------------------------------------------------------------
    // Erros vindos do servidor
    // -----------------------------------------------------------------

    function limparErrosServidor() {
        $resumoErros.addClass('d-none').empty();
        $formulario.find('.form-control, .form-select')
            .removeClass('input-validation-error');
    }

    /** Aplica o dicionário "errors" do ValidationProblemDetails nos campos.
        O servidor é a autoridade final: o erro dele precisa aparecer no campo
        certo, não como aviso genérico. */
    function aplicarErrosDeCampo(porCampo) {
        var mensagensGerais = [];

        Object.keys(porCampo).forEach(function (campo) {
            var mensagens = porCampo[campo];
            if (!mensagens || !mensagens.length) {
                return;
            }

            // "Nome" ou "$.nome", conforme a origem do erro.
            var nomeLimpo = campo.replace(/^\$\./, '');
            var $campo = $formulario.find(
                '[name="' + nomeLimpo + '"], [name="' + capitalizar(nomeLimpo) + '"]');

            if ($campo.length) {
                $campo.addClass('input-validation-error');
                $campo.closest('.mb-3, .col-12')
                    .find('.field-validation-error')
                    .text(mensagens[0]);
            } else {
                mensagensGerais.push(mensagens[0]);
            }
        });

        if (mensagensGerais.length) {
            $resumoErros
                .removeClass('d-none')
                .html('<ul class="mb-0 ps-3">' +
                    mensagensGerais.map(function (m) {
                        return '<li>' + app.escaparHtml(m) + '</li>';
                    }).join('') +
                    '</ul>');
        }
    }

    function capitalizar(texto) {
        return texto.charAt(0).toUpperCase() + texto.slice(1);
    }

    function mostrarErroGeral(mensagem) {
        $resumoErros
            .removeClass('d-none')
            .html('<i class="bi bi-exclamation-triangle me-1"></i>' + app.escaparHtml(mensagem));
    }

    // -----------------------------------------------------------------
    // Envio
    // -----------------------------------------------------------------

    function montarCorpo() {
        return {
            id: parseInt($formulario.find('[name="Id"]').val(), 10) || 0,
            nome: $.trim($formulario.find('[name="Nome"]').val()),
            // Converte o texto pt-BR em numero: JSON e sempre invariante.
            valorHora: app.converterParaNumero($campoValor.val()),
            dataCadastro: $formulario.find('[name="DataCadastro"]').val(),
            ativo: $formulario.find('[name="Ativo"]').is(':checked')
        };
    }

    function salvar() {
        limparErrosServidor();

        // Conveniência de UX, não segurança: o servidor revalida tudo.
        if (!$formulario.valid()) {
            return;
        }

        if (!app.ocuparBotao($botaoSalvar, 'Salvando...')) {
            return;
        }

        var corpo = montarCorpo();
        var edicao = corpo.id > 0;

        app.requisitar({
            url: edicao ? URL_API + '/' + corpo.id : URL_API,
            metodo: edicao ? 'PUT' : 'POST',
            corpo: corpo
        })
            .done(function () {
                // A mensagem e entregue pela proxima tela, ja que a
                // navegacao acontece em seguida.
                sessionStorage.setItem(
                    'mensagemSucesso',
                    edicao ? 'Usuário atualizado com sucesso.' : 'Usuário cadastrado com sucesso.');

                window.location.href = '/Usuarios/Index';
            })
            .fail(function (jqXHR) {
                app.liberarBotao($botaoSalvar);

                var erro = app.interpretarErro(jqXHR);

                if (erro.porCampo) {
                    aplicarErrosDeCampo(erro.porCampo);
                    app.toast('Verifique os campos destacados.', 'alerta');
                    return;
                }

                mostrarErroGeral(erro.mensagem);
                app.toast(erro.mensagem, 'erro');
            });
    }

    // -----------------------------------------------------------------
    // Inicializacao
    // -----------------------------------------------------------------

    $(function () {
        normalizarCampoValor();

        $campoValor.on('blur', normalizarCampoValor);

        // Ao focar, remove a formatacao para facilitar a edicao do numero.
        $campoValor.on('focus', function () {
            var numero = app.converterParaNumero($(this).val());
            if (!isNaN(numero)) {
                $(this).val(String(numero).replace('.', ','));
            }
            $(this).select();
        });

        $formulario.on('submit', function (evento) {
            evento.preventDefault();
            salvar();
        });

        // Limpa a marcacao de erro do servidor assim que o usuario corrige.
        $formulario.on('input change', '.form-control, .form-check-input', function () {
            $(this).removeClass('input-validation-error');
        });
    });
})(jQuery, window.app);
