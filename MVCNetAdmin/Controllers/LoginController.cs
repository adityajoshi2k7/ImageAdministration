using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Labcorp.CustSvr.Base.Classes.Tools;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVCNetAdmin.Models;

namespace MVCNetAdmin.Controllers
{
    public class LoginController : Controller
    {
        static NetAdminContext db;
       
        string IPAddress;
        private IHttpContextAccessor _accessor;
        private IPAddress IP;
        public LoginController(NetAdminContext context, IHttpContextAccessor accessor)
        {

            db = context;
            _accessor = accessor;

            IP = _accessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4();
            IPAddress = IP.ToString();
        }
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult LoginForm([FromQuery]string ReturnUrl="")
        {
            if (ReturnUrl != "")
                TempData["ReturnUrl"] = ReturnUrl;
            return View();
        }

       
        public async Task<IActionResult> Logout()
        {
            UserLogs u = new UserLogs(db);
            u.LogDetails(_accessor.HttpContext.User.Claims.FirstOrDefault().Value, IPAddress, "Logged Out");
            await HttpContext.SignOutAsync();
            TempData["logout"] = "You have been logged out. Please close the browser tab for security purposes";
           

            return RedirectToAction("LoginForm", "Login");
        }

        public async Task<ActionResult> Authenticate(string username, string password,string returnURL="")
        {

           

            var authorizedUserIDs = db.Users.Select(o => o.UserId);   //authorized users
            if (!authorizedUserIDs.Contains(username))
            {
                TempData["loginfailed"] = "You are not Authorized to login. Please Contact Admin.";
                return RedirectToAction("LoginForm", "Login");
            }
            if (LDAP.Authenticate(username, password))
            {
                var claims = new List<Claim>   //create a Claims list.
                    {
                        new Claim(ClaimTypes.Name, username)
                    };
                //build an identity, a principal, and then set the cookie using the SignInAsync method.
                ClaimsIdentity userIdentity = new ClaimsIdentity(claims, "login");
                ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

                await HttpContext.SignInAsync(principal);
                if(returnURL!=null && returnURL.ToString() != "")
                {
                    //System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!1" + returnURL);
                    UserLogs u = new UserLogs(db);
                    u.LogDetails(username, IPAddress, "Logged In");
                    return Redirect(returnURL);
                }
                else
                {
                    UserLogs u = new UserLogs(db);
                    u.LogDetails(username, IPAddress, "Logged In");
                    TempData["msg"] = "Login Successfull";
                    return RedirectToAction("Index", "Home");
                }

               
            }

            else
            {
                TempData["loginfailed"] = "Invalid username/ password";
                return RedirectToAction("LoginForm", "Login");
            }
                


        }
    }
}