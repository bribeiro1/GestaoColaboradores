using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.DTOs;

namespace GestaoColaboradores.Application.UseCases.Usuarios.Contracts;

/// <summary>
/// Ativa ou inativa um usuario sem remover o registro.
/// </summary>
public interface IAlterarSituacaoUsuarioUseCase
{
    Task<Result<UsuarioDto>> ExecutarAsync(
        int id,
        bool ativo,
        CancellationToken cancellationToken = default);
}
