using GestaoColaboradores.Domain.Entities;

namespace GestaoColaboradores.Domain.Repositories;

/// <summary>
/// Contrato de persistência do agregado Usuario. Declarado no domínio e
/// implementado na infraestrutura, para que o núcleo não conheça o EF.
/// Nenhum método devolve IQueryable: a composição da consulta é
/// responsabilidade do repositório.
/// </summary>
public interface IUsuarioRepository
{
    /// <param name="nome">Filtro parcial. Nulo ou vazio = sem filtro.</param>
    /// <param name="ativo">Filtro por situação. Nulo = ambos.</param>
    Task<IReadOnlyList<Usuario>> ListarAsync(
        string? nome,
        bool? ativo,
        CancellationToken cancellationToken = default);

    /// <summary>Retorna a entidade rastreada, para alteração.</summary>
    Task<Usuario?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <param name="idIgnorado">
    /// Evita que a própria linha seja considerada duplicada durante a edição.
    /// </param>
    Task<bool> ExisteComNomeAsync(
        string nome,
        int? idIgnorado = null,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default);

    void Remover(Usuario usuario);
}
