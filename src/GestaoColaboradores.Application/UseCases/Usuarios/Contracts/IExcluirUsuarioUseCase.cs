using GestaoColaboradores.Application.Common;

namespace GestaoColaboradores.Application.UseCases.Usuarios.Contracts;

/// <summary>
/// Exclui fisicamente um usuario.
/// </summary>
public interface IExcluirUsuarioUseCase
{
    Task<Result> ExecutarAsync(int id, CancellationToken cancellationToken = default);
}
