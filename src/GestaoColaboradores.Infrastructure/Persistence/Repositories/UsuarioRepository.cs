using GestaoColaboradores.Domain.Entities;
using GestaoColaboradores.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.Infrastructure.Persistence.Repositories;

/// <summary>Repositório de Usuario sobre EF Core.</summary>
public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Usuario>> ListarAsync(
        string? nome,
        bool? ativo,
        CancellationToken cancellationToken = default)
    {
        // AsNoTracking: consulta somente leitura, não há por que o change
        // tracker criar snapshot de cada entidade.
        var query = _context.Usuarios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var termo = EscaparCuringasLike(nome.Trim());

            // A comparação case-insensitive vem da collation do banco, e não
            // de um ToLower() no C#: aplicar função sobre a coluna tornaria o
            // predicado non-sargable e impediria o uso de índice.
            query = query.Where(u => EF.Functions.Like(u.Nome, $"%{termo}%"));
        }

        if (ativo.HasValue)
        {
            query = query.Where(u => u.Ativo == ativo.Value);
        }

        return await query
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Sem AsNoTracking: o retorno alimenta uma alteração, e o change tracker
    /// gera o UPDATE apenas das colunas que mudaram.
    /// </summary>
    public async Task<Usuario?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExisteComNomeAsync(
        string nome,
        int? idIgnorado = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return false;
        }

        var termo = nome.Trim();

        // AnyAsync gera EXISTS: o banco para na primeira linha encontrada,
        // sem materializar a entidade.
        return await _context.Usuarios
            .AsNoTracking()
            .AnyAsync(
                u => u.Nome == termo && (idIgnorado == null || u.Id != idIgnorado),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken).ConfigureAwait(false);
    }

    public void Remover(Usuario usuario)
    {
        _context.Usuarios.Remove(usuario);
    }

    /// <summary>
    /// Neutraliza os curingas do LIKE vindos da entrada do usuário: digitar
    /// "%" no filtro retornaria a tabela inteira. Não é questão de injeção
    /// (o EF parametriza), é o filtro fazer o que o usuário espera.
    /// </summary>
    private static string EscaparCuringasLike(string termo) => termo
        .Replace("[", "[[]", StringComparison.Ordinal)
        .Replace("%", "[%]", StringComparison.Ordinal)
        .Replace("_", "[_]", StringComparison.Ordinal);
}
