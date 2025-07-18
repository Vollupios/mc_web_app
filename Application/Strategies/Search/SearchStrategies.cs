using IntranetDocumentos.Application.DTOs.Common;
using IntranetDocumentos.Application.DTOs.Documents;
using IntranetDocumentos.Application.Strategies;
using IntranetDocumentos.Models;
using System.Linq.Expressions;

namespace IntranetDocumentos.Application.Strategies.Search
{
    /// <summary>
    /// Contexto para estratégias de busca
    /// </summary>
    public class SearchContext
    {
        public string UserId { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public int UserDepartmentId { get; set; }
        public int PageSize { get; set; } = 10;
        public int MaxResults { get; set; } = 1000;
        public bool IncludeInactive { get; set; } = false;
    }

    /// <summary>
    /// Resultado da busca
    /// </summary>
    /// <typeparam name="T">Tipo do resultado</typeparam>
    public class SearchResult<T>
    {
        public IEnumerable<T> Results { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage => PageNumber * PageSize < TotalCount;
        public bool HasPreviousPage => PageNumber > 1;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Estratégia de busca simples por texto
    /// </summary>
    public class SimpleTextSearchStrategy : IContextStrategy<string, SearchResult<DocumentResponseDTO>, SearchContext>
    {
        public SearchResult<DocumentResponseDTO> Execute(string searchTerm, SearchContext context)
        {
            var result = new SearchResult<DocumentResponseDTO>();

            try
            {
                // Em implementação real, executar busca no banco
                // Aplicar filtros baseados no contexto do usuário
                // Implementar paginação

                result.Metadata["SearchTerm"] = searchTerm;
                result.Metadata["SearchExecutedAt"] = DateTime.UtcNow;
                result.Metadata["SearchExecutedBy"] = context.UserId;
            }
            catch (Exception ex)
            {
                result.Metadata["Error"] = ex.Message;
            }

            return result;
        }
    }

    /// <summary>
    /// Estratégia de busca avançada com filtros
    /// </summary>
    public class AdvancedSearchStrategy : IContextStrategy<SearchFilterDTO, SearchResult<DocumentResponseDTO>, SearchContext>
    {
        public SearchResult<DocumentResponseDTO> Execute(SearchFilterDTO filter, SearchContext context)
        {
            var result = new SearchResult<DocumentResponseDTO>();

            try
            {
                // Construir expressão de busca baseada nos filtros
                var searchExpression = BuildSearchExpression(filter, context);
                
                // Em implementação real, executar busca no banco
                // Aplicar ordenação, paginação e filtros de segurança

                result.PageNumber = filter.PageNumber;
                result.PageSize = filter.PageSize;
                result.Metadata["FilterApplied"] = true;
                result.Metadata["SearchExecutedAt"] = DateTime.UtcNow;
                result.Metadata["SearchExecutedBy"] = context.UserId;
            }
            catch (Exception ex)
            {
                result.Metadata["Error"] = ex.Message;
            }

            return result;
        }

        private Expression<Func<Document, bool>> BuildSearchExpression(SearchFilterDTO filter, SearchContext context)
        {
            // Construir expressão dinamicamente baseada nos filtros
            Expression<Func<Document, bool>> expression = d => true;

            // Aplicar filtros de segurança baseados no contexto do usuário
            if (context.UserRole != "Admin" && context.UserRole != "Gestor")
            {
                expression = expression.And(d => d.DepartmentId == context.UserDepartmentId || d.Department.Name == "Geral");
            }

            // Aplicar filtros do DTO
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                expression = expression.And(d => d.OriginalFileName.Contains(filter.SearchTerm) || 
                                               d.Description.Contains(filter.SearchTerm));
            }

            if (filter.DepartmentId.HasValue)
            {
                expression = expression.And(d => d.DepartmentId == filter.DepartmentId.Value);
            }

            if (filter.DateFrom.HasValue)
            {
                expression = expression.And(d => d.UploadDate >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                expression = expression.And(d => d.UploadDate <= filter.DateTo.Value);
            }

            if (!context.IncludeInactive)
            {
                expression = expression.And(d => d.IsActive);
            }

            return expression;
        }
    }

    /// <summary>
    /// Estratégia de busca por similaridade
    /// </summary>
    public class SimilaritySearchStrategy : IContextStrategy<DocumentResponseDTO, SearchResult<DocumentResponseDTO>, SearchContext>
    {
        public SearchResult<DocumentResponseDTO> Execute(DocumentResponseDTO referenceDocument, SearchContext context)
        {
            var result = new SearchResult<DocumentResponseDTO>();

            try
            {
                // Implementar busca por similaridade baseada em:
                // - Tipo de arquivo
                // - Tamanho similar
                // - Departamento
                // - Palavras-chave na descrição

                result.Metadata["ReferenceDocumentId"] = referenceDocument.Id;
                result.Metadata["SimilaritySearchExecutedAt"] = DateTime.UtcNow;
                result.Metadata["SearchExecutedBy"] = context.UserId;
            }
            catch (Exception ex)
            {
                result.Metadata["Error"] = ex.Message;
            }

            return result;
        }
    }

    /// <summary>
    /// Estratégia de busca com cache
    /// </summary>
    public class CachedSearchStrategy : IContextStrategy<SearchFilterDTO, SearchResult<DocumentResponseDTO>, SearchContext>
    {
        private readonly AdvancedSearchStrategy _baseStrategy;
        private readonly Dictionary<string, (SearchResult<DocumentResponseDTO> result, DateTime cachedAt)> _cache;
        private readonly TimeSpan _cacheExpiration;

        public CachedSearchStrategy(TimeSpan cacheExpiration)
        {
            _baseStrategy = new AdvancedSearchStrategy();
            _cache = new Dictionary<string, (SearchResult<DocumentResponseDTO>, DateTime)>();
            _cacheExpiration = cacheExpiration;
        }

        public SearchResult<DocumentResponseDTO> Execute(SearchFilterDTO filter, SearchContext context)
        {
            var cacheKey = GenerateCacheKey(filter, context);

            // Verificar cache
            if (_cache.ContainsKey(cacheKey))
            {
                var cached = _cache[cacheKey];
                if (DateTime.UtcNow - cached.cachedAt < _cacheExpiration)
                {
                    cached.result.Metadata["FromCache"] = true;
                    return cached.result;
                }
                else
                {
                    _cache.Remove(cacheKey);
                }
            }

            // Executar busca e armazenar no cache
            var result = _baseStrategy.Execute(filter, context);
            result.Metadata["FromCache"] = false;
            _cache[cacheKey] = (result, DateTime.UtcNow);

            return result;
        }

        private string GenerateCacheKey(SearchFilterDTO filter, SearchContext context)
        {
            // Gerar chave baseada nos parâmetros de busca
            var keyParts = new List<string>
            {
                context.UserId,
                filter.SearchTerm ?? string.Empty,
                filter.DepartmentId?.ToString() ?? string.Empty,
                filter.DateFrom?.ToString("yyyy-MM-dd") ?? string.Empty,
                filter.DateTo?.ToString("yyyy-MM-dd") ?? string.Empty,
                filter.PageNumber.ToString(),
                filter.PageSize.ToString()
            };

            return string.Join("|", keyParts);
        }
    }

    /// <summary>
    /// Estratégia de busca assíncrona
    /// </summary>
    public class AsyncSearchStrategy : IAsyncStrategy<SearchFilterDTO, SearchResult<DocumentResponseDTO>>
    {
        private readonly AdvancedSearchStrategy _baseStrategy;

        public AsyncSearchStrategy()
        {
            _baseStrategy = new AdvancedSearchStrategy();
        }

        public async Task<SearchResult<DocumentResponseDTO>> ExecuteAsync(SearchFilterDTO input, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var context = new SearchContext(); // Em implementação real, obter do contexto HTTP
                return _baseStrategy.Execute(input, context);
            }, cancellationToken);
        }
    }
}

/// <summary>
/// Extensões para Expression
/// </summary>
public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var parameter = expr1.Parameters[0];
        var body = Expression.AndAlso(expr1.Body, Expression.Invoke(expr2, parameter));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var parameter = expr1.Parameters[0];
        var body = Expression.OrElse(expr1.Body, Expression.Invoke(expr2, parameter));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
