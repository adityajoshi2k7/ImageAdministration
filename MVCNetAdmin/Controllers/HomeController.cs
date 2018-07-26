using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MVCNetAdmin.Models;
using Microsoft.Extensions.DependencyInjection;
namespace MVCNetAdmin.Controllers
{
    public class HomeController : Controller
    {
        static NetAdminContext db;

        public HomeController(NetAdminContext context)
        {

            db = context;
        }
        public IActionResult Index()
        {

            String message = "";
            if (TempData["msg"] != null)
                message = TempData["msg"].ToString();

            Location loc = new Location(db);
            List<Location> result = loc.FetchAllLocations();
            ViewData["locations"] = result;
            TempData["msg"] = message;
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
