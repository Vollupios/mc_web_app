using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace IntranetDocumentos.Controllers.Api
{
    /// <summary>
    /// Controller base para todos os endpoints da API REST
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Obtém o ID do usuário atual
        /// </summary>
        protected string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        /// <summary>
        /// Obtém o email do usuário atual
        /// </summary>
        protected string GetCurrentUserEmail()
        {
            return User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        }

        /// <summary>
        /// Verifica se o usuário atual é Admin
        /// </summary>
        protected bool IsCurrentUserAdmin()
        {
            return User.IsInRole("Admin");
        }

        /// <summary>
        /// Verifica se o usuário atual é Gestor
        /// </summary>
        protected bool IsCurrentUserGestor()
        {
            return User.IsInRole("Gestor");
        }

        /// <summary>
        /// Obtém os roles do usuário atual
        /// </summary>
        protected IEnumerable<string> GetCurrentUserRoles()
        {
            return User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        }

        /// <summary>
        /// Cria uma resposta de erro padronizada
        /// </summary>
        protected ActionResult CreateErrorResponse(string message, int statusCode = 400)
        {
            return StatusCode(statusCode, new
            {
                success = false,
                message = message,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Cria uma resposta de sucesso padronizada
        /// </summary>
        protected ActionResult CreateSuccessResponse<T>(T data, string message = "Operação realizada com sucesso")
        {
            return Ok(new
            {
                success = true,
                message = message,
                data = data,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Cria uma resposta paginada
        /// </summary>
        protected ActionResult CreatePaginatedResponse<T>(IEnumerable<T> items, int totalCount, int page, int pageSize)
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            return Ok(new
            {
                success = true,
                data = items,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                },
                timestamp = DateTime.UtcNow
            });
        }
    }
}
