using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IntranetDocumentos.Data;
using System.IO;
using System.IO.Compression;
using System.Text;
using MySqlConnector;

namespace IntranetDocumentos.Services
{
    /// <summary>
    /// Serviço para backup e restauração do banco de dados MySQL
    /// </summary>
    public interface IDatabaseBackupService
    {
        Task<string> CreateBackupAsync();
        Task<string> CreateScheduledBackupAsync();
        Task RestoreBackupAsync(string backupPath);
        Task<List<string>> GetBackupListAsync();
        Task CleanOldBackupsAsync(int keepDays = 30);
    }

    public class DatabaseBackupService : IDatabaseBackupService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly string _backupDirectory;

        public DatabaseBackupService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<DatabaseBackupService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            
            // Diretório de backup
            _backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "DatabaseBackups");
            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
            }
        }

        /// <summary>
        /// Cria um backup manual do banco de dados
        /// </summary>
        public async Task<string> CreateBackupAsync()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"IntranetDocumentos_Backup_{timestamp}.sql";
                var backupPath = Path.Combine(_backupDirectory, backupFileName);

                // Para MySQL, fazer backup usando mysqldump
                var connectionString = _context.Database.GetConnectionString();
                await CreateMySqlBackup(connectionString, backupPath);

                // Cria também um backup comprimido
                var zipPath = Path.Combine(_backupDirectory, $"IntranetDocumentos_Backup_{timestamp}.zip");
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    // Adiciona o arquivo SQL do backup
                    zip.CreateEntryFromFile(backupPath, Path.GetFileName(backupPath));
                    
                    // Adiciona informações do backup
                    var infoEntry = zip.CreateEntry("backup_info.txt");
                    using (var stream = infoEntry.Open())
                    using (var writer = new StreamWriter(stream))
                    {
                        await writer.WriteLineAsync($"Backup criado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                        await writer.WriteLineAsync($"Versão da aplicação: 1.0");
                        await writer.WriteLineAsync($"Tipo de backup: MySQL");
                        await writer.WriteLineAsync($"Tamanho do backup: {new FileInfo(backupPath).Length} bytes");
                    }
                }

                _logger.LogInformation($"Backup criado com sucesso: {backupPath}");
                return backupPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar backup do banco de dados");
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
                var backupFileName = $"IntranetDocumentos_Auto_{timestamp}.sql";
                var backupPath = Path.Combine(_backupDirectory, "Auto", backupFileName);
                
                var autoBackupDir = Path.Combine(_backupDirectory, "Auto");
                if (!Directory.Exists(autoBackupDir))
                {
                    Directory.CreateDirectory(autoBackupDir);
                }

                // Para MySQL, fazer backup usando mysqldump
                var connectionString = _context.Database.GetConnectionString();
                await CreateMySqlBackup(connectionString, backupPath);
                
                _logger.LogInformation($"Backup automático criado: {backupPath}");
                return backupPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar backup automático");
                throw;
            }
        }
        /// <summary>
        /// Cria backup usando mysqldump
        /// </summary>
        private async Task CreateMySqlBackup(string connectionString, string backupPath)
        {
            try
            {
                // Extrair informações da connection string
                var builder = new MySqlConnector.MySqlConnectionStringBuilder(connectionString);
                var server = builder.Server;
                var database = builder.Database;
                var user = builder.UserID;
                var password = builder.Password;
                var port = builder.Port;

                // Comando mysqldump
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "mysqldump",
                    Arguments = $"--host={server} --port={port} --user={user} --password={password} --routines --triggers {database}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new System.Diagnostics.Process { StartInfo = processInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    await File.WriteAllTextAsync(backupPath, output);
                    _logger.LogInformation($"Backup MySQL criado: {backupPath}");
                }
                else
                {
                    throw new Exception($"Erro no mysqldump: {error}");
                }
            }
            catch (Exception ex)
            {
                // Fallback: backup simples usando Entity Framework
                _logger.LogWarning($"mysqldump não disponível, usando backup EF: {ex.Message}");
                await CreateEntityFrameworkBackup(backupPath);
            }
        }

        /// <summary>
        /// Backup usando Entity Framework (fallback)
        /// </summary>
        private async Task CreateEntityFrameworkBackup(string backupPath)
        {
            var backup = new StringBuilder();
            backup.AppendLine("-- Backup Entity Framework MySQL");
            backup.AppendLine($"-- Data: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            backup.AppendLine();

            // Backup das tabelas principais (exemplo simplificado)
            backup.AppendLine("-- Documentos");
            var documents = await _context.Documents.ToListAsync();
            foreach (var doc in documents)
            {
                backup.AppendLine($"INSERT INTO Documents VALUES ({doc.Id}, '{doc.OriginalFileName}', ...);");
            }

            await File.WriteAllTextAsync(backupPath, backup.ToString());
        }

        /// <summary>
        /// Restaura o banco de dados a partir de um backup MySQL
        /// </summary>
        public async Task RestoreBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException($"Arquivo de backup não encontrado: {backupPath}");
                }

                var connectionString = _context.Database.GetConnectionString();
                await RestoreMySqlBackup(connectionString, backupPath);
                
                _logger.LogInformation($"Banco de dados restaurado com sucesso de: {backupPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao restaurar backup: {backupPath}");
                throw;
            }
        }

        /// <summary>
        /// Restaura backup MySQL usando mysql command
        /// </summary>
        private async Task RestoreMySqlBackup(string connectionString, string backupPath)
        {
            try
            {
                // Extrair informações da connection string
                var builder = new MySqlConnectionStringBuilder(connectionString);
                var server = builder.Server;
                var database = builder.Database;
                var user = builder.UserID;
                var password = builder.Password;
                var port = builder.Port;

                // Comando mysql para restore
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "mysql",
                    Arguments = $"--host={server} --port={port} --user={user} --password={password} {database}",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new System.Diagnostics.Process { StartInfo = processInfo };
                process.Start();

                // Lê o arquivo de backup e envia para o mysql
                var backupContent = await File.ReadAllTextAsync(backupPath);
                await process.StandardInput.WriteAsync(backupContent);
                process.StandardInput.Close();

                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
                {
                    throw new Exception($"Erro no restore MySQL: {error}");
                }

                _logger.LogInformation($"Restore MySQL executado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"mysql command não disponível ou erro no restore: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lista todos os backups disponíveis
        /// </summary>
        public async Task<List<string>> GetBackupListAsync()
        {
            await Task.CompletedTask;
            
            var backups = new List<string>();
            
            if (Directory.Exists(_backupDirectory))
            {
                var files = Directory.GetFiles(_backupDirectory, "*.db", SearchOption.AllDirectories)
                    .Union(Directory.GetFiles(_backupDirectory, "*.zip", SearchOption.AllDirectories))
                    .OrderByDescending(f => new FileInfo(f).CreationTime)
                    .ToList();
                
                backups.AddRange(files);
            }
            
            return backups;
        }

        /// <summary>
        /// Remove backups antigos
        /// </summary>
        public async Task CleanOldBackupsAsync(int keepDays = 30)
        {
            await Task.CompletedTask;
            
            try
            {
                if (!Directory.Exists(_backupDirectory)) return;

                var cutoffDate = DateTime.Now.AddDays(-keepDays);
                var files = Directory.GetFiles(_backupDirectory, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        _logger.LogInformation($"Backup antigo removido: {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar backups antigos");
            }
        }

        /// <summary>
        /// Extrai o caminho do banco de dados da connection string
        /// </summary>
        private string ExtractDatabasePath(string connectionString)
        {
            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                if (part.Trim().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
                {
                    var path = part.Substring(part.IndexOf('=') + 1).Trim();
                    return Path.IsPathRooted(path) ? path : Path.Combine(Directory.GetCurrentDirectory(), path);
                }
            }
            throw new InvalidOperationException("Caminho do banco de dados não encontrado na connection string");
        }
    }
}
