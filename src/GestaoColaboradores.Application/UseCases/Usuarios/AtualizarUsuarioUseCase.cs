using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.DTOs;
using GestaoColaboradores.Application.UseCases.Usuarios.Contracts;
using GestaoColaboradores.Domain.Repositories;

namespace GestaoColaboradores.Application.UseCases.Usuarios;

/// <summary>
/// Atualiza um usuário existente.
///
/// Caso de uso separado do de criação: as pré-condições, a regra de
/// duplicidade e os testes são diferentes.
/// </summary>
public sealed class AtualizarUsuarioUseCase : IAtualizarUsuarioUseCase
{
    private readonly IUsuarioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _clock;

    public AtualizarUsuarioUseCase(
        IUsuarioRepository repository,
        IUnitOfWork unitOfWork,
        TimeProvider clock)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<UsuarioDto>> ExecutarAsync(
        int id,
        SalvarUsuarioRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var usuario = await _repository.ObterPorIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (usuario is null)
        {
            return Result<UsuarioDto>.NaoEncontrado($"Usuário {id} não encontrado.");
        }

        var nome = request.Nome?.Trim() ?? string.Empty;

        var nomeEmUso = await _repository
            .ExisteComNomeAsync(nome, idIgnorado: id, cancellationToken)
            .ConfigureAwait(false);

        if (nomeEmUso)
        {
            return Result<UsuarioDto>.Conflito($"Já existe outro usuário com o nome '{nome}'.");
        }

        usuario.Atualizar(nome, request.ValorHora, request.DataCadastro, request.Ativo, _clock);

        // Sem chamada a Update(): a entidade foi carregada rastreada, então o
        // change tracker gera o UPDATE apenas das colunas alteradas.
        await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return Result<UsuarioDto>.Ok(usuario.ParaDto());
    }
}
