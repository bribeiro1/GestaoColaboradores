using System.Globalization;
using GestaoColaboradores.Domain.Entities;
using GestaoColaboradores.Domain.Exceptions;
using GestaoColaboradores.Tests.Builders;
using Xunit;

namespace GestaoColaboradores.Tests.UnitTests;

/// <summary>
/// Invariantes do agregado Usuario - a regra verificada no único lugar onde
/// ela vale para todos os caminhos de entrada.
///
/// Asserts nativos do xUnit, sem FluentAssertions: a v8 passou a exigir
/// licença paga para uso comercial.
/// </summary>
public sealed class UsuarioTests
{
    private static readonly TimeProvider Relogio = new RelogioFake(UsuarioBuilder.DataReferencia);

    [Fact]
    public void Criar_ComDadosValidos_DevePreencherTodasAsPropriedades()
    {
        var dataCadastro = new DateTime(2026, 3, 10);

        var usuario = Usuario.Criar("Ana Souza", 150.00m, dataCadastro, true, Relogio);

        Assert.Equal("Ana Souza", usuario.Nome);
        Assert.Equal(150.00m, usuario.ValorHora);
        Assert.Equal(dataCadastro, usuario.DataCadastro);
        Assert.True(usuario.Ativo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Criar_ComNomeVazio_DeveLancarDomainException(string? nomeInvalido)
    {
        var excecao = Assert.Throws<DomainException>(() =>
            new UsuarioBuilder().ComNome(nomeInvalido!).Construir());

        Assert.Contains("obrigatório", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Criar_ComNomeAbaixoDoMinimo_DeveLancarDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new UsuarioBuilder().ComNome("Jo").Construir());
    }

    [Fact]
    public void Criar_ComNomeAcimaDoMaximo_DeveLancarDomainException()
    {
        var nomeLongo = new string('A', UsuarioLimites.NomeTamanhoMaximo + 1);

        Assert.Throws<DomainException>(() =>
            new UsuarioBuilder().ComNome(nomeLongo).Construir());
    }

    [Fact]
    public void Criar_ComNomeCercadoDeEspacos_DeveArmazenarSemEspacos()
    {
        // Normalizar na entidade evita que "Ana" e " Ana " convivam como
        // registros distintos, furando a regra de nome unico.
        var usuario = new UsuarioBuilder().ComNome("   Ana Souza   ").Construir();

        Assert.Equal("Ana Souza", usuario.Nome);
    }

    // Os valores decimais vao como string e sao convertidos com cultura
    // invariante. InlineData so aceita constantes de tempo de compilacao, e
    // decimal nao e uma delas - passar 0.01 gravaria um double no atributo
    // e a conversao para decimal aconteceria (ou falharia) so em execucao.
    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("-0.01")]
    public void Criar_ComValorHoraNaoPositivo_DeveLancarDomainException(string valorInvalido)
    {
        var valor = decimal.Parse(valorInvalido, CultureInfo.InvariantCulture);

        Assert.Throws<DomainException>(() =>
            new UsuarioBuilder().ComValorHora(valor).Construir());
    }

    [Fact]
    public void Criar_ComValorHoraAcimaDoLimite_DeveLancarDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new UsuarioBuilder()
                .ComValorHora(UsuarioLimites.ValorHoraMaximo + 1m)
                .Construir());
    }

    [Theory]
    [InlineData("10.005", "10.01")]
    [InlineData("10.004", "10.00")]
    [InlineData("99.999", "100.00")]
    public void Criar_ComMaisDeDuasCasasDecimais_DeveArredondarParaAEscalaDaColuna(
        string valorInformado, string valorEsperado)
    {
        var informado = decimal.Parse(valorInformado, CultureInfo.InvariantCulture);
        var esperado = decimal.Parse(valorEsperado, CultureInfo.InvariantCulture);

        // Sem este arredondamento explicito, o SQL Server truncaria o valor
        // ao gravar em decimal(18,2) - perda silenciosa em dado monetario.
        var usuario = new UsuarioBuilder().ComValorHora(informado).Construir();

        Assert.Equal(esperado, usuario.ValorHora);
    }

    [Fact]
    public void Criar_ComDataCadastroFutura_DeveLancarDomainException()
    {
        var amanha = UsuarioBuilder.DataReferencia.Date.AddDays(1);

        var excecao = Assert.Throws<DomainException>(() =>
            new UsuarioBuilder().ComDataCadastro(amanha).Construir());

        Assert.Contains("futura", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Criar_ComDataCadastroHoje_DeveSerAceita()
    {
        var hoje = UsuarioBuilder.DataReferencia.Date;

        var usuario = new UsuarioBuilder().ComDataCadastro(hoje).Construir();

        Assert.Equal(hoje, usuario.DataCadastro);
    }

    [Fact]
    public void Criar_ComDataCadastroNaoInformada_DeveLancarDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new UsuarioBuilder().ComDataCadastro(default).Construir());
    }

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAlterarOEstado()
    {
        var usuario = new UsuarioBuilder().ComNome("Nome Antigo").Construir();
        var novaData = new DateTime(2026, 4, 1);

        usuario.Atualizar("Nome Novo", 250.75m, novaData, false, Relogio);

        Assert.Equal("Nome Novo", usuario.Nome);
        Assert.Equal(250.75m, usuario.ValorHora);
        Assert.Equal(novaData, usuario.DataCadastro);
        Assert.False(usuario.Ativo);
    }

    [Fact]
    public void Atualizar_ComDadosInvalidos_NaoDeveAlterarNenhumCampo()
    {
        // Verifica que a entidade nao fica em estado parcialmente alterado
        // quando a validacao falha no meio da atualizacao.
        var usuario = new UsuarioBuilder().ComNome("Nome Original").ComValorHora(100m).Construir();

        Assert.Throws<DomainException>(() =>
            usuario.Atualizar("X", 250m, new DateTime(2026, 4, 1), false, Relogio));

        Assert.Equal("Nome Original", usuario.Nome);
        Assert.Equal(100m, usuario.ValorHora);
    }

    [Fact]
    public void Inativar_DeveMarcarComoInativoSemPerderOsDemaisDados()
    {
        var usuario = new UsuarioBuilder().ComNome("Ana Souza").Construir();

        usuario.Inativar();

        Assert.False(usuario.Ativo);
        Assert.Equal("Ana Souza", usuario.Nome);
    }

    [Fact]
    public void Ativar_EmUsuarioJaAtivo_DeveSerIdempotente()
    {
        var usuario = new UsuarioBuilder().Construir();

        usuario.Ativar();
        usuario.Ativar();

        Assert.True(usuario.Ativo);
    }
}
