namespace GestaoColaboradores.Application.Common;

/// <summary>
/// Natureza da falha de um caso de uso. Existe para a camada Web traduzir
/// em status HTTP sem interpretar mensagem de texto.
/// </summary>
public enum TipoErro
{
    Nenhum = 0,

    /// <summary>Entrada inválida. -&gt; 400</summary>
    Validacao = 1,

    /// <summary>Registro inexistente. -&gt; 404</summary>
    NaoEncontrado = 2,

    /// <summary>Conflito com o estado atual. -&gt; 409</summary>
    Conflito = 3
}
