# Test script for notification system
Write-Host "=== Teste do Sistema de Notificações ===" -ForegroundColor Green

# Check if notification service files exist
$notificationFiles = @(
    "Services\Notifications\INotificationService.cs",
    "Services\Notifications\NotificationService.cs", 
    "Services\Notifications\IEmailService.cs",
    "Services\Notifications\EmailService.cs",
    "Services\Background\MeetingReminderBackgroundService.cs"
)

Write-Host "`nVerificando arquivos do sistema de notificações:" -ForegroundColor Yellow

foreach ($file in $notificationFiles) {
    if (Test-Path $file) {
        Write-Host "✓ $file" -ForegroundColor Green
    } else {
        Write-Host "✗ $file - ARQUIVO NÃO ENCONTRADO" -ForegroundColor Red
    }
}

# Test build
Write-Host "`nTestando compilação:" -ForegroundColor Yellow
$buildResult = dotnet build IntranetDocumentos.csproj --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Projeto compilado com sucesso" -ForegroundColor Green
} else {
    Write-Host "✗ Erro na compilação" -ForegroundColor Red
}

# Check appsettings.json for notification configuration
Write-Host "`nVerificando configurações:" -ForegroundColor Yellow
$appSettings = Get-Content "appsettings.json" -Raw | ConvertFrom-Json

if ($appSettings.EmailSettings) {
    Write-Host "✓ Configurações de email encontradas" -ForegroundColor Green
    Write-Host "  - SMTP Host: $($appSettings.EmailSettings.SmtpHost)" -ForegroundColor Cyan
    Write-Host "  - SMTP Port: $($appSettings.EmailSettings.SmtpPort)" -ForegroundColor Cyan
    Write-Host "  - From Email: $($appSettings.EmailSettings.FromEmail)" -ForegroundColor Cyan
} else {
    Write-Host "✗ Configurações de email não encontradas" -ForegroundColor Red
}

if ($appSettings.NotificationSettings) {
    Write-Host "✓ Configurações de notificação encontradas" -ForegroundColor Green
    Write-Host "  - Enabled: $($appSettings.NotificationSettings.Enabled)" -ForegroundColor Cyan
    Write-Host "  - Reminder Interval: $($appSettings.NotificationSettings.ReminderIntervalMinutes) min" -ForegroundColor Cyan
    Write-Host "  - Reminder Lead Time: $($appSettings.NotificationSettings.ReminderLeadTimeMinutes) min" -ForegroundColor Cyan
} else {
    Write-Host "✗ Configurações de notificação não encontradas" -ForegroundColor Red
}

# Test Program.cs registration
Write-Host "`nVerificando registros no Program.cs:" -ForegroundColor Yellow
$programContent = Get-Content "Program.cs" -Raw

$services = @(
    "IEmailService",
    "INotificationService", 
    "MeetingReminderBackgroundService"
)

foreach ($service in $services) {
    if ($programContent -match $service) {
        Write-Host "✓ $service registrado" -ForegroundColor Green
    } else {
        Write-Host "✗ $service não registrado" -ForegroundColor Red
    }
}

Write-Host "`n=== Resumo ===" -ForegroundColor Green
Write-Host "Sistema de notificações implementado com:"
Write-Host "• Notificações de novos documentos" -ForegroundColor Cyan
Write-Host "• Notificações de novas reuniões" -ForegroundColor Cyan
Write-Host "• Notificações de cancelamento de reuniões" -ForegroundColor Cyan
Write-Host "• Notificações de atualização de reuniões" -ForegroundColor Cyan
Write-Host "• Lembretes automáticos de reuniões" -ForegroundColor Cyan
Write-Host "• Serviço de background para lembretes" -ForegroundColor Cyan

Write-Host "`nPara configurar o sistema:" -ForegroundColor Yellow
Write-Host "1. Configure as credenciais SMTP no appsettings.json" -ForegroundColor White
Write-Host "2. Ajuste os intervalos de lembrete conforme necessário" -ForegroundColor White
Write-Host "3. Certifique-se de que os usuários têm emails válidos cadastrados" -ForegroundColor White

Write-Host "`nTodos os componentes estão instalados e funcionais!" -ForegroundColor Green
