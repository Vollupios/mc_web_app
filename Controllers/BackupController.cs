using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IntranetDocumentos.Services;
using System.IO;

namespace IntranetDocumentos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BackupController : Controller
    {
        private readonly IDatabaseBackupService _backupService;
        private readonly ILogger<BackupController> _logger;

        public BackupController(
            IDatabaseBackupService backupService,
            ILogger<BackupController> logger)
        {
            _backupService = backupService;
            _logger = logger;
        }

        /// <summary>
        /// Página principal de gerenciamento de backups
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var backups = await _backupService.GetBackupListAsync();
                var backupInfo = backups.Select(path => new
                {
                    FileName = Path.GetFileName(path),
                    FullPath = path,
                    CreatedDate = System.IO.File.GetCreationTime(path),
                    Size = new FileInfo(path).Length,
                    SizeFormatted = FormatBytes(new FileInfo(path).Length)
                }).OrderByDescending(b => b.CreatedDate).ToList();

                ViewBag.BackupInfo = (dynamic)backupInfo!;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar lista de backups");
                TempData["Error"] = "Erro ao carregar lista de backups: " + ex.Message;
                return View();
            }
        }

        /// <summary>
        /// Cria um backup manual
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateBackup()
        {
            try
            {
                var backupPath = await _backupService.CreateBackupAsync();
                TempData["Success"] = $"Backup criado com sucesso! Arquivo: {Path.GetFileName(backupPath)}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar backup manual");
                TempData["Error"] = "Erro ao criar backup: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }        /// <summary>
        /// Download de um arquivo de backup
        /// </summary>
        public IActionResult Download(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName) || fileName.Contains(".."))
                {
                    return BadRequest("Nome de arquivo inválido");
                }

                var backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "DatabaseBackups");
                var filePath = Path.Combine(backupDirectory, fileName);

                // Busca também na pasta Auto
                if (!System.IO.File.Exists(filePath))
                {
                    filePath = Path.Combine(backupDirectory, "Auto", fileName);
                }

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Arquivo de backup não encontrado");
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = fileName.EndsWith(".zip") ? "application/zip" : "application/octet-stream";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer download do backup: {FileName}", fileName);
                TempData["Error"] = "Erro ao fazer download do backup: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Restaura um backup (cuidado!)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RestoreBackup(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName) || fileName.Contains(".."))
                {
                    TempData["Error"] = "Nome de arquivo inválido";
                    return RedirectToAction(nameof(Index));
                }

                var backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "DatabaseBackups");
                var filePath = Path.Combine(backupDirectory, fileName);

                // Busca também na pasta Auto
                if (!System.IO.File.Exists(filePath))
                {
                    filePath = Path.Combine(backupDirectory, "Auto", fileName);
                }

                if (!System.IO.File.Exists(filePath))
                {
                    TempData["Error"] = "Arquivo de backup não encontrado";
                    return RedirectToAction(nameof(Index));
                }

                await _backupService.RestoreBackupAsync(filePath);
                TempData["Success"] = $"Banco de dados restaurado com sucesso do backup: {fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao restaurar backup: {FileName}", fileName);
                TempData["Error"] = "Erro ao restaurar backup: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Limpa backups antigos
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CleanOldBackups(int keepDays = 30)
        {
            try
            {
                await _backupService.CleanupOldBackupsAsync(keepDays);
                TempData["Success"] = $"Backups antigos removidos (mantidos últimos {keepDays} dias)";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar backups antigos");
                TempData["Error"] = "Erro ao limpar backups antigos: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Formata bytes para exibição legível
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1} {1}", number, suffixes[counter]);
        }
    }
}
