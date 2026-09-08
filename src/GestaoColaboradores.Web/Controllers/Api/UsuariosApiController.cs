using GestaoColaboradores.Application.DTOs;
using GestaoColaboradores.Application.UseCases.Usuarios.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Web.Controllers.Api;

/// <summary>
/// Endpoints consumidos por jQuery.ajax nas duas telas.
///
/// O controller é uma casca fina: traduz HTTP para caso de uso e de volta.
/// Nenhuma regra de negócio aqui - se houvesse, ficaria indisponível para
/// qualquer outro ponto de entrada e só seria testável com o pipeline HTTP
/// de pé. Os [ProducesResponseType] alimentam o documento OpenAPI.
/// </summary>
[Route("api/usuarios")]
public sealed class UsuariosApiController : ApiControllerBase
{
    private readonly IListarUsuariosUseCase _listar;
    private readonly IObterUsuarioUseCase _obter;
    private readonly ICriarUsuarioUseCase _criar;
    private readonly IAtualizarUsuarioUseCase _atualizar;
    private readonly IExcluirUsuarioUseCase _excluir;
    private readonly IAlterarSituacaoUsuarioUseCase _alterarSituacao;

    public UsuariosApiController(
        IListarUsuariosUseCase listar,
        IObterUsuarioUseCase obter,
        ICriarUsuarioUseCase criar,
        IAtualizarUsuarioUseCase atualizar,
        IExcluirUsuarioUseCase excluir,
        IAlterarSituacaoUsuarioUseCase alterarSituacao)
    {
        _listar = listar;
        _obter = obter;
        _criar = criar;
        _atualizar = atualizar;
        _excluir = excluir;
        _alterarSituacao = alterarSituacao;
    }

    /// <summary>Lista usuários, com filtros opcionais de nome e situação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] FiltroUsuarioRequest filtro,
        CancellationToken cancellationToken)
    {
        var resultado = await _listar.ExecutarAsync(filtro, cancellationToken);
        return Responder(resultado);
    }

    /// <summary>Obtém um usuário pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(int id, CancellationToken cancellationToken)
    {
        var resultado = await _obter.ExecutarAsync(id, cancellationToken);
        return Responder(resultado);
    }

    /// <summary>Cria um novo usuário.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(
        [FromBody] SalvarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _criar.ExecutarAsync(request, cancellationToken);

        return ResponderCriado(
            resultado,
            nameof(Obter),
            new { id = resultado.Valor?.Id ?? 0 });
    }

    /// <summary>Atualiza um usuário existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] SalvarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _atualizar.ExecutarAsync(id, request, cancellationToken);
        return Responder(resultado);
    }

    /// <summary>Exclui fisicamente o usuário.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id, CancellationToken cancellationToken)
    {
        var resultado = await _excluir.ExecutarAsync(id, cancellationToken);
        return Responder(resultado);
    }

    /// <summary>
    /// Ativa ou inativa sem apagar. PATCH e não PUT: altera parte do
    /// recurso, não o substitui.
    /// </summary>
    [HttpPatch("{id:int}/situacao")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarSituacao(
        int id,
        [FromBody] AlterarSituacaoRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _alterarSituacao.ExecutarAsync(id, request.Ativo, cancellationToken);
        return Responder(resultado);
    }
}
