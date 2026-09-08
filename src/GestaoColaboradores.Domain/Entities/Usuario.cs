using GestaoColaboradores.Domain.Exceptions;

namespace GestaoColaboradores.Domain.Entities;

/// <summary>
/// Agregado Usuario. Setters privados: o estado só muda por métodos que
/// expressam intenção de negócio, então não existe caminho para um Usuario
/// inválido em memória.
/// </summary>
public sealed class Usuario
{
    /// <summary>Exigido pelo EF Core para materializar a entidade.</summary>
    private Usuario()
    {
    }

    private Usuario(string nome, decimal valorHora, DateTime dataCadastro, bool ativo)
    {
        Nome = nome;
        ValorHora = valorHora;
        DataCadastro = dataCadastro;
        Ativo = ativo;
    }

    public int Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public decimal ValorHora { get; private set; }

    public DateTime DataCadastro { get; private set; }

    public bool Ativo { get; private set; }

    /// <param name="clock">
    /// TimeProvider em vez de DateTime.Now: torna a regra de data não futura
    /// determinística no teste, sem depender do relógio da máquina.
    /// </param>
    public static Usuario Criar(
        string nome,
        decimal valorHora,
        DateTime dataCadastro,
        bool ativo,
        TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return new Usuario(
            ValidarNome(nome),
            ValidarValorHora(valorHora),
            ValidarDataCadastro(dataCadastro, clock),
            ativo);
    }

    public void Atualizar(
        string nome,
        decimal valorHora,
        DateTime dataCadastro,
        bool ativo,
        TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        Nome = ValidarNome(nome);
        ValorHora = ValidarValorHora(valorHora);
        DataCadastro = ValidarDataCadastro(dataCadastro, clock);
        Ativo = ativo;
    }

    /// <summary>Idempotente por escolha: reativar quem já está ativo não é erro.</summary>
    public void Ativar() => Ativo = true;

    /// <summary>
    /// Alternativa não destrutiva à exclusão: preserva o registro e o histórico.
    /// Ver a seção de divergências do README.
    /// </summary>
    public void Inativar() => Ativo = false;

    private static string ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome do usuário é obrigatório.");
        }

        // Normaliza aqui para que " Ana " e "Ana" não convivam como
        // registros distintos, furando a regra de nome único.
        var nomeNormalizado = nome.Trim();

        if (nomeNormalizado.Length < UsuarioLimites.NomeTamanhoMinimo)
        {
            throw new DomainException(
                $"O nome deve ter no mínimo {UsuarioLimites.NomeTamanhoMinimo} caracteres.");
        }

        if (nomeNormalizado.Length > UsuarioLimites.NomeTamanhoMaximo)
        {
            throw new DomainException(
                $"O nome deve ter no máximo {UsuarioLimites.NomeTamanhoMaximo} caracteres.");
        }

        return nomeNormalizado;
    }

    private static decimal ValidarValorHora(decimal valorHora)
    {
        if (valorHora < UsuarioLimites.ValorHoraMinimo)
        {
            throw new DomainException(
                $"O valor/hora deve ser maior ou igual a {UsuarioLimites.ValorHoraMinimo:N2}.");
        }

        if (valorHora > UsuarioLimites.ValorHoraMaximo)
        {
            throw new DomainException(
                $"O valor/hora deve ser menor ou igual a {UsuarioLimites.ValorHoraMaximo:N2}.");
        }

        // Sem este arredondamento, o SQL Server truncaria em silêncio ao
        // gravar em decimal(18,2) - perda invisível em dado monetário.
        return decimal.Round(valorHora, UsuarioLimites.ValorHoraEscala, MidpointRounding.AwayFromZero);
    }

    private static DateTime ValidarDataCadastro(DateTime dataCadastro, TimeProvider clock)
    {
        if (dataCadastro == default)
        {
            throw new DomainException("A data de cadastro é obrigatória.");
        }

        if (dataCadastro.Date > clock.GetLocalNow().Date)
        {
            throw new DomainException("A data de cadastro não pode ser futura.");
        }

        return dataCadastro;
    }
}
