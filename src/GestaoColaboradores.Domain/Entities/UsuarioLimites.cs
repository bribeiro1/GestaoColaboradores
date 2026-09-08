namespace GestaoColaboradores.Domain.Entities;

/// <summary>
/// Limites das invariantes de Usuario. Consumidos pela entidade, pela
/// configuração do EF e pelos DataAnnotations do DTO - fonte única para
/// que a regra não divergir entre os três.
/// </summary>
public static class UsuarioLimites
{
    public const int NomeTamanhoMaximo = 150;
    public const int NomeTamanhoMinimo = 3;

    public const int ValorHoraPrecisao = 18;
    public const int ValorHoraEscala = 2;

    public const decimal ValorHoraMinimo = 0.01m;
    public const decimal ValorHoraMaximo = 9_999_999.99m;
}
