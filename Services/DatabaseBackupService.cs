using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IntranetDocumentos.Data;
using System.IO;
using System.IO.Compression;

namespace IntranetDocumentos.Services
{
    /// <summary>
    /// Serviço para backup e restauração do banco de dados SQLite
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
        private readonly string _databasePath;

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
            }            // Caminho do banco de dados
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            _databasePath = ExtractDatabasePath(connectionString ?? "Data Source=IntranetDocumentos.db");
        }

        /// <summary>
        /// Cria um backup manual do banco de dados
        /// </summary>
        public async Task<string> CreateBackupAsync()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"IntranetDocumentos_Backup_{timestamp}.db";
                var backupPath = Path.Combine(_backupDirectory, backupFileName);

                // Força o flush de todas as transações pendentes
                await _context.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(FULL);");
                
                // Copia o arquivo do banco
                File.Copy(_databasePath, backupPath, overwrite: true);

                // Cria também um backup comprimido
                var zipPath = Path.Combine(_backupDirectory, $"IntranetDocumentos_Backup_{timestamp}.zip");
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(_databasePath, "IntranetDocumentos.db");
                    
                    // Adiciona informações do backup
                    var infoEntry = zip.CreateEntry("backup_info.txt");
                    using (var stream = infoEntry.Open())
                    using (var writer = new StreamWriter(stream))
                    {
                        await writer.WriteLineAsync($"Backup criado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                        await writer.WriteLineAsync($"Versão da aplicação: 1.0");
                        await writer.WriteLineAsync($"Tamanho do banco: {new FileInfo(_databasePath).Length} bytes");
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
                var backupFileName = $"IntranetDocumentos_Auto_{timestamp}.db";
                var backupPath = Path.Combine(_backupDirectory, "Auto", backupFileName);
                
                var autoBackupDir = Path.Combine(_backupDirectory, "Auto");
                if (!Directory.Exists(autoBackupDir))
                {
                    Directory.CreateDirectory(autoBackupDir);
                }

                // Força checkpoint do WAL
                await _context.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(FULL);");
                
                File.Copy(_databasePath, backupPath, overwrite: true);
                
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
        /// Restaura o banco de dados a partir de um backup
        /// </summary>
        public async Task RestoreBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException($"Arquivo de backup não encontrado: {backupPath}");
                }

                // Para as conexões ativas
                await _context.Database.CloseConnectionAsync();
                
                // Aguarda um pouco para garantir que as conexões foram fechadas
                await Task.Delay(1000);

                // Faz backup do arquivo atual antes de restaurar
                var currentBackup = Path.Combine(_backupDirectory, $"Pre_Restore_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                if (File.Exists(_databasePath))
                {
                    File.Copy(_databasePath, currentBackup, overwrite: true);
                }

                // Restaura o backup
                File.Copy(backupPath, _databasePath, overwrite: true);
                  // Testa a conexão com o banco restaurado
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();
                
                _logger.LogInformation($"Banco de dados restaurado com sucesso de: {backupPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao restaurar backup: {backupPath}");
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
