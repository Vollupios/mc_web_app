using IntranetDocumentos.Application.DTOs.Common;
using IntranetDocumentos.Application.DTOs.Documents;
using IntranetDocumentos.Application.DTOs.Reunioes;
using IntranetDocumentos.Application.DTOs.Departments;

namespace IntranetDocumentos.Application.DTOs.Analytics
{
    /// <summary>
    /// DTO principal para dashboard de analytics
    /// </summary>
    public class DashboardAnalyticsDTO
    {
        public DocumentAnalyticsDTO DocumentAnalytics { get; set; } = new();
        public ReuniaoAnalyticsDTO ReuniaoAnalytics { get; set; } = new();
        public DepartmentAnalyticsDTO DepartmentAnalytics { get; set; } = new();
        public UserAnalyticsDTO UserAnalytics { get; set; } = new();
        public SystemAnalyticsDTO SystemAnalytics { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// DTO para analytics de documentos
    /// </summary>
    public class DocumentAnalyticsDTO
    {
        public int TotalDocuments { get; set; }
        public int DocumentsThisMonth { get; set; }
        public int DocumentsThisWeek { get; set; }
        public int DocumentsToday { get; set; }
        public int TotalDownloads { get; set; }
        public int DownloadsThisMonth { get; set; }
        public int DownloadsThisWeek { get; set; }
        public int DownloadsToday { get; set; }
        public long TotalStorageUsed { get; set; }
        public string TotalStorageFormatted => FormatBytes(TotalStorageUsed);
        public long StorageUsedThisMonth { get; set; }
        public double AverageFileSize { get; set; }
        public double GrowthRate { get; set; } // Percentual de crescimento mensal
        
        public List<DocumentsByDepartmentDTO> DocumentsByDepartment { get; set; } = new();
        public List<DocumentTypeStatDTO> DocumentTypeStats { get; set; } = new();
        public List<TopDownloadedDocumentDTO> TopDownloadedDocuments { get; set; } = new();
        public List<MonthlyDocumentStatDTO> MonthlyStats { get; set; } = new();
        public List<DailyDocumentStatDTO> DailyStats { get; set; } = new();
        public List<HourlyDocumentStatDTO> HourlyStats { get; set; } = new();
        
        private static string FormatBytes(long bytes)
        {
            const int unit = 1024;
            if (bytes < unit) return $"{bytes} B";
            int exp = (int)(Math.Log(bytes) / Math.Log(unit));
            return $"{bytes / Math.Pow(unit, exp):F2} {"KMGTPE"[exp - 1]}B";
        }
    }

    /// <summary>
    /// DTO para analytics de reuniões
    /// </summary>
    public class ReuniaoAnalyticsDTO
    {
        public int TotalReunioes { get; set; }
        public int ReunioesMesAtual { get; set; }
        public int ReunioesSemanaAtual { get; set; }
        public int ReunioesHoje { get; set; }
        public int ReunioesPendentes { get; set; }
        public int ReunioesRealizadas { get; set; }
        public int ReunioesCanceladas { get; set; }
        public double TempoMedioReunioes { get; set; } // em minutos
        public double TaxaComparecimento { get; set; }
        public double TaxaCancelamento { get; set; }
        public int TotalParticipantes { get; set; }
        public double MediaParticipantesPorReuniao { get; set; }
        
        public List<ReunioesByTipoDTO> ReunioesByTipo { get; set; } = new();
        public List<ReunioesByStatusDTO> ReunioesByStatus { get; set; } = new();
        public List<ReunioesByDepartmentDTO> ReunioesByDepartment { get; set; } = new();
        public List<ReunioesByMesDTO> ReunioesByMes { get; set; } = new();
        public List<ReunioesByWeekdayDTO> ReunioesByWeekday { get; set; } = new();
        public List<ReunioesByHourDTO> ReunioesByHour { get; set; } = new();
    }

    /// <summary>
    /// DTO para analytics de departamentos
    /// </summary>
    public class DepartmentAnalyticsDTO
    {
        public int TotalDepartments { get; set; }
        public int ActiveDepartments { get; set; }
        public List<DepartmentStatisticsDTO> DepartmentStats { get; set; } = new();
        public List<DepartmentActivityDTO> DepartmentActivity { get; set; } = new();
        public List<DepartmentComparisonDTO> DepartmentComparison { get; set; } = new();
        public DepartmentPerformanceDTO TopPerformingDepartment { get; set; } = new();
        public DepartmentPerformanceDTO LeastActiveDepartment { get; set; } = new();
    }

    /// <summary>
    /// DTO para analytics de usuários
    /// </summary>
    public class UserAnalyticsDTO
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int UsersOnline { get; set; }
        public double AverageSessionDuration { get; set; } // em minutos
        public DateTime? LastLogin { get; set; }
        public List<UserActivitySummaryDTO> TopActiveUsers { get; set; } = new();
        public List<UserLoginStatsDTO> LoginStats { get; set; } = new();
        public List<UserByRoleDTO> UsersByRole { get; set; } = new();
        public List<UserByDepartmentDTO> UsersByDepartment { get; set; } = new();
    }

    /// <summary>
    /// DTO para analytics do sistema
    /// </summary>
    public class SystemAnalyticsDTO
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public long TotalRequests { get; set; }
        public long RequestsThisMonth { get; set; }
        public long ErrorsThisMonth { get; set; }
        public double ErrorRate { get; set; }
        public double AverageResponseTime { get; set; } // em milissegundos
        public List<SystemHealthMetricDTO> HealthMetrics { get; set; } = new();
        public List<SystemErrorLogDTO> RecentErrors { get; set; } = new();
        public List<SystemPerformanceDTO> PerformanceMetrics { get; set; } = new();
    }

    /// <summary>
    /// DTO para estatísticas diárias de documentos
    /// </summary>
    public class DailyDocumentStatDTO
    {
        public DateTime Date { get; set; }
        public int DocumentsUploaded { get; set; }
        public int TotalDownloads { get; set; }
        public long StorageUsed { get; set; }
        public int UniqueUsers { get; set; }
    }

    /// <summary>
    /// DTO para estatísticas horárias de documentos
    /// </summary>
    public class HourlyDocumentStatDTO
    {
        public int Hour { get; set; }
        public int DocumentsUploaded { get; set; }
        public int TotalDownloads { get; set; }
        public int UniqueUsers { get; set; }
        public double AverageResponseTime { get; set; }
    }

    /// <summary>
    /// DTO para reuniões por dia da semana
    /// </summary>
    public class ReunioesByWeekdayDTO
    {
        public int Weekday { get; set; }
        public string WeekdayName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public double AverageDuration { get; set; }
    }

    /// <summary>
    /// DTO para reuniões por hora
    /// </summary>
    public class ReunioesByHourDTO
    {
        public int Hour { get; set; }
        public string HourRange { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public double AverageDuration { get; set; }
    }

    /// <summary>
    /// DTO para comparação de departamentos
    /// </summary>
    public class DepartmentComparisonDTO
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public double DocumentsScore { get; set; }
        public double ReunioesScore { get; set; }
        public double UsersScore { get; set; }
        public double OverallScore { get; set; }
        public int Ranking { get; set; }
        public string PerformanceLevel { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para performance de departamento
    /// </summary>
    public class DepartmentPerformanceDTO
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public double ActivityScore { get; set; }
        public int DocumentCount { get; set; }
        public int ReuniaoCount { get; set; }
        public int UserCount { get; set; }
        public long StorageUsed { get; set; }
        public DateTime? LastActivity { get; set; }
    }

    /// <summary>
    /// DTO para resumo de atividade do usuário
    /// </summary>
    public class UserActivitySummaryDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int DocumentsUploaded { get; set; }
        public int DocumentsDownloaded { get; set; }
        public int ReunioesCriadas { get; set; }
        public int ReunioesPadticipadas { get; set; }
        public double ActivityScore { get; set; }
        public DateTime? LastActivity { get; set; }
        public int LoginCount { get; set; }
        public double AverageSessionDuration { get; set; }
    }

    /// <summary>
    /// DTO para estatísticas de login do usuário
    /// </summary>
    public class UserLoginStatsDTO
    {
        public DateTime Date { get; set; }
        public int LoginCount { get; set; }
        public int UniqueUsers { get; set; }
        public double AverageSessionDuration { get; set; }
        public int FailedLogins { get; set; }
    }

    /// <summary>
    /// DTO para usuários por role
    /// </summary>
    public class UserByRoleDTO
    {
        public string Role { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// DTO para usuários por departamento
    /// </summary>
    public class UserByDepartmentDTO
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// DTO para métricas de saúde do sistema
    /// </summary>
    public class SystemHealthMetricDTO
    {
        public string MetricName { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public double? Threshold { get; set; }
        public bool IsHealthy { get; set; }
    }

    /// <summary>
    /// DTO para log de erros do sistema
    /// </summary>
    public class SystemErrorLogDTO
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string? StackTrace { get; set; }
        public string Level { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? RequestPath { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    /// <summary>
    /// DTO para métricas de performance do sistema
    /// </summary>
    public class SystemPerformanceDTO
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public long RequestCount { get; set; }
        public double AverageResponseTime { get; set; }
        public int ErrorCount { get; set; }
        public int ActiveUsers { get; set; }
    }

    /// <summary>
    /// DTO para filtros de analytics
    /// </summary>
    public class AnalyticsFilterDTO
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DepartmentId { get; set; }
        public string? UserId { get; set; }
        public List<string> Metrics { get; set; } = new();
        public string? Granularity { get; set; } = "day"; // hour, day, week, month
        public int? TopN { get; set; } = 10;
    }

    /// <summary>
    /// DTO para exportação de analytics
    /// </summary>
    public class AnalyticsExportDTO
    {
        public string ReportName { get; set; } = string.Empty;
        public string Format { get; set; } = "xlsx"; // xlsx, csv, pdf
        public AnalyticsFilterDTO Filters { get; set; } = new();
        public List<string> Sections { get; set; } = new();
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeRawData { get; set; } = false;
    }
}
