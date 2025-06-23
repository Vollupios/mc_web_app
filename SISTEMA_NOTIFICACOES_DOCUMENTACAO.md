# Sistema de Notificações Automáticas - Documentação Completa

## Visão Geral

O sistema de notificações automáticas foi implementado para enviar alertas por email sobre documentos e reuniões. O sistema é totalmente integrado com os fluxos existentes e funciona de forma assíncrona para não impactar a performance da aplicação.

## Componentes Implementados

### 1. Interfaces de Serviço
- **IEmailService**: Interface para envio de emails
- **INotificationService**: Interface para notificações do sistema

### 2. Implementações de Serviço
- **EmailService**: Implementação SMTP para envio de emails
- **NotificationService**: Lógica de negócio para notificações
- **MeetingReminderBackgroundService**: Serviço em background para lembretes automáticos

### 3. Integração com Serviços Existentes
- **DocumentWriter**: Integrado para notificar sobre novos documentos
- **ReuniaoService**: Integrado para notificar sobre reuniões (criação, atualização, cancelamento)

## Funcionalidades Implementadas

### 📄 Notificações de Documentos
- **Novos Documentos**: Enviadas quando um documento é carregado
- **Destinatários**: Usuários com acesso ao departamento do documento
- **Conteúdo**: Nome do arquivo, departamento, data de upload, uploader

### 📅 Notificações de Reuniões
- **Nova Reunião**: Enviada quando uma reunião é criada
- **Atualização**: Enviada quando dados da reunião são alterados
- **Cancelamento**: Enviada quando reunião é cancelada ou removida
- **Destinatários**: Usuários dos departamentos dos participantes

### ⏰ Lembretes Automáticos
- **Execução**: Serviço em background executado a cada 15 minutos
- **Critério**: Reuniões nas próximas 2 horas
- **Conteúdo**: Detalhes da reunião e tempo restante

## Configuração

### appsettings.json
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "seu-email@gmail.com",
    "SmtpPassword": "sua-senha-de-app",
    "FromEmail": "noreply@suaempresa.com",
    "FromName": "Sistema Intranet",
    "EnableSsl": true
  },
  "NotificationSettings": {
    "Enabled": true,
    "ReminderIntervalMinutes": 15,
    "ReminderLeadTimeMinutes": 120
  }
}
```

### Configuração SMTP
Para usar Gmail:
1. Ative a autenticação de 2 fatores
2. Gere uma senha de aplicativo
3. Use a senha de aplicativo no campo SmtpPassword

## Registro de Serviços (Program.cs)

```csharp
// Serviços de notificação
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Serviço de background para lembretes
builder.Services.AddHostedService<MeetingReminderBackgroundService>();
```

## Fluxo de Notificações

### Documentos
1. Usuário faz upload de documento
2. DocumentWriter salva o documento
3. Sistema identifica usuários com acesso ao departamento
4. Email é enviado assincronamente
5. Log de sucesso/erro é registrado

### Reuniões
1. Usuário cria/atualiza/cancela reunião
2. ReuniaoService executa a operação
3. Sistema identifica participantes e seus departamentos
4. Email é enviado assincronamente
5. Log de sucesso/erro é registrado

### Lembretes
1. Serviço em background executa a cada 15 minutos
2. Busca reuniões nas próximas 2 horas
3. Identifica participantes
4. Envia lembretes por email
5. Log de quantidade de lembretes enviados

## Características Técnicas

### Execução Assíncrona
- Todas as notificações são enviadas em background
- Não bloqueiam o processo principal
- Falhas não afetam a funcionalidade principal

### Tolerância a Falhas
- Try-catch em todos os pontos críticos
- Logs detalhados para diagnóstico
- Continuidade mesmo com falhas de email

### Performance
- Reutilização de conexões SMTP
- Envio em lote para múltiplos destinatários
- Queries otimizadas para buscar destinatários

### Segurança
- Validação de emails
- Filtros de destinatários por departamento
- Não exposição de dados sensíveis nos logs

## Templates de Email

### Novos Documentos
```
📄 Novo Documento Disponível

Um novo documento foi adicionado ao sistema:

📋 Arquivo: [nome_do_arquivo]
🏢 Departamento: [departamento]
👤 Enviado por: [uploader]
📅 Data: [data_upload]

Acesse o sistema para visualizar o documento.
```

### Novas Reuniões
```
📅 Nova Reunião Agendada

Uma nova reunião foi agendada no sistema:

📋 Título: [título]
📅 Data: [data]
🕒 Horário: [hora_inicio] - [hora_fim]
🏢 Local: [sala/online]
👤 Responsável: [organizador]

Verifique os detalhes no sistema.
```

### Cancelamentos
```
❌ Reunião Cancelada

A seguinte reunião foi cancelada:

📋 Título: [título]
📅 Data: [data]
🕒 Horário: [hora_inicio] - [hora_fim]
👤 Responsável: [organizador]

A reunião foi removida do sistema.
```

### Lembretes
```
⏰ Lembrete: Reunião Próxima

Você tem uma reunião em breve:

📋 Título: [título]
📅 Data: [data]
🕒 Horário: [hora_inicio]
⏱️ Tempo restante: [tempo_restante]
🏢 Local: [sala/online]

Prepare-se para a reunião!
```

## Logs e Monitoramento

### Logs de Sucesso
- Quantidade de notificações enviadas
- Destinatários por tipo de notificação
- Performance do serviço de background

### Logs de Erro
- Falhas de conexão SMTP
- Emails inválidos
- Erros de configuração

### Métricas
- Taxa de sucesso de envio
- Tempo de execução do background service
- Volume de notificações por tipo

## Troubleshooting

### Emails não são enviados
1. Verificar configurações SMTP
2. Verificar logs de erro
3. Testar conexão de rede
4. Validar credenciais

### Lembretes não funcionam
1. Verificar se o background service está rodando
2. Verificar logs do serviço
3. Validar configurações de intervalo
4. Verificar se há reuniões próximas

### Performance lenta
1. Verificar quantidade de destinatários
2. Otimizar queries de busca
3. Ajustar intervalo do background service
4. Verificar latência do servidor SMTP

## Manutenção

### Configurações Recomendadas
- Intervalo de lembretes: 15-30 minutos
- Tempo de antecedência: 1-2 horas
- Timeout SMTP: 30 segundos
- Retry automático: 3 tentativas

### Atualizações Futuras
- Interface web para configuração
- Notificações por WhatsApp/SMS
- Templates personalizáveis
- Dashboard de estatísticas
- Notificações push no navegador

## Conclusão

O sistema de notificações automáticas está completamente implementado e funcional. Ele fornece:

✅ Notificações automáticas para documentos e reuniões
✅ Lembretes automáticos de reuniões
✅ Execução assíncrona e tolerante a falhas
✅ Configuração flexível via appsettings.json
✅ Logs detalhados para monitoramento
✅ Integração transparente com o sistema existente

O sistema está pronto para uso em produção, bastando configurar as credenciais SMTP adequadas.
