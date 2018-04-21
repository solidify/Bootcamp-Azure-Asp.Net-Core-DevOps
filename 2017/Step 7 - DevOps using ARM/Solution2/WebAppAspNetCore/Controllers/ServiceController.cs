using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace WebAppAspNetCore.Controllers
{
    public class ServiceController : Controller
    {
        public IActionResult Index()
        {
            //try
            //{
                Services.SomeService.SomeWorkWithDependency();
                Services.SomeService.ThrowAnExceptionPlease();
            //}
            //catch (Exception ex)
            //{
            //    var client = new TelemetryClient();
            //    client.TrackException(ex);
            //}
            return View();
        }
    }
}