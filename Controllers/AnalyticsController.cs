using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IntranetDocumentos.Services;

namespace IntranetDocumentos.Controllers
{
    /// <summary>
    /// Controller para analytics e dashboard de estatísticas
    /// </summary>
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Página principal do dashboard de analytics
        /// </summary>
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                _logger.LogInformation("Acessando dashboard de analytics");
                var dashboardData = await _analyticsService.GetDashboardDataAsync();
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dashboard de analytics");
                TempData["ErrorMessage"] = "Erro ao carregar os dados do dashboard.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// API endpoint para dados do dashboard (JSON)
        /// </summary>
        [Authorize(Roles = "Admin,Gestor")]
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var dashboardData = await _analyticsService.GetDashboardDataAsync();
                return Json(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter dados do dashboard via API");
                return Json(new { error = "Erro ao carregar dados" });
            }
        }

        /// <summary>
        /// Página específica para estatísticas de documentos
        /// </summary>
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> DocumentStatistics()
        {
            try
            {
                var documentStats = await _analyticsService.GetDocumentStatisticsAsync();
                return View(documentStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar estatísticas de documentos");
                TempData["ErrorMessage"] = "Erro ao carregar as estatísticas de documentos.";
                return RedirectToAction("Dashboard");
            }
        }

        /// <summary>
        /// Página específica para métricas de reuniões
        /// </summary>
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> ReunioesMetrics()
        {
            try
            {
                var reunioesMetrics = await _analyticsService.GetReunioesMetricsAsync();
                return View(reunioesMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar métricas de reuniões");
                TempData["ErrorMessage"] = "Erro ao carregar as métricas de reuniões.";
                return RedirectToAction("Dashboard");
            }
        }

        /// <summary>
        /// Página específica para atividade por departamento
        /// </summary>
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> DepartmentActivity()
        {
            try
            {
                var departmentActivity = await _analyticsService.GetDepartmentActivityAsync();
                return View(departmentActivity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar atividade por departamento");
                TempData["ErrorMessage"] = "Erro ao carregar a atividade por departamento.";
                return RedirectToAction("Dashboard");
            }
        }
    }
}
