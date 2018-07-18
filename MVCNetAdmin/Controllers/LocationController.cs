using System;
using System.Collections;

using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;
using MVCNetAdmin.Models;
using System.IO.Compression;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml;

namespace MVCNetAdmin.Controllers
{
    public class LocationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddNewForm()
        {
            return View();
        }
        [HttpPost]
        public JsonResult ViewSiteDetails(String expression)
        {

            System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@controller");
            System.Diagnostics.Debug.WriteLine(expression);
            Location loc = new Location();
            ArrayList al = loc.ViewSiteDetails(expression);
            return Json(al);

        }

        public IActionResult EditForm(String code)
        {
            Location loc = new Location();
            ArrayList al = loc.fetchSite(code);
            System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@controller");
            System.Diagnostics.Debug.WriteLine(al[0]);
            System.Diagnostics.Debug.WriteLine(al[1]);
            ViewData["location"] = al[0];
            ViewData["codes"] = al[1];
            return View();


        }



        public String PingAllSites()
        {


            Location loc = new Location();
            String errorlog = loc.PingAllSites();
            if (errorlog == "")
                return "All Sites successfully pinged.";


            return errorlog;

        }


        public String VerifyAll()
        {


            Location loc = new Location();
            String errorlog = loc.VerifyAll();
            return errorlog;

        }

        public String VerifySelected(string code)
        {


            Location loc = new Location();
            String errorlog = loc.VerifySelected(code);
            return errorlog;

        }



        public String PingSelectedSite(string server)
        {


            Location loc = new Location();
            String errorlog = loc.PingSelectedSite(server);
            return errorlog;

        }

        public ActionResult RemoveLocation(string site)
        {


            Location loc = new Location();
            loc.RemoveLocation(site);
            TempData["msg"] = "The site has been deleted successfully";
            return RedirectToAction("Index", "Home");


        }

        public ActionResult AddNewFTPForm()
        {


            return View();


        }

        public ActionResult ConvertToFTPForm(string code)
        {
            NetAdminContext db = new NetAdminContext();
            Location loc = db.Location.Where(o => o.Code == code).First();
            TempData["name"] = loc.Name;
            TempData["code"] = loc.Code;
            return PartialView();


        }



        public ActionResult RecoverDB()
        {
            return PartialView();


        }

        public string RecoverDBFinal(string path)
        {
           
               return  Location.Recover(path);
               
        }




        public ActionResult ConvertToFTP(string code, string host, string username, string password, string port, string directory = "")
        {
            NetAdminContext db = new NetAdminContext();
            Location loc = db.Location.Where(o => o.Code == code).First();
            if (username != null)
                loc.Username = username.Trim();
            if (password != null)
                loc.Passwrd = password.Trim();
            if (directory != null)
            {
                loc.DirectoryPath = directory.Trim();
            }
            loc.Host = host.Trim();
            loc.Port = port.Trim();
            loc.UpdatedAt = DateTime.Now;
            loc.IsFTP = "Y";
            db.SaveChanges();
            TempData["msg"] = "The site has been converted to FTP site";
            return RedirectToAction("Index", "Home");

        }


        public ActionResult AddNewFTPSite(string name, string code, string host, string username, string password, string port, string directory = "")
        {
            System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@username" + (username == null));
            System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@username" + (password == null) + (password == ""));
            System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@touchffgvg");
            NetAdminContext db = new NetAdminContext();
            Boolean flag = false;
            if (db.Location.Any(o => o.Code.Trim().Equals(code.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                flag = true;
            if (!flag)
            {
                System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@controller");
                Location l = new Location();
                l.Name = name.Trim();
                l.Code = code.Trim();
                l.Host = host.Trim();
                if (username != null)
                    l.Username = username.Trim();
                if (password != null)
                    l.Passwrd = password.Trim();
                l.Port = port.Trim();

                l.CreatedAt = DateTime.Now;
                l.UpdatedAt = DateTime.Now;
                l.IsFTP = "Y";

                if (directory != null)
                {
                    l.DirectoryPath = directory.Trim();
                }
                System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@controller");
                db.Location.Add(l);
                int res = db.SaveChanges();
                System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@controller" + res);
                TempData["msg"] = "The site has been added successfully";
                return RedirectToAction("Index", "Home");
            }
            TempData["code"] = code;
            TempData["name"] = name;
            TempData["username"] = username;
            TempData["password"] = password;
            TempData["host"] = host;
            TempData["directory"] = directory;
            TempData["port"] = port;
            ModelState.AddModelError("CustomError", "The Location Already exists. Please change the location code");
            return View("AddNewFTPForm");


        }

        public ActionResult AddNewSite(string name, string servername, string path, string code, string status, string diagnostic, string retention, string accession = "", string touch = "")
        {
            //System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@touch");
            System.Diagnostics.Debug.WriteLine(touch + touch == "");
            Regex rgx = new Regex("[^a-zA-Z0-9,]");
            NetAdminContext db = new NetAdminContext();
            
            Boolean flag = false;
            if (db.Location.Any(o => o.Code.Trim().Equals(code.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                flag = true;
            List<String> touchList = db.AccessionCodes.Where(o => o.IsTouch == "Y").Select(o => o.Code).ToList();
            //System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@touch"+touchList.ToString());
            if (accession != "" && accession != null)
            {
                String[] nontouchcodes = rgx.Replace(accession.Trim(), "").Split(',');
                //System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@touch" + nontouchcodes.ToString());
                String errorlog = "";
                foreach (String c in nontouchcodes)
                {
                    if (touchList.Contains(c))
                    {
                        //System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@entered");
                        errorlog += c + " ";
                        flag = true;
                    }
                }
                //System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@touch" + errorlog.ToString());
                if (errorlog != "")
                {
                    errorlog = String.Join(",", errorlog.Trim().Split(" "));
                    TempData["code"] = code;
                    TempData["name"] = name;
                    TempData["servername"] = servername;
                    TempData["path"] = path;
                    TempData["status"] = status;
                    TempData["diagnostic"] = diagnostic;
                    TempData["retention"] = retention;
                    if (accession != "" && accession != null)
                        TempData["accession"] = rgx.Replace(accession.Trim(), "");
                    if (touch != "" && touch != null)
                        TempData["touch"] = rgx.Replace(touch.Trim(), "");
                    ModelState.AddModelError("CustomError", "The following codes are touch codes and must be added as touch codes for this location" + "\n" + errorlog);
                    return View("AddNewForm");

                }

            }



            if (!flag)
            {
                Location l = new Location();
                l.Name = name.Trim();
                l.Server = servername.Trim();
                l.Path = path.Trim();
                l.Code = code.Trim();
                l.Status = status.Trim();
                l.DiagnosticLevel = Convert.ToInt32(diagnostic);
                l.RetentionPeriod = Convert.ToInt32(retention);
                //l.LockVersion = 0;
                l.CreatedAt = DateTime.Now;
                l.UpdatedAt = DateTime.Now;
                l.ConfigFileVersion = 0;
                l.IsFTP = "N";

                db.Location.Add(l);
                int res = db.SaveChanges();

                if (res > 0)
                {
                    var result = db.AccessionCodes
                                .Select(o => o.Code);
                    if (accession != "" && accession != null)
                    {
                        String[] codes = rgx.Replace(accession.Trim(), "").Split(',');
                        foreach (String c in codes)
                        {
                            if (!result.Contains(c))
                            {
                                AccessionCodes ac = new AccessionCodes();                    //new regular code only if it does not exist
                                ac.Code = c;
                                ac.IsTouch = "N";
                                ac.CreatedAt = DateTime.Now;
                                ac.UpdatedAt = DateTime.Now;
                                //ac.LockVersion = 0;
                                db.AccessionCodes.Add(ac);
                                db.SaveChanges();
                            }
                            AccLoc al = new AccLoc();                                             //location and code mapping
                            al.LocCode = code.Trim();
                            al.AccCode = c;
                            al.CreatedAt = DateTime.Now;
                            al.UpdatedAt = DateTime.Now;
                            //al.LockVersion = 0;
                            db.AccLoc.Add(al);
                            db.SaveChanges();

                        }
                    }
                    if (touch != "" && touch != null)
                    {
                        String[] touchCodes = rgx.Replace(touch.Trim(), "").Split(',');
                        foreach (String c in touchCodes)
                        {
                            if (!result.Contains(c))
                            {
                                AccessionCodes ac = new AccessionCodes();          //new touch code
                                ac.Code = c;
                                ac.IsTouch = "Y";
                                ac.CreatedAt = DateTime.Now;
                                ac.UpdatedAt = DateTime.Now;
                                // ac.LockVersion = 0;
                                db.AccessionCodes.Add(ac);
                                db.SaveChanges();
                            }
                            else
                            {
                                AccessionCodes old = db.AccessionCodes.Where(o => o.Code == c.Trim()).First();  //update the existing code as touch
                                old.IsTouch = "Y";
                                old.UpdatedAt = DateTime.Now;
                                db.SaveChanges();
                            }
                            AccLoc al = new AccLoc();
                            al.LocCode = code.Trim();
                            al.AccCode = c;
                            al.CreatedAt = DateTime.Now;
                            al.UpdatedAt = DateTime.Now;
                            //al.LockVersion = 0;
                            db.AccLoc.Add(al);
                            db.SaveChanges();

                        }

                    }


                }
                TempData["msg"] = "The site has been added successfully";
                return RedirectToAction("Index", "Home");

            }
            TempData["code"] = code;
            TempData["name"] = name;
            TempData["servername"] = servername;
            TempData["path"] = path;
            TempData["status"] = status;
            TempData["diagnostic"] = diagnostic;
            TempData["retention"] = retention;
            if (accession != "" && accession != null)
                TempData["accession"] = rgx.Replace(accession.Trim(), "");
            if (touch != "" && touch != null)
                TempData["touch"] = rgx.Replace(touch.Trim(), "");
            ModelState.AddModelError("CustomError", "The Location Already exists. Please change the location code");
            return View("AddNewForm");





        }


        public ActionResult EditSite(string name, string servername, string path, string code, string status, string diagnostic, string retention, string username, string password, string host, string port, string directory)
        {
            System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@controller");
            System.Diagnostics.Debug.WriteLine(code);
            System.Diagnostics.Debug.WriteLine(name);
            NetAdminContext db = new NetAdminContext();
            var result = db.Location.Where(o => o.Code.Trim() == code.Trim()).First();
            if (result != null)
            {
                result.Name = name.Trim();
                if (result.IsFTP == "N")
                {
                    result.Server = servername.Trim();
                    result.Path = path.Trim();
                    result.Status = status.Trim();
                    result.DiagnosticLevel = Convert.ToInt32(diagnostic.Trim());
                    result.RetentionPeriod = Convert.ToInt32(retention.Trim());
                }
                else
                {
                    result.Host = host.Trim();
                    result.Username = username == null ? null : username.Trim();
                    result.Passwrd = password == null ? null : password.Trim();
                    result.Port = port.Trim();
                    result.DirectoryPath = directory == null ? null : directory.Trim();
                }


                result.UpdatedAt = DateTime.Now;
                db.SaveChanges();
            }

            TempData["msg"] = "The site has been updated successfully";
            return RedirectToAction("Index", "Home");
        }


        public String PublishSelectedOrALL(string path = "", string code = "")
        {
            Location loc = new Location();
            String message = loc.PublishSelectedOrALL(path, code);

            return message;
        }






        public string ViewSelectedSite(string path) //downloads the zip of directory in the given path
        {
            var processInfo = new ProcessStartInfo
            {
                Arguments = path,
                //FileName = "explorer.exe",
                //UserName = Username,
                //Password = GetPasswordAsSecureString(),
                //Domain = Domain,
                UseShellExecute = false,
            };

            if (Directory.Exists(path))
            {
                Process.Start("explorer.exe", path);
                return "";
            }
            else
                return "The path is incorrect. Please check again";
            //string startPath =path;
            //string zipPath = @"sitedetails.zip";
            //System.IO.File.Delete(@"sitedetails.zip");

            //    if (!Directory.Exists(path))
            //    {
            //        String errorlog ="The path is incorrect." + "\n";
            //        TempData["msgfailure"] = "Something went wrong. Please try later." + "\n" + "Error: " + errorlog;
            //        return RedirectToAction("Index", "Home");

            //    }
            //    ZipFile.CreateFromDirectory(startPath, zipPath);
            //    byte[] fileBytes = System.IO.File.ReadAllBytes(@"sitedetails.zip");
            //    System.IO.File.Delete(@"sitedetails.zip");
            //    return File(fileBytes, "application/zip", "sitedetails.zip");



        }
        public string Ftp()
        {
            try
            {
                string directory = "/test";   //it should be there
                string filename = "/abc2.txt";  //the file which would be created there
                string host = "10.105.198.84";
                string port = "21";
                //FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://b2bgateway-staging.labcorp.com:30032/" + directory + "/" + filename);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + host + ":" + port + "/" + directory + filename);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential("", "");

                // Copy the contents of the file to the request stream.
                byte[] fileContents;
                using (StreamReader sourceStream = new StreamReader("testfile.txt"))
                {
                    fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                }

                request.ContentLength = fileContents.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileContents, 0, fileContents.Length);
                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    return $"Upload File Complete, status {response.StatusDescription}";
                }
            }
            catch (Exception e)
            {
                return "Error : " + e.Message;
            }

        }


    }
}
