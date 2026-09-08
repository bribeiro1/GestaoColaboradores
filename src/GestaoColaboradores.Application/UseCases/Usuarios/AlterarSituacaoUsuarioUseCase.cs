using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.DTOs;
using GestaoColaboradores.Application.UseCases.Usuarios.Contracts;
using GestaoColaboradores.Domain.Repositories;

namespace GestaoColaboradores.Application.UseCases.Usuarios;

/// <summary>
/// Ativa ou inativa um usuário sem apagar o registro. Operação própria, e não
/// um PUT do formulário inteiro, porque a intenção de negócio é outra.
/// </summary>
public sealed class AlterarSituacaoUsuarioUseCase : IAlterarSituacaoUsuarioUseCase
{
    private readonly IUsuarioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AlterarSituacaoUsuarioUseCase(IUsuarioRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UsuarioDto>> ExecutarAsync(
        int id,
        bool ativo,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _repository.ObterPorIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (usuario is null)
        {
            return Result<UsuarioDto>.NaoEncontrado($"Usuário {id} não encontrado.");
        }

        if (ativo)
        {
            usuario.Ativar();
        }
        else
        {
            usuario.Inativar();
        }

        await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return Result<UsuarioDto>.Ok(usuario.ParaDto());
    }
}
