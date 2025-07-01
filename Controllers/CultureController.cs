using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace IntranetDocumentos.Controllers
{
    public class CultureController : Controller
    {
        /// <summary>
        /// Altera o idioma da aplicação e redireciona para a página de origem
        /// </summary>
        /// <param name="culture">Código da cultura (pt-BR, en-US, etc.)</param>
        /// <param name="returnUrl">URL para retornar após a mudança de idioma</param>
        [HttpPost]
        public IActionResult SetCulture(string culture, string returnUrl)
        {
            // Validar se a cultura é suportada
            var supportedCultures = new[] { "pt-BR", "en-US" };
            if (!supportedCultures.Contains(culture))
            {
                culture = "pt-BR"; // Fallback para português
            }

            // Definir cookie de cultura
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions 
                { 
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                }
            );

            // Retornar para a página de origem ou Home se não especificada
            return LocalRedirect(returnUrl ?? "~/");
        }

        /// <summary>
        /// Obtém a cultura atual via GET (útil para JavaScript)
        /// </summary>
        [HttpGet]
        public IActionResult GetCulture()
        {
            var culture = System.Globalization.CultureInfo.CurrentUICulture.Name;
            return Json(new { culture = culture });
        }
    }
}
