using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.DTOs;
using GestaoColaboradores.Application.UseCases.Usuarios.Contracts;
using GestaoColaboradores.Domain.Entities;
using GestaoColaboradores.Domain.Repositories;

namespace GestaoColaboradores.Application.UseCases.Usuarios;

/// <summary>
/// Cria um novo usuário.
///
/// Divisão adotada: invariante da entidade (nome, valor/hora, data) fica no
/// agregado, porque vale para qualquer entrada; regra de conjunto (nome
/// duplicado) fica aqui, porque depende de consultar outros registros.
/// </summary>
public sealed class CriarUsuarioUseCase : ICriarUsuarioUseCase
{
    private readonly IUsuarioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _clock;

    public CriarUsuarioUseCase(
        IUsuarioRepository repository,
        IUnitOfWork unitOfWork,
        TimeProvider clock)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<UsuarioDto>> ExecutarAsync(
        SalvarUsuarioRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var nome = request.Nome?.Trim() ?? string.Empty;

        var nomeEmUso = await _repository
            .ExisteComNomeAsync(nome, idIgnorado: null, cancellationToken)
            .ConfigureAwait(false);

        if (nomeEmUso)
        {
            return Result<UsuarioDto>.Conflito($"Já existe um usuário com o nome '{nome}'.");
        }

        var usuario = Usuario.Criar(
            nome,
            request.ValorHora,
            request.DataCadastro,
            request.Ativo,
            _clock);

        await _repository.AdicionarAsync(usuario, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return Result<UsuarioDto>.Ok(usuario.ParaDto());
    }
}
