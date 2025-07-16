using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IntranetDocumentos.Data;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.Data.SqlClient;

namespace IntranetDocumentos.Services
{
    /// <summary>
    /// Serviço para backup e restauração do banco de dados SQL Server
    /// </summary>
    public interface IDatabaseBackupService
    {
        Task<string> CreateBackupAsync();
        Task<string> CreateScheduledBackupAsync();
        Task RestoreBackupAsync(string backupPath);
        Task<List<string>> GetBackupListAsync();
        Task CleanupOldBackupsAsync(int retentionDays = 30);
    }

    /// <summary>
    /// Implementação do serviço de backup para SQL Server
    /// </summary>
    public class DatabaseBackupService : IDatabaseBackupService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _backupBasePath;

        public DatabaseBackupService(
            ApplicationDbContext context,
            ILogger<DatabaseBackupService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _backupBasePath = _configuration["Backup:BackupPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "DatabaseBackups");
        }

        /// <summary>
        /// Cria backup manual do banco de dados SQL Server
        /// </summary>
        public async Task<string> CreateBackupAsync()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"IntranetDocumentos_Backup_{timestamp}.bak";
                var backupPath = Path.Combine(_backupBasePath, backupFileName);

                // Garantir que o diretório existe
                Directory.CreateDirectory(_backupBasePath);

                var connectionString = _context.Database.GetConnectionString();
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Comando SQL Server para backup
                var backupCommand = $@"
                    BACKUP DATABASE [{databaseName}] 
                    TO DISK = N'{backupPath}' 
                    WITH FORMAT, INIT, 
                         NAME = 'IntranetDocumentos-Full Database Backup', 
                         SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                using var command = new SqlCommand(backupCommand, connection);
                command.CommandTimeout = 600; // 10 minutos timeout

                _logger.LogInformation($"Iniciando backup SQL Server: {backupPath}");
                await command.ExecuteNonQueryAsync();

                // Criar arquivo ZIP com o backup
                var zipPath = backupPath.Replace(".bak", ".zip");
                using (var zip = new FileStream(zipPath, FileMode.Create))
                using (var archive = new ZipArchive(zip, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(backupPath, Path.GetFileName(backupPath));
                }

                // Remover arquivo .bak original (manter apenas o ZIP)
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                _logger.LogInformation($"Backup SQL Server criado: {zipPath}");
                return zipPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar backup do banco de dados SQL Server");
                throw;
            }
        }

        /// <summary>
        /// Cria backup automático (agendado)
        /// </summary>
        public async Task<string> CreateScheduledBackupAsync()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HH");
                var backupFileName = $"IntranetDocumentos_Auto_{timestamp}.bak";
                var autoBackupPath = Path.Combine(_backupBasePath, "Auto");
                var backupPath = Path.Combine(autoBackupPath, backupFileName);

                // Garantir que o diretório existe
                Directory.CreateDirectory(autoBackupPath);

                var connectionString = _context.Database.GetConnectionString();
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Comando SQL Server para backup
                var backupCommand = $@"
                    BACKUP DATABASE [{databaseName}] 
                    TO DISK = N'{backupPath}' 
                    WITH FORMAT, INIT, 
                         NAME = 'IntranetDocumentos-Auto Database Backup', 
                         SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                using var command = new SqlCommand(backupCommand, connection);
                command.CommandTimeout = 600; // 10 minutos timeout

                _logger.LogInformation($"Iniciando backup automático SQL Server: {backupPath}");
                await command.ExecuteNonQueryAsync();

                _logger.LogInformation($"Backup automático criado: {backupPath}");
                return backupPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar backup automático do banco de dados");
                throw;
            }
        }

        /// <summary>
        /// Restaura backup do banco de dados
        /// </summary>
        public async Task RestoreBackupAsync(string backupPath)
        {
            try
            {
                _logger.LogInformation($"Iniciando restauração do backup: {backupPath}");

                var connectionString = _context.Database.GetConnectionString();
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;

                // Extrair arquivo .bak se for ZIP
                string actualBackupPath = backupPath;
                if (Path.GetExtension(backupPath).ToLower() == ".zip")
                {
                    var extractPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(backupPath));
                    Directory.CreateDirectory(extractPath);
                    
                    using (var archive = ZipFile.OpenRead(backupPath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (entry.Name.EndsWith(".bak"))
                            {
                                actualBackupPath = Path.Combine(extractPath, entry.Name);
                                entry.ExtractToFile(actualBackupPath, true);
                                break;
                            }
                        }
                    }
                }

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Comando para restaurar banco
                var restoreCommand = $@"
                    RESTORE DATABASE [{databaseName}] 
                    FROM DISK = N'{actualBackupPath}' 
                    WITH REPLACE, STATS = 10";

                using var command = new SqlCommand(restoreCommand, connection);
                command.CommandTimeout = 1800; // 30 minutos timeout

                await command.ExecuteNonQueryAsync();

                // Limpar arquivo temporário se foi extraído
                if (actualBackupPath != backupPath && File.Exists(actualBackupPath))
                {
                    File.Delete(actualBackupPath);
                    var tempDir = Path.GetDirectoryName(actualBackupPath);
                    if (Directory.Exists(tempDir) && !Directory.EnumerateFileSystemEntries(tempDir).Any())
                    {
                        Directory.Delete(tempDir);
                    }
                }

                _logger.LogInformation($"Restauração concluída com sucesso: {backupPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao restaurar backup: {backupPath}");
                throw;
            }
        }

        /// <summary>
        /// Obtém lista de backups disponíveis
        /// </summary>
        public async Task<List<string>> GetBackupListAsync()
        {
            try
            {
                var backups = new List<string>();

                if (Directory.Exists(_backupBasePath))
                {
                    // Backups manuais
                    var manualBackups = Directory.GetFiles(_backupBasePath, "*.zip")
                        .Where(f => Path.GetFileName(f).StartsWith("IntranetDocumentos_Backup_"))
                        .OrderByDescending(f => File.GetCreationTime(f))
                        .ToList();

                    backups.AddRange(manualBackups);

                    // Backups automáticos
                    var autoBackupPath = Path.Combine(_backupBasePath, "Auto");
                    if (Directory.Exists(autoBackupPath))
                    {
                        var autoBackups = Directory.GetFiles(autoBackupPath, "*.bak")
                            .Where(f => Path.GetFileName(f).StartsWith("IntranetDocumentos_Auto_"))
                            .OrderByDescending(f => File.GetCreationTime(f))
                            .Take(10) // Últimos 10 backups automáticos
                            .ToList();

                        backups.AddRange(autoBackups);
                    }
                }

                return await Task.FromResult(backups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter lista de backups");
                return new List<string>();
            }
        }

        /// <summary>
        /// Remove backups antigos
        /// </summary>
        public async Task CleanupOldBackupsAsync(int retentionDays = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                int deletedCount = 0;

                if (Directory.Exists(_backupBasePath))
                {
                    // Limpar backups manuais antigos
                    var oldManualBackups = Directory.GetFiles(_backupBasePath, "*.zip")
                        .Where(f => File.GetCreationTime(f) < cutoffDate)
                        .ToList();

                    foreach (var backup in oldManualBackups)
                    {
                        File.Delete(backup);
                        deletedCount++;
                        _logger.LogInformation($"Backup antigo removido: {backup}");
                    }

                    // Limpar backups automáticos antigos
                    var autoBackupPath = Path.Combine(_backupBasePath, "Auto");
                    if (Directory.Exists(autoBackupPath))
                    {
                        var oldAutoBackups = Directory.GetFiles(autoBackupPath, "*.bak")
                            .Where(f => File.GetCreationTime(f) < cutoffDate)
                            .ToList();

                        foreach (var backup in oldAutoBackups)
                        {
                            File.Delete(backup);
                            deletedCount++;
                            _logger.LogInformation($"Backup automático antigo removido: {backup}");
                        }
                    }
                }

                _logger.LogInformation($"Limpeza de backups concluída. {deletedCount} arquivos removidos.");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante limpeza de backups antigos");
                throw;
            }
        }
    }
}
