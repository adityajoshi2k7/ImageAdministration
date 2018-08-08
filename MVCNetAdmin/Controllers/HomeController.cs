using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MVCNetAdmin.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace MVCNetAdmin.Controllers
{
    [Authorize]
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

           //LDAP.Authenticate("joshia", "test");
            ViewData["Message"] = "Your contact page.";
            return View();
        }
        public ActionResult ManageForm()
        {

            List<Users> lc = db.Users.ToList();
            TempData["users"] = lc;
            return View();
        }



        public ActionResult AddRemoveUser(string userid, string type)
        {

            Regex rgx = new Regex("[^a-zA-Z0-9,]");
            var result = db.Users                                             //all existing user ids
                               .Select(o => o.UserId);
            String[] userids = rgx.Replace(userid.Trim(), "").Split(',');
            HashSet<String> set = new HashSet<string>();
            foreach (String s in userids)
                set.Add(s);
            String errorlog = "";
            if (type == "add")
            {
                foreach (String c in set)
                {
                    if (result.Contains(c))
                    {
                        errorlog += c + " ";
                    }
                }
                if (errorlog == "")
                {
                    foreach (String c in set)
                    {
                        
                            Users u = new Users(db);
                            u.UserId = c;
                            u.CreatedAt = DateTime.Now;
                            
                            db.Users.Add(u);
                            db.SaveChanges();
                       
                        

                    }
                }
                else
                {
                    errorlog = String.Join(",", errorlog.Trim().Split(" "));
                    TempData["userids"] = rgx.Replace(userid.Trim(), "");

                    List<Users> lc = db.Users.ToList();
                    TempData["users"] = lc;
                    ModelState.AddModelError("CustomError", "The following users are already registered" + "\n" + errorlog);
                    return View("ManageForm");
                }


            }
            else //remove
            {
                foreach (String c in set)
                {
                    if (result.Contains(c))
                    {
                        Users u = db.Users.Where(o => o.UserId == c).First();
                        db.Remove(u);
                        db.SaveChanges();
                    }
                }

                //removing the unused codes    NOW HANDLED BY TRIGGER
                //List<String> accLocLeft = db.AccLoc.Select(o => o.AccCode).ToList();
                //List<AccessionCodes> toBeDel = db.AccessionCodes.Where(o => !accLocLeft.Contains(o.Code)).ToList();
                //foreach (AccessionCodes ac in toBeDel)
                //{
                //    db.AccessionCodes.Remove(ac);
                //    db.SaveChanges();
                //}

            }

            TempData["msg"] = "Success";
            return RedirectToAction("Index", "Home");

        }





        public IActionResult Error()
        {

            
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
