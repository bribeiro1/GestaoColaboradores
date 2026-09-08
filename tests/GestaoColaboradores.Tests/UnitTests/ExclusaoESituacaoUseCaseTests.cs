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

public sealed class ExclusaoESituacaoUseCaseTests
{
    private readonly IUsuarioRepository _repositorio = Substitute.For<IUsuarioRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Excluir_QuandoUsuarioNaoExiste_DeveRetornarNaoEncontrado()
    {
        _repositorio.ObterPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var resultado = await new ExcluirUsuarioUseCase(_repositorio, _unitOfWork)
            .ExecutarAsync(99);

        Assert.True(resultado.Falhou);
        Assert.Equal(TipoErro.NaoEncontrado, resultado.TipoErro);
        _repositorio.DidNotReceive().Remover(Arg.Any<Usuario>());
    }

    [Fact]
    public async Task Excluir_QuandoUsuarioExiste_DeveRemoverEConfirmar()
    {
        var usuario = new UsuarioBuilder().Construir();
        _repositorio.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(usuario);

        var resultado = await new ExcluirUsuarioUseCase(_repositorio, _unitOfWork)
            .ExecutarAsync(1);

        Assert.True(resultado.Sucesso);
        _repositorio.Received(1).Remover(usuario);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AlterarSituacao_ParaInativo_DeveInativarSemRemoverORegistro()
    {
        var usuario = new UsuarioBuilder().Construir();
        _repositorio.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(usuario);

        var resultado = await new AlterarSituacaoUsuarioUseCase(_repositorio, _unitOfWork)
            .ExecutarAsync(1, ativo: false);

        Assert.True(resultado.Sucesso);
        Assert.False(usuario.Ativo);

        // A diferenca entre inativar e excluir: o registro continua existindo.
        _repositorio.DidNotReceive().Remover(Arg.Any<Usuario>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AlterarSituacao_ParaAtivo_DeveReativarUsuarioInativo()
    {
        var usuario = new UsuarioBuilder().Inativo().Construir();
        _repositorio.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(usuario);

        var resultado = await new AlterarSituacaoUsuarioUseCase(_repositorio, _unitOfWork)
            .ExecutarAsync(1, ativo: true);

        Assert.True(resultado.Sucesso);
        Assert.True(usuario.Ativo);
    }

    [Fact]
    public async Task Listar_DeveRepassarOsFiltrosParaORepositorio()
    {
        IReadOnlyList<Usuario> retorno = new List<Usuario> { new UsuarioBuilder().Construir() };

        _repositorio
            .ListarAsync(Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(retorno);

        var filtro = new FiltroUsuarioRequest { Nome = "ana", Ativo = true };

        var resultado = await new ListarUsuariosUseCase(_repositorio).ExecutarAsync(filtro);

        Assert.True(resultado.Sucesso);
        Assert.Single(resultado.Valor!);

        // O filtro precisa chegar ao repositorio para virar predicado SQL.
        // Se fosse aplicado em memoria, a consulta traria a tabela inteira.
        await _repositorio.Received(1).ListarAsync("ana", true, Arg.Any<CancellationToken>());
    }
}
