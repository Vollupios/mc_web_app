namespace IntranetDocumentos.Application.DTOs.Common
{
    /// <summary>
    /// DTO base para todos os DTOs do sistema
    /// </summary>
    public abstract class BaseDTO
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO para resposta de operações
    /// </summary>
    public class OperationResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public object? Data { get; set; }
    }

    /// <summary>
    /// DTO para resposta de operações com tipo específico
    /// </summary>
    public class OperationResultDTO<T> : OperationResultDTO
    {
        public new T? Data { get; set; }
    }

    /// <summary>
    /// DTO para paginação
    /// </summary>
    public class PaginationDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    /// <summary>
    /// DTO para resposta paginada
    /// </summary>
    public class PagedResultDTO<T>
    {
        public List<T> Items { get; set; } = new();
        public PaginationDTO Pagination { get; set; } = new();
    }
}
