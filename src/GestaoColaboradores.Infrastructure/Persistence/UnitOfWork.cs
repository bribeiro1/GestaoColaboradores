using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Domain.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.Infrastructure.Persistence;

/// <summary>
/// Confirma as alterações pendentes e traduz erro de infraestrutura em erro
/// de negócio: uma violação de índice único chegaria como DbUpdateException
/// e viraria HTTP 500 se subisse crua.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    // 2601 = chave duplicada em índice único; 2627 = violação de constraint.
    private const int ErroChaveDuplicada = 2601;
    private const int ErroViolacaoDeConstraintUnica = 2627;

    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException ex) when (EhViolacaoDeUnicidade(ex))
        {
            throw new ConflitoDePersistenciaException(
                "Já existe um usuário cadastrado com esse nome.",
                ex);
        }
    }

    private static bool EhViolacaoDeUnicidade(DbUpdateException ex)
    {
        if (ex.InnerException is SqlException sqlException)
        {
            return sqlException.Number is ErroChaveDuplicada or ErroViolacaoDeConstraintUnica;
        }

        // Caminho do SQLite nos testes de integração, verificado pela mensagem
        // para não trazer o pacote como dependência de produção.
        return ex.InnerException?.Message.Contains(
            "UNIQUE constraint failed",
            StringComparison.OrdinalIgnoreCase) == true;
    }
}
