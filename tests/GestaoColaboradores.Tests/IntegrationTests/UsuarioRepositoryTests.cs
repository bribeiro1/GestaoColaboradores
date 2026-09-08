using GestaoColaboradores.Domain.Entities;
using GestaoColaboradores.Infrastructure.Persistence;
using GestaoColaboradores.Infrastructure.Persistence.Repositories;
using GestaoColaboradores.Tests.Builders;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestaoColaboradores.Tests.IntegrationTests;

/// <summary>
/// Integração do repositório contra SQLite em memória.
/// </summary>
public sealed class UsuarioRepositoryTests : IDisposable
{
    private readonly SqliteConnection _conexao;
    private readonly AppDbContext _contexto;
    private readonly UsuarioRepository _repositorio;

    public UsuarioRepositoryTests()
    {
        // A conexao precisa ficar ABERTA durante todo o teste: o banco
        // em memoria do SQLite deixa de existir quando ela e fechada.
        _conexao = new SqliteConnection("DataSource=:memory:");
        _conexao.Open();

        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conexao)
            .Options;

        _contexto = new AppDbContext(opcoes);
        _contexto.Database.EnsureCreated();

        _repositorio = new UsuarioRepository(_contexto);
    }

    private async Task SemearAsync()
    {
        _contexto.Usuarios.AddRange(
            new UsuarioBuilder().ComNome("Ana Carolina").ComValorHora(100m).Construir(),
            new UsuarioBuilder().ComNome("Bruno Almeida").ComValorHora(200m).Construir(),
            new UsuarioBuilder().ComNome("Carla Menezes").ComValorHora(300m).Inativo().Construir());

        await _contexto.SaveChangesAsync();
        _contexto.ChangeTracker.Clear();
    }

    [Fact]
    public async Task ListarAsync_SemFiltro_DeveRetornarTodosOrdenadosPorNome()
    {
        await SemearAsync();

        var usuarios = await _repositorio.ListarAsync(null, null);

        Assert.Equal(3, usuarios.Count);
        Assert.Equal("Ana Carolina", usuarios[0].Nome);
        Assert.Equal("Bruno Almeida", usuarios[1].Nome);
        Assert.Equal("Carla Menezes", usuarios[2].Nome);
    }

    [Fact]
    public async Task ListarAsync_ComFiltroDeNomeParcial_DeveTraduzirParaLikeNoBanco()
    {
        await SemearAsync();

        var usuarios = await _repositorio.ListarAsync("Almeida", null);

        Assert.Single(usuarios);
        Assert.Equal("Bruno Almeida", usuarios[0].Nome);
    }

    [Fact]
    public async Task ListarAsync_ComFiltroDeSituacao_DeveRetornarSomenteOsInativos()
    {
        await SemearAsync();

        var usuarios = await _repositorio.ListarAsync(null, ativo: false);

        Assert.Single(usuarios);
        Assert.Equal("Carla Menezes", usuarios[0].Nome);
    }

    [Fact]
    public async Task ExisteComNomeAsync_IgnorandoOProprioId_DeveRetornarFalso()
    {
        await SemearAsync();
        var ana = await _repositorio.ObterPorIdAsync(1);

        var existe = await _repositorio.ExisteComNomeAsync(ana!.Nome, idIgnorado: ana.Id);

        Assert.False(existe);
    }

    [Fact]
    public async Task SaveChanges_ComNomeDuplicado_DeveFalharPeloIndiceUnico()
    {
        // Este e o teste que so um banco relacional de verdade permite:
        // comprova que a integridade nao depende da checagem previa da
        // aplicacao. E a garantia contra a condicao de corrida entre a
        // consulta de duplicidade e o INSERT.
        await SemearAsync();

        _contexto.Usuarios.Add(new UsuarioBuilder().ComNome("Ana Carolina").Construir());

        var excecao = await Assert.ThrowsAsync<DbUpdateException>(
            () => _contexto.SaveChangesAsync());

        Assert.Contains("UNIQUE", excecao.InnerException?.Message ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValorHora_DeveSerPersistidoSemPerdaDePrecisao()
    {
        var usuario = new UsuarioBuilder().ComNome("Teste Precisao").ComValorHora(1234.56m).Construir();
        _contexto.Usuarios.Add(usuario);
        await _contexto.SaveChangesAsync();
        _contexto.ChangeTracker.Clear();

        var recuperado = await _repositorio.ObterPorIdAsync(usuario.Id);

        Assert.Equal(1234.56m, recuperado!.ValorHora);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexao.Dispose();
    }
}
