using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using IntranetDocumentos.Services;

namespace IntranetDocumentos.Services
{
    /// <summary>
    /// Serviço em background para backups automáticos
    /// </summary>
    public class BackupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackupBackgroundService> _logger;
        private readonly TimeSpan _backupInterval = TimeSpan.FromHours(6); // Backup a cada 6 horas

        public BackupBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BackupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de backup automático iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var backupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();
                        
                        // Cria backup automático
                        await backupService.CreateScheduledBackupAsync();
                        
                        // Limpa backups antigos
                        await backupService.CleanupOldBackupsAsync(30);
                        
                        _logger.LogInformation("Backup automático executado com sucesso");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro durante backup automático");
                }

                // Aguarda próximo backup
                await Task.Delay(_backupInterval, stoppingToken);
            }
        }
    }
}
