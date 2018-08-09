using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCNetAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace MVCNetAdmin.Controllers
{
    [Authorize]
    public class AccessionCodesController : Controller
    {

        static NetAdminContext db;
        string currentUserID;
        string IPAddress;
        private IHttpContextAccessor _accessor;
        private IPAddress IP;
        public AccessionCodesController(NetAdminContext context, IHttpContextAccessor accessor)
        {
            _accessor = accessor;
            currentUserID = _accessor.HttpContext.User.Claims.FirstOrDefault().Value;
            IP = _accessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4();
            IPAddress = IP.ToString();
            db = context;
        }
        // GET: AccessionCodes/Create
        public String ViewTouchCodes()
        {
            AccessionCodes ac = new AccessionCodes(db);
            String codes = ac.ViewTouchCodes();
            return codes;
        }

        public String ViewALLCodes()
        {
            AccessionCodes ac = new AccessionCodes(db);
            String codes = ac.ViewALLCodes();
            return codes;
        }

        public String SearchCode(string code)
        {
            AccessionCodes ac = new AccessionCodes(db);
            String locations = ac.SearchCode(code);
            return locations;
        }


        
         public ActionResult AddRemove()
        {
           
            List<Location> lc = db.Location.ToList();
            TempData["locations"] = lc;
            return View();
        }
        
        public ActionResult AddRemoveCode(string site, string accession, string type, string isTouch = "")
        {
            
            Regex rgx = new Regex("[^a-zA-Z0-9,]");
            var result = db.AccessionCodes                                             //all existing accession codes
                               .Select(o => o.Code);

            List<String> siteCodes = db.AccLoc.Where(o => o.LocCode == site).Select(o => o.AccCode).ToList();
            List<String> touchList = db.AccessionCodes.Where(o => o.IsTouch == "Y").Select(o => o.Code).ToList();
            String[] codes = rgx.Replace(accession.Trim(), "").Split(',');
            HashSet<String> set = new HashSet<string>();
            foreach (String s in codes)
                set.Add(s);
            String errorlog = "";
            if (type == "add")
            {
                foreach(String c in set)
                {
                    if(touchList.Contains(c) && isTouch == "")
                    {
                        errorlog += c + " ";
                    }
                }
                if(errorlog=="")
                {
                    string added = "";
                    foreach (String c in set)
                    {
                        added += c + " ";
                        if (!result.Contains(c))
                        {
                            AccessionCodes ac = new AccessionCodes(db);                    //new regular code only if it does not exist
                            ac.Code = c;
                            if (isTouch != "")
                                ac.IsTouch = "Y";
                            else
                                ac.IsTouch = "N";
                            ac.CreatedAt = DateTime.Now;
                            ac.UpdatedAt = DateTime.Now;
                            //ac.LockVersion = 0;
                            db.AccessionCodes.Add(ac);
                            db.SaveChanges();
                            
                            AccLoc al = new AccLoc(db);                                             //location and code mapping
                            al.LocCode = site.Trim();
                            al.AccCode = c;
                            al.CreatedAt = DateTime.Now;
                            al.UpdatedAt = DateTime.Now;
                            //al.LockVersion = 0;
                            db.AccLoc.Add(al);
                            db.SaveChanges();
                        }
                        else if (siteCodes.Contains(c))
                        {
                            if (isTouch == "y")
                            {
                                if (!touchList.Contains(c))
                                {
                                    AccessionCodes acode = db.AccessionCodes.Where(o => o.Code == c).First();
                                    acode.IsTouch = "Y";
                                    db.SaveChanges();
                                }
                            }
                        }
                        else if (!siteCodes.Contains(c))
                        {
                            if (isTouch == "y")
                            {
                                if (!touchList.Contains(c))
                                {
                                    AccessionCodes acode = db.AccessionCodes.Where(o => o.Code == c).First();
                                    acode.IsTouch = "Y";
                                    db.SaveChanges();
                                }
                            }
                            AccLoc al = new AccLoc(db);                                             //location and code mapping
                            al.LocCode = site.Trim();
                            al.AccCode = c;
                            al.CreatedAt = DateTime.Now;
                            al.UpdatedAt = DateTime.Now;
                            //al.LockVersion = 0;
                            db.AccLoc.Add(al);
                            db.SaveChanges();

                        }


                    }
                    
                    
                    added= String.Join(",", added.Trim().Split(" "));
                    UserLogs u = new UserLogs(db);
                    u.LogDetails(currentUserID, IPAddress, "Added accession codes: "+ added + " at location : " + site);
                }
                else
                {
                    errorlog = String.Join(",", errorlog.Trim().Split(" "));
                    TempData["accession"]= rgx.Replace(accession.Trim(), "");
                    TempData["site"] = site.Trim();
                    List<Location> lc = db.Location.ToList();
                    TempData["locations"] = lc;
                    ModelState.AddModelError("CustomError", "The following codes are touch codes and must be added as touch codes for this location" + "\n" + errorlog);
                    return View("AddRemove");
                }

                
            }
            else //remove
            {
                string removed = "";
                foreach (String c in set)
                {
                    
                    if (siteCodes.Contains(c))
                    {
                        AccLoc acl = db.AccLoc.Where(o => o.LocCode == site && o.AccCode == c).First();
                        db.Remove(acl);
                        db.SaveChanges();
                        removed += c + " ";
                    }
                }
                
                removed = String.Join(",", removed.Trim().Split(" "));
                UserLogs u = new UserLogs(db);
                u.LogDetails(currentUserID, IPAddress, "Removed accession codes: " + removed + " at location : " + site);

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



        // POST: AccessionCodes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Code,IsTouch,CreatedAt,UpdatedAt,LockVersion")] AccessionCodes accessionCodes)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(accessionCodes);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(accessionCodes);
        //}
    }
}
