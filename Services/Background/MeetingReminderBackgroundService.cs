using IntranetDocumentos.Services.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntranetDocumentos.Services.Background
{
    /// <summary>
    /// Serviço de background para envio automático de lembretes de reunião
    /// </summary>
    public class MeetingReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MeetingReminderBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Verifica a cada 6 horas

        public MeetingReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<MeetingReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de lembretes de reunião iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Executar apenas durante o horário comercial (8h às 18h)
                    var now = DateTime.Now;
                    if (now.Hour >= 8 && now.Hour <= 18 && now.DayOfWeek != DayOfWeek.Saturday && now.DayOfWeek != DayOfWeek.Sunday)
                    {
                        await SendMeetingReminders();
                    }
                    else
                    {
                        _logger.LogDebug("Fora do horário comercial, lembretes não enviados");
                    }

                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no serviço de lembretes de reunião");
                    // Aguarda um tempo menor em caso de erro antes de tentar novamente
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
            }

            _logger.LogInformation("Serviço de lembretes de reunião finalizado");
        }

        private async Task SendMeetingReminders()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                _logger.LogDebug("Iniciando verificação de lembretes de reunião");
                await notificationService.SendMeetingRemindersAsync();
                _logger.LogDebug("Verificação de lembretes de reunião concluída");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar lembretes de reunião");
            }
        }
    }
}
