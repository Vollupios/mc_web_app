using Microsoft.AspNetCore.Mvc;

namespace IntranetDocumentos.Controllers
{
    public class TestController : Controller
    {
        public IActionResult FooterPequeno()
        {
            return View();
        }

        public IActionResult FooterLongo()
        {
            return View();
        }
        
        public IActionResult FooterFinal()
        {
            return View();
        }
    }
}
