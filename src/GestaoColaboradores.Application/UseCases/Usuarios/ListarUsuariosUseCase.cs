using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.DTOs;
using GestaoColaboradores.Application.UseCases.Usuarios.Contracts;
using GestaoColaboradores.Domain.Repositories;

namespace GestaoColaboradores.Application.UseCases.Usuarios;

/// <summary>
/// Consulta a lista de usuários. Lista vazia é sucesso, não erro.
/// </summary>
public sealed class ListarUsuariosUseCase : IListarUsuariosUseCase
{
    private readonly IUsuarioRepository _repository;

    public ListarUsuariosUseCase(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<UsuarioDto>>> ExecutarAsync(
        FiltroUsuarioRequest filtro,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var usuarios = await _repository
            .ListarAsync(filtro.Nome, filtro.Ativo, cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<UsuarioDto>>.Ok(usuarios.ParaDto());
    }
}
