using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCNetAdmin.Models;

namespace MVCNetAdmin.Controllers
{
    public class AccessionCodesController : Controller
    {
        // GET: AccessionCodes/Create
        public String ViewTouchCodes()
        {
            AccessionCodes ac = new AccessionCodes();
            String codes = ac.ViewTouchCodes();
            return codes;
        }

        public String ViewALLCodes()
        {
            AccessionCodes ac = new AccessionCodes();
            String codes = ac.ViewALLCodes();
            return codes;
        }

        public String SearchCode(string code)
        {
            AccessionCodes ac = new AccessionCodes();
            String locations = ac.SearchCode(code);
            return locations;
        }


        
         public ActionResult AddRemove()
        {
            NetAdminContext db = new NetAdminContext();
            List<Location> lc = db.Location.ToList();
            TempData["locations"] = lc;
            return View();
        }
        
        public ActionResult AddRemoveCode(string site, string accession, string type, string isTouch = "")
        {
            NetAdminContext db = new NetAdminContext();
            Regex rgx = new Regex("[^a-zA-Z0-9,]");
            var result = db.AccessionCodes                                             //all existing accession codes
                               .Select(o => o.Code);

            List<String> siteCodes = db.AccLoc.Where(o => o.LocCode == site).Select(o => o.AccCode).ToList();
            List<String> touchList = db.AccessionCodes.Where(o => o.IsTouch == "Y").Select(o => o.Code).ToList();
            String[] codes = rgx.Replace(accession.Trim(), "").Split(',');
            String errorlog = "";
            if (type == "add")
            {
                foreach(String c in codes)
                {
                    if(touchList.Contains(c) && isTouch == "")
                    {
                        errorlog += c + " ";
                    }
                }
                if(errorlog=="")
                {
                    foreach (String c in codes)
                    {
                        if (!result.Contains(c))
                        {
                            AccessionCodes ac = new AccessionCodes();                    //new regular code only if it does not exist
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
                            AccLoc al = new AccLoc();                                             //location and code mapping
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
                            AccLoc al = new AccLoc();                                             //location and code mapping
                            al.LocCode = site.Trim();
                            al.AccCode = c;
                            al.CreatedAt = DateTime.Now;
                            al.UpdatedAt = DateTime.Now;
                            //al.LockVersion = 0;
                            db.AccLoc.Add(al);
                            db.SaveChanges();

                        }


                    }
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
                foreach (String c in codes)
                {
                    if (siteCodes.Contains(c))
                    {
                        AccLoc acl = db.AccLoc.Where(o => o.LocCode == site && o.AccCode == c).First();
                        db.Remove(acl);
                        db.SaveChanges();
                    }
                }

                //removing the unused codes
                List<String> accLocLeft = db.AccLoc.Select(o => o.AccCode).ToList();
                List<AccessionCodes> toBeDel = db.AccessionCodes.Where(o => !accLocLeft.Contains(o.Code)).ToList();
                foreach (AccessionCodes ac in toBeDel)
                {
                    db.AccessionCodes.Remove(ac);
                    db.SaveChanges();
                }

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
