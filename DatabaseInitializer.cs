using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Data;

namespace IntranetDocumentos
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Aplicar migrations pendentes
                if (context.Database.GetPendingMigrations().Any())
                {
                    logger.LogInformation("Aplicando migrations pendentes...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations aplicadas com sucesso.");
                }
                else
                {
                    logger.LogInformation("Banco de dados está atualizado.");
                }

                // Verificar se o banco está acessível
                await context.Database.CanConnectAsync();
                logger.LogInformation("Conexão com banco de dados verificada com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao inicializar banco de dados: {Message}", ex.Message);
                throw;
            }
        }

        public static void EnsureDataDirectories(IConfiguration configuration, ILogger logger)
        {
            try
            {
                var documentsPath = configuration["DocumentSettings:StoragePath"] ?? "DocumentsStorage";
                var backupPath = configuration["DocumentSettings:BackupPath"] ?? "DatabaseBackups";

                // Converter caminhos relativos para absolutos se necessário
                if (!Path.IsPathRooted(documentsPath))
                {
                    documentsPath = Path.Combine(Directory.GetCurrentDirectory(), documentsPath);
                }

                if (!Path.IsPathRooted(backupPath))
                {
                    backupPath = Path.Combine(Directory.GetCurrentDirectory(), backupPath);
                }

                // Criar diretórios se não existirem
                var directories = new[]
                {
                    documentsPath,
                    backupPath,
                    Path.Combine(documentsPath, "Geral"),
                    Path.Combine(documentsPath, "Pessoal"),
                    Path.Combine(documentsPath, "Fiscal"),
                    Path.Combine(documentsPath, "Contabil"),
                    Path.Combine(documentsPath, "Cadastro"),
                    Path.Combine(documentsPath, "Apoio"),
                    Path.Combine(documentsPath, "TI"),
                    Path.Combine(backupPath, "Auto")
                };

                foreach (var dir in directories)
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                        logger.LogInformation("Diretório criado: {Directory}", dir);
                    }
                }

                logger.LogInformation("Estrutura de diretórios verificada com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao criar estrutura de diretórios: {Message}", ex.Message);
                throw;
            }
        }

        public static void ValidateConfiguration(IConfiguration configuration, ILogger logger)
        {
            var errors = new List<string>();

            // Validar connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                errors.Add("Connection string 'DefaultConnection' não configurada");
            }
            else if (connectionString.Contains("CHANGE_THIS_PASSWORD"))
            {
                errors.Add("Connection string contém senha padrão - configure uma senha segura");
            }

            // Validar configurações de email (opcional)
            var emailConfig = configuration.GetSection("EmailSettings");
            if (emailConfig.Exists())
            {
                var smtpPassword = emailConfig["SmtpPassword"];
                if (!string.IsNullOrEmpty(smtpPassword) && smtpPassword.Contains("CHANGE_THIS"))
                {
                    logger.LogWarning("Configuração de email contém senha padrão - emails não funcionarão");
                }
            }

            // Validar paths
            var documentsPath = configuration["DocumentSettings:StoragePath"];
            if (!string.IsNullOrEmpty(documentsPath) && !Directory.Exists(Path.GetDirectoryName(documentsPath)))
            {
                errors.Add($"Diretório pai para documentos não existe: {Path.GetDirectoryName(documentsPath)}");
            }

            if (errors.Any())
            {
                var errorMessage = "Erros de configuração encontrados:\n" + string.Join("\n", errors);
                logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            logger.LogInformation("Configuração validada com sucesso.");
        }
    }
}
