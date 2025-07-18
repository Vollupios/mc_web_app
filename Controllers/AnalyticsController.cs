using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Services;
using IntranetDocumentos.Data;

namespace IntranetDocumentos.Controllers
{    /// <summary>
    /// Controller para analytics e dashboard de estatísticas
    /// Acesso restrito a Administradores e Gestores
    /// </summary>
    [Authorize(Roles = "Admin,Gestor")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }        /// <summary>
        /// Página principal do dashboard de analytics
        /// </summary>
        public async Task<ActionResult> Dashboard()
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
        }        /// <summary>
        /// API endpoint para dados do dashboard (JSON)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetDashboardData()
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
        }        /// <summary>
        /// Página específica para estatísticas de documentos
        /// </summary>
        public async Task<ActionResult> DocumentStatistics()
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
        }        /// <summary>
        /// Página específica para métricas de reuniões
        /// </summary>
        public async Task<ActionResult> MeetingMetrics()
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
        }        /// <summary>
        /// Página específica para atividade por departamento
        /// </summary>
        public async Task<ActionResult> DepartmentActivity()
        {
            try
            {
                var departmentActivity = await _analyticsService.GetDepartmentActivityAsync();
                return View(departmentActivity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar atividade por departamento");                TempData["ErrorMessage"] = "Erro ao carregar a atividade por departamento.";
                return RedirectToAction("Dashboard");
            }
        }

        /// <summary>
        /// Endpoint para verificar a saúde da API
        /// </summary>
        [HttpGet("health")]
        public ActionResult HealthCheck()
        {
            return Ok(new { status = "Healthy" });
        }
    }
}
