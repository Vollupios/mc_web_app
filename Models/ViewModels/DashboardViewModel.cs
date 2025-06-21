namespace IntranetDocumentos.Models.ViewModels
{
    /// <summary>
    /// ViewModel principal para o dashboard de analytics
    /// </summary>
    public class DashboardViewModel
    {
        public DocumentStatisticsViewModel DocumentStatistics { get; set; } = new();
        public ReunioesMetricsViewModel ReunioesMetrics { get; set; } = new();
        public DepartmentActivityViewModel DepartmentActivity { get; set; } = new();    
    }

    /// <summary>
    /// Estatísticas de documentos
    /// </summary>
    public class DocumentStatisticsViewModel
    {
        public int TotalDocuments { get; set; }
        public int DocumentsThisMonth { get; set; }
        public int TotalDownloads { get; set; }
        public int DownloadsThisMonth { get; set; }
        public long TotalStorageUsed { get; set; } // em bytes
        public string TotalStorageFormatted => FormatBytes(TotalStorageUsed);
        
        public List<DocumentsByDepartmentViewModel> DocumentsByDepartment { get; set; } = new();
        public List<DocumentTypeStatViewModel> DocumentTypeStats { get; set; } = new();
        public List<TopDownloadedDocumentViewModel> TopDownloadedDocuments { get; set; } = new();
        public List<MonthlyDocumentStatsViewModel> MonthlyStats { get; set; } = new();

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
        }
    }

    /// <summary>
    /// Métricas de reuniões
    /// </summary>
    public class ReunioesMetricsViewModel
    {
        public int TotalReunioes { get; set; }
        public int ReunioesMesAtual { get; set; }
        public int ReunioesPendentes { get; set; }
        public int ReunioesPassadas { get; set; }
        public double TempoMedioReunioes { get; set; } // em minutos
        
        public List<ReuniaoPorTipoViewModel> ReuniaoPorTipo { get; set; } = new();
        public List<ReuniaoPorStatusViewModel> ReuniaoPorStatus { get; set; } = new();
        public List<ReuniaoPorDepartamentoViewModel> ReuniaoPorDepartamento { get; set; } = new();
        public List<ReuniaoPorMesViewModel> ReuniaoPorMes { get; set; } = new();
    }

    /// <summary>
    /// Atividade por departamento
    /// </summary>
    public class DepartmentActivityViewModel
    {
        public List<DepartmentStatsViewModel> DepartmentStats { get; set; } = new();
        public List<UserActivityViewModel> TopActiveUsers { get; set; } = new();
    }

    // Classes auxiliares para as estatísticas
    public class DocumentsByDepartmentViewModel
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
        public int DownloadCount { get; set; }
        public long StorageUsed { get; set; }
    }

    public class DocumentTypeStatViewModel
    {
        public string FileType { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class TopDownloadedDocumentViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int DownloadCount { get; set; }
        public DateTime LastDownload { get; set; }
    }

    public class MonthlyDocumentStatsViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int DocumentsUploaded { get; set; }
        public int TotalDownloads { get; set; }
    }

    public class ReuniaoPorTipoViewModel
    {
        public string TipoReuniao { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class ReuniaoPorStatusViewModel
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class ReuniaoPorDepartamentoViewModel
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class ReuniaoPorMesViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DepartmentStatsViewModel
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public int DocumentCount { get; set; }
        public int DownloadCount { get; set; }
        public int ReunioesCount { get; set; }
        public double ActivityScore { get; set; } // Pontuação calculada de atividade
    }

    public class UserActivityViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int DocumentsUploaded { get; set; }
        public int DocumentsDownloaded { get; set; }
        public int ReunioesCreated { get; set; }
        public double ActivityScore { get; set; }
    }
}
