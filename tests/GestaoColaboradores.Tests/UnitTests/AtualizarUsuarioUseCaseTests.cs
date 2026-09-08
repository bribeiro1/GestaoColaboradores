using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.DTOs;
using GestaoColaboradores.Application.UseCases.Usuarios;
using GestaoColaboradores.Application.UseCases.Usuarios.Contracts;
using GestaoColaboradores.Domain.Entities;
using GestaoColaboradores.Domain.Repositories;
using GestaoColaboradores.Tests.Builders;
using NSubstitute;
using Xunit;

namespace GestaoColaboradores.Tests.UnitTests;

public sealed class AtualizarUsuarioUseCaseTests
{
    private readonly IUsuarioRepository _repositorio = Substitute.For<IUsuarioRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _relogio = new RelogioFake(UsuarioBuilder.DataReferencia);

    private AtualizarUsuarioUseCase CriarUseCase() => new(_repositorio, _unitOfWork, _relogio);

    private static SalvarUsuarioRequest RequisicaoValida() => new()
    {
        Id = 1,
        Nome = "Nome Atualizado",
        ValorHora = 200m,
        DataCadastro = new DateTime(2026, 2, 1),
        Ativo = false
    };

    [Fact]
    public async Task Executar_QuandoUsuarioNaoExiste_DeveRetornarNaoEncontrado()
    {
        _repositorio.ObterPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var resultado = await CriarUseCase().ExecutarAsync(99, RequisicaoValida());

        Assert.True(resultado.Falhou);
        Assert.Equal(TipoErro.NaoEncontrado, resultado.TipoErro);
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Executar_ComNomeDeOutroUsuario_DeveRetornarConflito()
    {
        var existente = new UsuarioBuilder().ComNome("Nome Original").Construir();
        _repositorio.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(existente);
        _repositorio
            .ExisteComNomeAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var resultado = await CriarUseCase().ExecutarAsync(1, RequisicaoValida());

        Assert.True(resultado.Falhou);
        Assert.Equal(TipoErro.Conflito, resultado.TipoErro);

        // A entidade nao pode ter sido alterada em memoria antes da recusa.
        Assert.Equal("Nome Original", existente.Nome);
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Executar_DeveIgnorarOProprioRegistroNaChecagemDeDuplicidade()
    {
        // Sem o idIgnorado, salvar um usuario sem trocar o nome acusaria
        // duplicidade contra ele mesmo e a edicao ficaria impossivel.
        var existente = new UsuarioBuilder().ComNome("Nome Original").Construir();
        _repositorio.ObterPorIdAsync(7, Arg.Any<CancellationToken>()).Returns(existente);
        _repositorio
            .ExisteComNomeAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        await CriarUseCase().ExecutarAsync(7, RequisicaoValida());

        await _repositorio.Received(1).ExisteComNomeAsync(
            "Nome Atualizado", 7, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Executar_ComDadosValidos_DeveAlterarAEntidadeEConfirmar()
    {
        var existente = new UsuarioBuilder().ComNome("Nome Original").Construir();
        _repositorio.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(existente);
        _repositorio
            .ExisteComNomeAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var resultado = await CriarUseCase().ExecutarAsync(1, RequisicaoValida());

        Assert.True(resultado.Sucesso);
        Assert.Equal("Nome Atualizado", existente.Nome);
        Assert.Equal(200m, existente.ValorHora);
        Assert.False(existente.Ativo);

        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
