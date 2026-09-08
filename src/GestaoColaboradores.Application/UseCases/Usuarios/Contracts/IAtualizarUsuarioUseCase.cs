using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.DTOs;

namespace GestaoColaboradores.Application.UseCases.Usuarios.Contracts;

/// <summary>
/// Atualiza um usuario existente.
/// </summary>
public interface IAtualizarUsuarioUseCase
{
    Task<Result<UsuarioDto>> ExecutarAsync(
        int id,
        SalvarUsuarioRequest request,
        CancellationToken cancellationToken = default);
}
