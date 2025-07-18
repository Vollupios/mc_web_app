using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IntranetDocumentos.Models;

namespace IntranetDocumentos.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Controller da Home, exibe páginas públicas e de erro.
    /// </summary>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public ActionResult Index()
    {
        _logger.LogInformation("Acessando página inicial.");
        return View();
    }

    public ActionResult Privacy()
    {
        _logger.LogInformation("Acessando página de privacidade.");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public ActionResult Error()
    {
        _logger.LogError("Erro detectado na aplicação. RequestId: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
