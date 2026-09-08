using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Web.Controllers;

public sealed class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => RedirectToAction("Index", "Usuarios");

    /// <summary>
    /// Erro de navegação, fora do fluxo AJAX. Não expõe detalhe técnico: o
    /// usuário recebe o identificador de correlação do log.
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Erro()
    {
        ViewBag.Correlacao = HttpContext.TraceIdentifier;
        return View();
    }
}
