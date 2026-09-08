using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.DTOs;
using GestaoColaboradores.Application.UseCases.Usuarios;
using GestaoColaboradores.Domain.Entities;
using GestaoColaboradores.Domain.Exceptions;
using GestaoColaboradores.Domain.Repositories;
using GestaoColaboradores.Tests.Builders;
using NSubstitute;
using Xunit;

namespace GestaoColaboradores.Tests.UnitTests;

/// <summary>
/// Orquestração do caso de uso de criação: qual regra é checada, em que
/// ordem, e se a transação é confirmada apenas quando deve.
///
/// NSubstitute no lugar de Moq por decisão de dependência: o SponsorLink do
/// Moq coletava e-mail de desenvolvedores em tempo de build.
/// </summary>
public sealed class CriarUsuarioUseCaseTests
{
    private readonly IUsuarioRepository _repositorio = Substitute.For<IUsuarioRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _relogio = new RelogioFake(UsuarioBuilder.DataReferencia);

    private CriarUsuarioUseCase CriarUseCase() => new(_repositorio, _unitOfWork, _relogio);

    private static SalvarUsuarioRequest RequisicaoValida() => new()
    {
        Nome = "Ana Souza",
        ValorHora = 150m,
        DataCadastro = new DateTime(2026, 2, 1),
        Ativo = true
    };

    [Fact]
    public async Task Executar_ComDadosValidos_DeveAdicionarEConfirmarATransacao()
    {
        _repositorio
            .ExisteComNomeAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var resultado = await CriarUseCase().ExecutarAsync(RequisicaoValida());

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Valor);
        Assert.Equal("Ana Souza", resultado.Valor!.Nome);

        await _repositorio.Received(1).AdicionarAsync(
            Arg.Is<Usuario>(u => u.Nome == "Ana Souza"),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Executar_ComNomeJaCadastrado_DeveRetornarConflitoSemGravar()
    {
        _repositorio
            .ExisteComNomeAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var resultado = await CriarUseCase().ExecutarAsync(RequisicaoValida());

        Assert.True(resultado.Falhou);
        Assert.Equal(TipoErro.Conflito, resultado.TipoErro);

        // O ponto central: nada foi gravado nem confirmado.
        await _repositorio.DidNotReceive().AdicionarAsync(
            Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Executar_ComNomeInvalido_DevePropagarDomainExceptionSemConfirmar()
    {
        // A invariante e do agregado, nao do caso de uso: o caso de uso nao
        // reimplementa a validacao, apenas deixa a excecao subir para que o
        // middleware a traduza em HTTP 400.
        _repositorio
            .ExisteComNomeAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var requisicao = RequisicaoValida();
        requisicao.Nome = "A";

        await Assert.ThrowsAsync<DomainException>(() =>
            CriarUseCase().ExecutarAsync(requisicao));

        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Executar_DeveConsultarDuplicidadeComONomeSemEspacos()
    {
        _repositorio
            .ExisteComNomeAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var requisicao = RequisicaoValida();
        requisicao.Nome = "  Ana Souza  ";

        await CriarUseCase().ExecutarAsync(requisicao);

        // Sem o trim, " Ana Souza " nao encontraria o "Ana Souza" existente
        // e a duplicidade so seria barrada pelo indice unico do banco.
        await _repositorio.Received(1).ExisteComNomeAsync(
            "Ana Souza", null, Arg.Any<CancellationToken>());
    }
}
