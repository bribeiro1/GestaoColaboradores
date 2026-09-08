using GestaoColaboradores.Domain.Entities;

namespace GestaoColaboradores.Tests.Builders;

/// <summary>
/// Test Data Builder: cada teste declara apenas o que importa para o cenário
/// que verifica, e um campo novo não obriga a alterar dezenas de arquivos.
/// </summary>
public sealed class UsuarioBuilder
{
    public static readonly DateTimeOffset DataReferencia =
        new(2026, 9, 5, 12, 0, 0, TimeSpan.Zero);

    private string _nome = "Usuario de Teste";
    private decimal _valorHora = 100m;
    private DateTime _dataCadastro = new(2026, 1, 10);
    private bool _ativo = true;
    private TimeProvider _clock = new RelogioFake(DataReferencia);

    public UsuarioBuilder ComNome(string nome)
    {
        _nome = nome;
        return this;
    }

    public UsuarioBuilder ComValorHora(decimal valorHora)
    {
        _valorHora = valorHora;
        return this;
    }

    public UsuarioBuilder ComDataCadastro(DateTime dataCadastro)
    {
        _dataCadastro = dataCadastro;
        return this;
    }

    public UsuarioBuilder Inativo()
    {
        _ativo = false;
        return this;
    }

    public UsuarioBuilder ComRelogio(TimeProvider clock)
    {
        _clock = clock;
        return this;
    }

    public Usuario Construir() =>
        Usuario.Criar(_nome, _valorHora, _dataCadastro, _ativo, _clock);
}
