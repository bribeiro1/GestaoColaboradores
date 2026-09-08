using GestaoColaboradores.Application.DTOs;
using GestaoColaboradores.Application.UseCases.Usuarios.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Web.Controllers;

/// <summary>
/// Devolve apenas as duas telas em Razor. Salvar, excluir e consultar são
/// chamadas por jQuery.ajax contra o UsuariosApiController.
///
/// Dois controllers porque os contratos de erro diferem - página de erro
/// versus ProblemDetails - e para a API poder ser extraída depois sem tocar
/// nas views.
/// </summary>
public sealed class UsuariosController : Controller
{
    private readonly IObterUsuarioUseCase _obterUsuario;

    public UsuariosController(IObterUsuarioUseCase obterUsuario)
    {
        _obterUsuario = obterUsuario;
    }

    /// <summary>
    /// Tela 1: consultar e excluir. Renderiza a estrutura; o grid é
    /// carregado por AJAX.
    /// </summary>
    [HttpGet]
    public IActionResult Index() => View();

    /// <summary>
    /// Tela 2: cadastrar e editar. Sem id = novo.
    ///
    /// O formulário já chega preenchido pelo servidor, em vez de um AJAX
    /// adicional após o load: menos uma ida ao servidor e sem campo piscando.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Form(int? id, CancellationToken cancellationToken)
    {
        if (id is null or <= 0)
        {
            return View(new SalvarUsuarioRequest
            {
                DataCadastro = DateTime.Today,
                Ativo = true
            });
        }

        var resultado = await _obterUsuario.ExecutarAsync(id.Value, cancellationToken);

        if (resultado.Falhou || resultado.Valor is null)
        {
            TempData["Mensagem"] = resultado.Erro ?? "Usuário não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var usuario = resultado.Valor;

        return View(new SalvarUsuarioRequest
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            ValorHora = usuario.ValorHora,
            DataCadastro = usuario.DataCadastro,
            Ativo = usuario.Ativo
        });
    }
}
