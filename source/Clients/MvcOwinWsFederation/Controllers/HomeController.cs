using System.Web;
using System.Web.Mvc;

namespace MvcOwinWsFederation.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Claims()
        {
            ViewBag.Message = "Claims";

            return View();
        }

        public ActionResult SignOut()
        {
            Request.GetOwinContext().Authentication.SignOut();
            // ReSharper disable once Mvc.ViewNotResolved
            return View();
        }
    }
}