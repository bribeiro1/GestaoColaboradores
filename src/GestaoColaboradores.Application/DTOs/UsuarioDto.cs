namespace GestaoColaboradores.Application.DTOs;

/// <summary>
/// Representação de saída de um usuário. A entidade não é serializada
/// diretamente, para o contrato da API poder evoluir sem arrastar o domínio.
///
/// Os valores vão crus, sem formatação: a apresentação decide o locale, e
/// assim o mesmo endpoint serve tela, relatório e integração.
/// </summary>
public sealed record UsuarioDto(
    int Id,
    string Nome,
    decimal ValorHora,
    DateTime DataCadastro,
    bool Ativo);
