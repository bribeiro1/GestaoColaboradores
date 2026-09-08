namespace GestaoColaboradores.Domain.Repositories;

/// <summary>
/// Confirma as alterações pendentes como uma única transação.
/// Fica fora do repositório para que o caso de uso decida o limite da
/// transação, e não cada repositório por conta própria.
/// </summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
