namespace GestaoColaboradores.Application.Common;

/// <summary>
/// Resultado de um caso de uso.
///
/// "Não encontrado" e "nome duplicado" são fluxos previstos do negócio, não
/// situações excepcionais - modelá-los com exceção faria a assinatura do
/// método mentir e não obrigaria quem chama a tratar. Exceção fica reservada
/// à invariante de agregado violada (ver DomainException).
/// </summary>
public class Result
{
    protected Result(bool sucesso, string? erro, TipoErro tipoErro)
    {
        Sucesso = sucesso;
        Erro = erro;
        TipoErro = tipoErro;
    }

    public bool Sucesso { get; }

    public bool Falhou => !Sucesso;

    public string? Erro { get; }

    public TipoErro TipoErro { get; }

    public static Result Ok() => new(true, null, TipoErro.Nenhum);

    public static Result Invalido(string erro) => new(false, erro, TipoErro.Validacao);

    public static Result NaoEncontrado(string erro) => new(false, erro, TipoErro.NaoEncontrado);

    public static Result Conflito(string erro) => new(false, erro, TipoErro.Conflito);
}

/// <summary>
/// Resultado que carrega um valor em caso de sucesso.
///
/// Único arquivo da solução com dois tipos públicos, por convenção: Result e
/// Result&lt;T&gt; são o mesmo conceito em duas aridades, como Task e Task&lt;T&gt;.
/// </summary>
public sealed class Result<T> : Result
{
    private Result(bool sucesso, T? valor, string? erro, TipoErro tipoErro)
        : base(sucesso, erro, tipoErro)
    {
        Valor = valor;
    }

    public T? Valor { get; }

    public static Result<T> Ok(T valor) => new(true, valor, null, TipoErro.Nenhum);

    public static new Result<T> Invalido(string erro) => new(false, default, erro, TipoErro.Validacao);

    public static new Result<T> NaoEncontrado(string erro) => new(false, default, erro, TipoErro.NaoEncontrado);

    public static new Result<T> Conflito(string erro) => new(false, default, erro, TipoErro.Conflito);
}
