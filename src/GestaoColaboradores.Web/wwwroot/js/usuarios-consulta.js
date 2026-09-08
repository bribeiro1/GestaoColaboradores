/* Tela 1: consultar e excluir. Todas as operações por jQuery.ajax; a página
   nunca recarrega. */

(function ($, app) {
    'use strict';

    var URL_API = '/api/usuarios';
    var COLUNAS = 5;

    var $corpoTabela = $('#corpoTabela');
    var $resumo = $('#resumoRegistros');
    var $filtroNome = $('#filtroNome');
    var $filtroSituacao = $('#filtroSituacao');

    var modalExclusao = null;
    var idParaExcluir = null;
    var requisicaoEmVoo = null;
    var temporizadorBusca = null;

    // -----------------------------------------------------------------
    // Renderizacao
    // -----------------------------------------------------------------

    function montarLinha(usuario) {
        var situacao = usuario.ativo
            ? '<span class="badge-situacao badge-ativo">Ativo</span>'
            : '<span class="badge-situacao badge-inativo">Inativo</span>';

        var rotuloAlternar = usuario.ativo ? 'Inativar' : 'Ativar';
        var iconeAlternar = usuario.ativo ? 'bi-toggle-on' : 'bi-toggle-off';

        // Todo dado vindo do servidor passa por escaparHtml antes de virar HTML.
        var nome = app.escaparHtml(usuario.nome);

        return '' +
            '<tr data-id="' + usuario.id + '">' +
                '<td class="nome-usuario">' + nome + '</td>' +
                '<td class="col-numerica">' + app.formatarMoeda(usuario.valorHora) + '</td>' +
                '<td>' + app.formatarData(usuario.dataCadastro) + '</td>' +
                '<td>' + situacao + '</td>' +
                '<td class="col-acoes">' +
                    '<a class="btn-acao acao-editar" href="/Usuarios/Form/' + usuario.id + '" ' +
                       'title="Editar" aria-label="Editar ' + nome + '">' +
                        '<i class="bi bi-pencil"></i>' +
                    '</a>' +
                    '<button type="button" class="btn-acao js-alternar" ' +
                            'data-ativo="' + usuario.ativo + '" ' +
                            'title="' + rotuloAlternar + '" aria-label="' + rotuloAlternar + ' ' + nome + '">' +
                        '<i class="bi ' + iconeAlternar + '"></i>' +
                    '</button>' +
                    '<button type="button" class="btn-acao acao-excluir js-excluir" ' +
                            'data-nome="' + nome + '" ' +
                            'title="Excluir" aria-label="Excluir ' + nome + '">' +
                        '<i class="bi bi-trash"></i>' +
                    '</button>' +
                '</td>' +
            '</tr>';
    }

    function mostrarCarregando() {
        var linhas = '';
        for (var i = 0; i < 4; i++) {
            linhas += '<tr>';
            for (var c = 0; c < COLUNAS; c++) {
                linhas += '<td><div class="skeleton-linha"></div></td>';
            }
            linhas += '</tr>';
        }
        $corpoTabela.html(linhas);
        $resumo.text('Carregando...');
    }

    function mostrarVazio() {
        var houveFiltro = !!($filtroNome.val() || $filtroSituacao.val());

        var conteudo = houveFiltro
            ? '<i class="bi bi-search"></i>' +
              '<h2>Nenhum resultado</h2>' +
              '<p>Nenhum usuário corresponde aos filtros informados.</p>' +
              '<button type="button" class="btn btn-sm btn-outline-secondary" id="btnLimparVazio">' +
              'Limpar filtros</button>'
            : '<i class="bi bi-people"></i>' +
              '<h2>Nenhum usuário cadastrado</h2>' +
              '<p>Cadastre o primeiro usuário para começar.</p>' +
              '<a class="btn btn-sm btn-primary" href="/Usuarios/Form">' +
              '<i class="bi bi-plus-lg"></i> Novo usuário</a>';

        $corpoTabela.html(
            '<tr><td colspan="' + COLUNAS + '"><div class="estado-bloco">' + conteudo + '</div></td></tr>');
        $resumo.text('Nenhum registro');
    }

    function mostrarErro(mensagem) {
        $corpoTabela.html(
            '<tr><td colspan="' + COLUNAS + '"><div class="estado-bloco">' +
                '<i class="bi bi-wifi-off"></i>' +
                '<h2>Não foi possível carregar</h2>' +
                '<p>' + app.escaparHtml(mensagem) + '</p>' +
                '<button type="button" class="btn btn-sm btn-outline-secondary" id="btnTentarNovamente">' +
                    '<i class="bi bi-arrow-clockwise"></i> Tentar novamente</button>' +
            '</div></td></tr>');
        $resumo.text('');
    }

    // -----------------------------------------------------------------
    // Consulta
    // -----------------------------------------------------------------

    function consultar() {
        // Cancela a consulta anterior. Sem isso, ao digitar rápido, uma
        // resposta antiga pode chegar depois da nova e sobrescrever o grid.
        if (requisicaoEmVoo) {
            requisicaoEmVoo.abort();
        }

        mostrarCarregando();

        var parametros = {};
        var nome = $.trim($filtroNome.val());
        var situacao = $filtroSituacao.val();

        if (nome) {
            parametros.nome = nome;
        }
        if (situacao !== '') {
            parametros.ativo = situacao;
        }

        requisicaoEmVoo = app.requisitar({ url: URL_API, parametros: parametros });

        requisicaoEmVoo
            .done(function (usuarios) {
                if (!usuarios || usuarios.length === 0) {
                    mostrarVazio();
                    return;
                }

                $corpoTabela.html(usuarios.map(montarLinha).join(''));
                $resumo.text(usuarios.length === 1
                    ? '1 registro encontrado'
                    : usuarios.length + ' registros encontrados');
            })
            .fail(function (jqXHR, textStatus) {
                if (textStatus === 'abort') {
                    return;
                }
                var erro = app.interpretarErro(jqXHR);
                mostrarErro(erro.mensagem);
            })
            .always(function () {
                requisicaoEmVoo = null;
            });
    }

    // -----------------------------------------------------------------
    // Exclusao
    // -----------------------------------------------------------------

    function abrirConfirmacaoExclusao(id, nome) {
        idParaExcluir = id;
        $('#nomeUsuarioExclusao').text(nome);
        modalExclusao.show();
    }

    function confirmarExclusao() {
        var $botao = $('#btnConfirmarExclusao');

        if (!app.ocuparBotao($botao, 'Excluindo...')) {
            return;
        }

        var $linha = $corpoTabela.find('tr[data-id="' + idParaExcluir + '"]');
        $linha.addClass('linha-saindo');

        app.requisitar({ url: URL_API + '/' + idParaExcluir, metodo: 'DELETE' })
            .done(function () {
                modalExclusao.hide();
                app.toast('Usuário excluído com sucesso.', 'sucesso');
                consultar();
            })
            .fail(function (jqXHR) {
                $linha.removeClass('linha-saindo');
                var erro = app.interpretarErro(jqXHR);

                // 404 aqui quase sempre é grid desatualizado.
                if (erro.status === 404) {
                    modalExclusao.hide();
                    app.toast('Este usuário já havia sido excluído.', 'alerta');
                    consultar();
                    return;
                }

                app.toast(erro.mensagem, 'erro');
            })
            .always(function () {
                app.liberarBotao($botao);
            });
    }

    // -----------------------------------------------------------------
    // Ativar / inativar
    // -----------------------------------------------------------------

    function alternarSituacao($botao, id, ativoAtual) {
        var novoEstado = !ativoAtual;

        $botao.prop('disabled', true);

        app.requisitar({
            url: URL_API + '/' + id + '/situacao',
            metodo: 'PATCH',
            corpo: { ativo: novoEstado }
        })
            .done(function () {
                app.toast(novoEstado ? 'Usuário ativado.' : 'Usuário inativado.', 'sucesso');
                consultar();
            })
            .fail(function (jqXHR) {
                $botao.prop('disabled', false);
                app.toast(app.interpretarErro(jqXHR).mensagem, 'erro');
            });
    }

    // -----------------------------------------------------------------
    // Eventos
    // -----------------------------------------------------------------

    function limparFiltros() {
        $filtroNome.val('');
        $filtroSituacao.val('');
        consultar();
    }

    $(function () {
        modalExclusao = new bootstrap.Modal(document.getElementById('modalExclusao'));

        $('#btnFiltrar').on('click', consultar);
        $('#btnAtualizar').on('click', consultar);
        $('#btnLimparFiltro').on('click', limparFiltros);

        // Debounce: sem ele, cada tecla dispararia uma requisição.
        $filtroNome.on('input', function () {
            clearTimeout(temporizadorBusca);
            temporizadorBusca = setTimeout(consultar, 400);
        });

        $filtroNome.on('keydown', function (evento) {
            if (evento.key === 'Enter') {
                evento.preventDefault();
                clearTimeout(temporizadorBusca);
                consultar();
            }
        });

        $filtroSituacao.on('change', consultar);

        // Delegação: as linhas são recriadas a cada consulta, então o
        // handler fica no container, que é permanente.
        $corpoTabela
            .on('click', '.js-excluir', function () {
                var $linha = $(this).closest('tr');
                abrirConfirmacaoExclusao($linha.data('id'), $(this).data('nome'));
            })
            .on('click', '.js-alternar', function () {
                var $botao = $(this);
                var $linha = $botao.closest('tr');
                alternarSituacao($botao, $linha.data('id'), $botao.data('ativo') === true);
            })
            .on('click', '#btnTentarNovamente', consultar)
            .on('click', '#btnLimparVazio', limparFiltros);

        $('#btnConfirmarExclusao').on('click', confirmarExclusao);

        $('#modalExclusao').on('hidden.bs.modal', function () {
            idParaExcluir = null;
            $corpoTabela.find('tr').removeClass('linha-saindo');
        });

        consultar();
    });
})(jQuery, window.app);

/* Mensagem de sucesso deixada pela tela de formulario antes de navegar. */
(function ($, app) {
    'use strict';

    $(function () {
        var mensagem = sessionStorage.getItem('mensagemSucesso');
        if (mensagem) {
            sessionStorage.removeItem('mensagemSucesso');
            app.toast(mensagem, 'sucesso');
        }
    });
})(jQuery, window.app);
