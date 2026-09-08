namespace GestaoColaboradores.Tests.Builders;

/// <summary>
/// TimeProvider determinístico. Com DateTime.Now no domínio, o teste
/// dependeria do relógio da máquina - passaria hoje e poderia falhar na
/// virada do dia ou no fuso do agente de build.
/// </summary>
public sealed class RelogioFake : TimeProvider
{
    private DateTimeOffset _agora;

    public RelogioFake(DateTimeOffset agora)
    {
        _agora = agora;
    }

    public override DateTimeOffset GetUtcNow() => _agora;

    /// <summary>UTC fixo: elimina o fuso da maquina como variavel do teste.</summary>
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public void Avancar(TimeSpan intervalo) => _agora = _agora.Add(intervalo);
}
