using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.DTOs;
using GestaoColaboradores.Application.UseCases.Usuarios.Contracts;
using GestaoColaboradores.Domain.Repositories;

namespace GestaoColaboradores.Application.UseCases.Usuarios;

/// <summary>Obtém um usuário para preencher o formulário de edição.</summary>
public sealed class ObterUsuarioUseCase : IObterUsuarioUseCase
{
    private readonly IUsuarioRepository _repository;

    public ObterUsuarioUseCase(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UsuarioDto>> ExecutarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _repository.ObterPorIdAsync(id, cancellationToken).ConfigureAwait(false);

        return usuario is null
            ? Result<UsuarioDto>.NaoEncontrado($"Usuário {id} não encontrado.")
            : Result<UsuarioDto>.Ok(usuario.ParaDto());
    }
}
