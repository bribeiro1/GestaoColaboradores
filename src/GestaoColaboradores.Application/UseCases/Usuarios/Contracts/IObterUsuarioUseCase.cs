using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.DTOs;

namespace GestaoColaboradores.Application.UseCases.Usuarios.Contracts;

/// <summary>
/// Obtem um usuario pelo identificador.
/// </summary>
public interface IObterUsuarioUseCase
{
    Task<Result<UsuarioDto>> ExecutarAsync(int id, CancellationToken cancellationToken = default);
}
