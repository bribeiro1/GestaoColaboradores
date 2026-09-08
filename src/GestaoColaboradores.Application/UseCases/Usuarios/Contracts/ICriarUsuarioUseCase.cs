using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.DTOs;

namespace GestaoColaboradores.Application.UseCases.Usuarios.Contracts;

/// <summary>
/// Cria um novo usuario.
/// </summary>
public interface ICriarUsuarioUseCase
{
    Task<Result<UsuarioDto>> ExecutarAsync(
        SalvarUsuarioRequest request,
        CancellationToken cancellationToken = default);
}
