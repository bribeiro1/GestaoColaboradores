using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.UseCases.Usuarios.Contracts;
using GestaoColaboradores.Domain.Repositories;

namespace GestaoColaboradores.Application.UseCases.Usuarios;

/// <summary>
/// Exclui fisicamente um usuário, conforme o enunciado. A alternativa não
/// destrutiva é AlterarSituacaoUsuarioUseCase - ver as divergências no README.
/// </summary>
public sealed class ExcluirUsuarioUseCase : IExcluirUsuarioUseCase
{
    private readonly IUsuarioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ExcluirUsuarioUseCase(IUsuarioRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _repository.ObterPorIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (usuario is null)
        {
            // 404 em vez de sucesso silencioso: a tela precisa distinguir
            // "excluído agora" de "já não existia", que costuma indicar
            // listagem desatualizada em outra aba.
            return Result.NaoEncontrado($"Usuário {id} não encontrado.");
        }

        _repository.Remover(usuario);
        await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return Result.Ok();
    }
}
