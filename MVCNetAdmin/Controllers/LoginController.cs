using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MVCNetAdmin.Controllers
{
    public class LoginController : Controller
    {
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
            await HttpContext.SignOutAsync();
            TempData["logout"] = "You have been logged out. Please close the browser tab for security purposes";

            return RedirectToAction("LoginForm", "Login");
        }

        public async Task<ActionResult> Authenticate(string username, string password,string returnURL="")
        {

            Boolean flag = true;
            //flag = LDAP.Authenticate(username, password);
            if (flag)
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
                    return Redirect(returnURL);
                }
                else
                {
                    TempData["msg"] = "Login Successfull";
                    return RedirectToAction("Index", "Home");
                }

               
            }

            else
            {
                TempData["loginfailed"] = "Invalid username/password";
                return RedirectToAction("LoginForm", "Login");
            }
                


        }
    }
}