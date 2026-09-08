using GestaoColaboradores.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Web.Controllers.Api;

/// <summary>
/// Tradução única de Result para resposta HTTP, para que duas actions não
/// respondam status diferentes na mesma situação.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Responder<T>(Result<T> resultado)
    {
        return resultado.Sucesso
            ? Ok(resultado.Valor)
            : ResponderFalha(resultado);
    }

    protected IActionResult Responder(Result resultado)
    {
        return resultado.Sucesso
            ? NoContent()
            : ResponderFalha(resultado);
    }

    protected IActionResult ResponderCriado<T>(Result<T> resultado, string action, object rota)
    {
        return resultado.Sucesso
            ? CreatedAtAction(action, rota, resultado.Valor)
            : ResponderFalha(resultado);
    }

    private ObjectResult ResponderFalha(Result resultado)
    {
        var (status, titulo) = resultado.TipoErro switch
        {
            TipoErro.NaoEncontrado => (StatusCodes.Status404NotFound, "Registro não encontrado"),
            TipoErro.Conflito => (StatusCodes.Status409Conflict, "Conflito"),
            _ => (StatusCodes.Status400BadRequest, "Requisição inválida")
        };

        return Problem(
            detail: resultado.Erro,
            statusCode: status,
            title: titulo,
            instance: HttpContext.Request.Path);
    }
}
