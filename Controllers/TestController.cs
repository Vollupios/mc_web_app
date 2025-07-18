using Microsoft.AspNetCore.Mvc;

namespace IntranetDocumentos.Controllers
{
    public class TestController : Controller
    {
        public ActionResult FooterPequeno()
        {
            return View();
        }

        public ActionResult FooterLongo()
        {
            return View();
        }
        
        public ActionResult FooterFinal()
        {
            return View();
        }
    }
}
