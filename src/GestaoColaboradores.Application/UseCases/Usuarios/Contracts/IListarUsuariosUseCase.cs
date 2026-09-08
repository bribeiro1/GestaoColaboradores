using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.DTOs;

namespace GestaoColaboradores.Application.UseCases.Usuarios.Contracts;

/// <summary>
/// Consulta a lista de usuarios com filtros opcionais.
/// </summary>
public interface IListarUsuariosUseCase
{
    Task<Result<IReadOnlyList<UsuarioDto>>> ExecutarAsync(
        FiltroUsuarioRequest filtro,
        CancellationToken cancellationToken = default);
}
