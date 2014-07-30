using GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SimulatorUI.Controllers
{
    public class SimulationController : Controller
    {
       public ActionResult Index()
        {
            ViewBag.sent = MvcApplication.GlobalObserver.c_sent;
            ViewBag.errors = MvcApplication.GlobalObserver.c_errors;
            ViewBag.all_sent = MvcApplication.GlobalObserver.c_sent_requests;
            ViewBag.all_errors = MvcApplication.GlobalObserver.c_failed_requests;

            return View();
        }

        public async Task<ActionResult> Start()
        {
            int batch_count = int.Parse(Request.Params["batchcount"]);
            int batch_size = int.Parse(Request.Params["batchsize"]);
            int delay = int.Parse(Request.Params["delay"]);
            int runtime = int.Parse(Request.Params["runtime"]);
            string url = Request.Params["testurl"];

            // Controller
            IControllerGrain controller = ControllerGrainFactory.GetGrain(0);
            await controller.StartSimulation(batch_count, batch_size, delay, runtime, url);

            return RedirectToAction("index");
        }

        public async Task<ActionResult> Stop()
        {
            IControllerGrain controller = ControllerGrainFactory.GetGrain(0);
            await controller.StopSimulation();

            MvcApplication.GlobalObserver.c_sent = 0;

            return RedirectToAction("index", "home");
        }

        public async Task<ActionResult> SetVelocity(double velocity)
        {
            IControllerGrain controller = ControllerGrainFactory.GetGrain(0);
            await controller.SetVelocity(velocity);

            return RedirectToAction("index");
        }
    }
}