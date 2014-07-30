using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using GPSTracker.Common;
using GPSTracker.GrainInterface;

namespace GPSTracker.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}