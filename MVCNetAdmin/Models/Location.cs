using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace MVCNetAdmin.Models
{
    public partial class Location
    {
        static NetAdminContext db;
        public Location(NetAdminContext context)
        {
            db = context;
            AccLoc = new HashSet<AccLoc>();
        }

        public Location()
        {
        }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Status { get; set; }

        public string Server { get; set; }

        public string Path { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string DirectoryPath { get; set; }
        public string Passwrd { get; set; }
        public string IsFTP { get; set; }
        public int? RetentionPeriod { get; set; }
        public int? DiagnosticLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        //public int LockVersion { get; set; }
        public int ConfigFileVersion { get; set; }
        public ICollection<AccLoc> AccLoc { get; set; }

         static List<String>  ListOfCodes(Location l)
        {
            List<String> codeList = db.AccLoc.Where(o => o.LocCode == l.Code).Select(o => o.AccCode).ToList();
            return codeList;

        }

        public  List<Location> FetchAllLocations()   //View Touch Codes
        {
            List<Location> al = new List<Location>();
            var result = db.Location.Select(o => new { o.Code, o.Name, o.IsFTP });
            foreach (var res in result)
            {
                Location loc = new Location();
                loc.Name = res.Name;
                loc.Code = res.Code;
                loc.IsFTP = res.IsFTP;
                al.Add(loc);
            }
            return al;
        }
        internal  string Recover(string xmlpath)
        {
            try
            {
               
                db.Database.ExecuteSqlCommand("delete from AccLoc");
                db.Database.ExecuteSqlCommand("delete from AccessionCodes");
                db.Database.ExecuteSqlCommand("delete from Location");


                string fullpath = xmlpath ;
                System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@");
                if (!(File.Exists(fullpath)))
                {
                    return "No XML file found at given path. Please check the path";
                }
                else
                {
                    XElement xelement = XElement.Load(fullpath);
                    System.Diagnostics.Debug.WriteLine(fullpath);
                    IEnumerable<XElement> locations = xelement.Elements();
                    foreach (var location in locations)
                    {
                        Location l = new Location();
                        l.Name = location.Element("name").Value;
                        if (location.Element("status").Value != null && location.Element("status").Value.Trim() != "")
                            l.Status = location.Element("status").Value;

                        l.Code = location.Element("code").Value;
                        if (location.Element("path").Value != null && location.Element("path").Value.Trim() != "")
                            l.Path = location.Element("path").Value;
                        if (location.Element("server").Value != null && location.Element("server").Value.Trim() != "")
                            l.Server = location.Element("server").Value;
                        if (location.Element("retentionperiod") != null && location.Element("retentionperiod").Value.Trim() != "")
                            l.RetentionPeriod = Convert.ToInt32(location.Element("retentionperiod").Value);
                        if (location.Element("diagnosticlevel") != null && location.Element("diagnosticlevel").Value.Trim() != "")
                            l.DiagnosticLevel = Convert.ToInt32(location.Element("diagnosticlevel").Value);
                        if (location.Element("username") != null && location.Element("username").Value.Trim() != "")
                            l.Username = location.Element("username").Value;
                        if (location.Element("password") != null && location.Element("password").Value.Trim() != "")
                            l.Passwrd = location.Element("password").Value;
                        if (location.Element("host") != null && location.Element("host").Value.Trim() != "")
                            l.Host = location.Element("host").Value;
                        if (location.Element("port") != null && location.Element("port").Value.Trim() != "")
                            l.Port = location.Element("port").Value;
                        if (location.Element("directory") != null && location.Element("directory").Value.Trim() != "")
                            l.DirectoryPath = location.Element("directory").Value;
                        if (location.Element("isFTP") != null && location.Element("isFTP").Value.Trim() != "")
                            l.IsFTP = location.Element("isFTP").Value;
                        else
                            l.IsFTP = "N";

                        if (location.Element("configfileversion") != null && location.Element("configfileversion").Value.Trim() != "")
                            l.ConfigFileVersion = Convert.ToInt32(location.Element("configfileversion").Value);


                        l.CreatedAt = DateTime.Now;
                        l.UpdatedAt = DateTime.Now;
                        db.Location.Add(l);
                        db.SaveChanges();
                        if (location.Elements("accession") != null)
                        {

                            foreach (var ac in location.Elements("accession").GroupBy(e => e.Value).Select(x => x.First()))
                            {

                                if(ac!=null && ac.Value.Trim() != "")
                                {
                                    AccessionCodes existing = db.AccessionCodes.Where(o => o.Code == ac.Value.Trim()).FirstOrDefault();
                                    if (existing == null)
                                    {
                                        AccessionCodes acode = new AccessionCodes();
                                        acode.Code = ac.Value.Trim();
                                        acode.IsTouch = ac.Attribute("isTouch")==null?"N": ac.Attribute("isTouch").Value.Trim();
                                        acode.CreatedAt = DateTime.Now;
                                        acode.UpdatedAt = DateTime.Now;
                                        db.AccessionCodes.Add(acode);
                                        db.SaveChanges();
                                    }

                                    AccLoc acl = new AccLoc();
                                    acl.LocCode = location.Element("code").Value;
                                    acl.AccCode = ac.Value;
                                    acl.CreatedAt = DateTime.Now;
                                    acl.UpdatedAt = DateTime.Now;
                                    db.AccLoc.Add(acl);

                                    db.SaveChanges();
                                }
                                
                            }

                        }


                    }




                    return "Database Restored Successfully. Please refresh the page.";

                }
            }
            catch (Exception e)
            {
                return e.Message;
            }



        }

        internal string VerifyAll()
        {

            List<Location> locations = db.Location.ToList();
            int latestVersion = db.Location.Select(o => o.ConfigFileVersion).Max();
            String errorlog = "";

            foreach (Location lc in locations)
            {
                try
                {
                    if (lc.IsFTP == "N")
                    {
                        if (!Directory.Exists(lc.Path))
                        {
                            errorlog += "\n" + "Location: " + lc.Name + " \nStatus: The path is incorrect." + "\n";

                        }
                        else
                        {
                            if (!(File.Exists(lc.Path + "\\" + "accLoc.xml") && File.Exists(lc.Path + "\\" + "accLoc.ini")))
                            {
                                errorlog += "\n" + "Location: " + lc.Name + " \nStatus: The config files are not present." + "\n";
                            }
                            else
                            {
                                if (lc.ConfigFileVersion != latestVersion)
                                {
                                    errorlog += "\n" + "Location: " + lc.Name + " \nStatus: The config files are outdated." + "\n";
                                }

                            }
                        }
                    }
                    else
                    {
                        if (CheckIfFileExistsOnServer("accLoc.xml", lc) && CheckIfFileExistsOnServer("accLoc.ini", lc))
                        {
                            if (lc.ConfigFileVersion != latestVersion)
                            {
                                errorlog += "\n" + "Location: " + lc.Name + " \nStatus: The config files are outdated." + "\n";
                            }

                        }


                    }


                }
                catch (Exception e)
                {
                    errorlog += "\n" + "Location " + lc.Name + " \nStatus: Some error occured." + "\n" + "Error Message : " + e.Message + "\n";
                }
            }
            if (errorlog == "")
                return "All sites verified successfully. All sites have latest configuration files. ";
            else
                return "Except for the below sites, all sites have latest configuration files." + "\n" + "\n" + errorlog;
        }

        internal string VerifySelected(string code)
        {
            Location location = db.Location.Where(o => o.Code == code).First();
            int latestVersion = db.Location.Select(o => o.ConfigFileVersion).Max();
            String errorlog = "";
            try
            {
                if (location.IsFTP == "N")
                {
                    if (!Directory.Exists(location.Path))
                    {
                        errorlog += "Location: " + location.Name + " \nStatus: The path is incorrect." + "\n";

                    }
                    else
                    {
                        if (!(File.Exists(location.Path + "\\" + "accLoc.xml") && File.Exists(location.Path + "\\" + "accLoc.ini")))
                        {
                            errorlog += "Location: " + location.Name + " \nStatus: The config files are not present." + "\n";
                        }
                        else
                        {
                            if (location.ConfigFileVersion != latestVersion)
                            {
                                errorlog += "Location: " + location.Name + " \nStatus: The config files are outdated." + "\n";
                            }

                        }
                    }
                }
                else
                {
                    if (CheckIfFileExistsOnServer("accLoc.xml", location) && CheckIfFileExistsOnServer("accLoc.ini", location))
                    {
                        if (location.ConfigFileVersion != latestVersion)
                        {
                            errorlog += "Location: " + location.Name + " \nStatus: The config files are outdated." + "\n";
                        }

                    }


                }

            }
            catch (Exception e)
            {
                errorlog += "Location " + location.Name + " \nStatus: Some error occured. Error Message : " + e.Message + "\n";
            }

            if (errorlog == "")
                return "The site has latest configuration files. ";
            else
                return errorlog;
        }

        public ArrayList ViewSiteDetails(string a)
        {
            //System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@22");
            //System.Diagnostics.Debug.WriteLine(a);
            Location loc = db.Location.Where(o => o.Code == a).First();
            var accDetails = from r in db.AccLoc
                             join p in db.AccessionCodes on r.AccCode equals p.Code
                             where r.LocCode == a
                             select new { code = r.AccCode, isTouch = p.IsTouch };
            ArrayList l = new ArrayList();
            l.Add(loc);
            l.Add(accDetails);
            return l;





        }
        public ArrayList fetchSite(string a)
        {
            
            Location loc = db.Location.Where(o => o.Code.Equals(a.Trim())).First();
            var accDetails = from r in db.AccLoc
                             join p in db.AccessionCodes on r.AccCode equals p.Code
                             where r.LocCode == a
                             select new { code = r.AccCode };
            String str = "";
            foreach (var v in accDetails)
            {
                str += v.code.ToString().Trim() + " ";
            }
            str = String.Join(",", str.Trim().Split(" "));


            System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!");
            System.Diagnostics.Debug.WriteLine(str);
            ArrayList al = new ArrayList();
            al.Add(loc);
            al.Add(str);
            return al;





        }

        public String PublishSelectedOrALL(string path = "", string sitecode = "")   //writing xml and ini files for a selected site or all locations
        {
            List<Location> lc = db.Location.ToList();
            XmlWriter xmlWriter;
            IniFile iniFile;
            string arcPath = "";
            int latestVersion = db.Location.Select(o => o.ConfigFileVersion).Max();
            Location archive = db.Location.Where(o => (o.Code == "ARC")).FirstOrDefault();
            if (archive != null)
                arcPath = archive.Path;
            System.Diagnostics.Debug.WriteLine("##############" + arcPath);
            string tempPath = System.IO.Path.GetTempPath();
            if (sitecode != "")   //publish at a given site
            {
                try
                {
                    Location currloc = db.Location.Where(o => o.Code == sitecode).First();
                    if (currloc.IsFTP == "Y")
                    {

                        File.Delete(tempPath + "accLoc.ini");
                        File.Delete(tempPath + "accLoc.xml");
                        System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@22" + tempPath);
                        iniFile = new IniFile(tempPath + "accLoc.ini");
                        xmlWriter = XmlWriter.Create(tempPath + "accLoc.xml");



                    }
                    else
                    {
                        xmlWriter = XmlWriter.Create(path + "\\" + "accLoc.xml");
                        File.Delete(path + "\\" + "accLoc.ini");
                        iniFile = new IniFile(path + "\\" + "accLoc.ini");
                    }

                    //ini file!!!!!!!!not a good choice to use ini files....xmls are used nowadays
                    String names = "";
                    List<String> inactiveSites = new List<string>();

                    foreach (Location l in lc)
                    {
                        names += l.Name + " ";
                        iniFile.WriteSetting("Paths", l.Name, l.Path);
                        iniFile.WriteSetting("Codes", l.Name, l.Code);
                        iniFile.WriteSetting("RetentionPeriod", l.Name, l.RetentionPeriod.ToString());
                        iniFile.WriteSetting("Diagnostic", l.Name, l.DiagnosticLevel.ToString());
                        //iniFile.WriteSetting("Retention", l.Name, l.RetentionPeriod.ToString());
                        iniFile.WriteSetting("Status", l.Name, l.Status);
                        iniFile.WriteSetting("Accession", l.Name, string.Join(",", from item in Location.ListOfCodes(l) select item));

                        if (string.Equals(l.Status, "Inactive", StringComparison.CurrentCultureIgnoreCase))
                        {
                            inactiveSites.Add(l.Name);
                        }
                    }
                    iniFile.WriteSetting("SiteStatus", "InactiveSites", string.Join(",", from item in inactiveSites select item));
                    iniFile.WriteSetting("TouchCodes", "Touch", string.Join(",", from item in AccessionCodes.GetTouchCodes() select item.Code));
                    names = String.Join(",", names.Trim().Split(" "));
                    iniFile.WriteSetting("Labs", "LabList", names);



                    //xml file
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("locationlist");
                    foreach (Location loc in lc)
                    {
                        xmlWriter.WriteStartElement("location");
                        xmlWriter.WriteAttributeString("LocationID", "loc1");

                        xmlWriter.WriteStartElement("name");
                        xmlWriter.WriteString(loc.Name);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("status");
                        xmlWriter.WriteString(loc.Status);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("code");
                        xmlWriter.WriteString(loc.Code);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("path");
                        xmlWriter.WriteString(loc.Path);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("server");
                        xmlWriter.WriteString(loc.Server);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("retentionperiod");
                        xmlWriter.WriteString(loc.RetentionPeriod.ToString());
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("diagnosticlevel");
                        xmlWriter.WriteString(loc.DiagnosticLevel.ToString());
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("configfileversion");
                        xmlWriter.WriteString(loc.ConfigFileVersion.ToString());
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("host");
                        xmlWriter.WriteString(loc.Host);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("port");
                        xmlWriter.WriteString(loc.Port);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("username");
                        xmlWriter.WriteString(loc.Username);
                        xmlWriter.WriteEndElement();


                        xmlWriter.WriteStartElement("password");
                        xmlWriter.WriteString(loc.Passwrd);
                        xmlWriter.WriteEndElement();


                        xmlWriter.WriteStartElement("isFTP");
                        xmlWriter.WriteString(loc.IsFTP);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("directory");
                        xmlWriter.WriteString(loc.DirectoryPath);
                        xmlWriter.WriteEndElement();




                        List<AccLoc> acl = db.AccLoc.Where(o => o.LocCode == loc.Code).ToList();

                        foreach (AccLoc code in acl)
                        {
                            xmlWriter.WriteStartElement("accession");
                            if (AccessionCodes.CheckIfTouch(code.AccCode))
                                xmlWriter.WriteAttributeString("isTouch", "Y");
                            else
                                xmlWriter.WriteAttributeString("isTouch", "N");


                            xmlWriter.WriteString(code.AccCode);
                            xmlWriter.WriteEndElement();

                        }
                        xmlWriter.WriteEndElement(); //for location

                    }
                    xmlWriter.WriteEndElement(); //for locationlist
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Close();
                    if (currloc.IsFTP == "Y")
                    {
                        Ftp(currloc.DirectoryPath, "accloc.xml", currloc.Host, currloc.Port, currloc.Username, currloc.Passwrd, tempPath + "accloc.xml");
                        Ftp(currloc.DirectoryPath, "accLoc.ini", currloc.Host, currloc.Port, currloc.Username, currloc.Passwrd, tempPath + "accLoc.ini");
                        if (archive != null)
                        {
                            File.Copy(tempPath + "accLoc.xml", arcPath + @"\history\" + "accLoc_" + (latestVersion + 1) + ".xml");
                            File.Copy(tempPath + "accLoc.ini", arcPath + @"\history\" + "accLoc_" + (latestVersion + 1) + ".ini");
                        }
                            
                        File.Delete(tempPath + "accLoc.ini");
                        File.Delete(tempPath + "accLoc.xml");
                    }
                    else
                    {
                        if (archive != null)
                        {
                            File.Copy(path + "\\" + "accLoc.xml", arcPath + @"\history\" + "accLoc_" + (latestVersion + 1) + ".xml");
                            File.Copy(path + "\\" + "accLoc.ini", arcPath + @"\history\" + "accLoc_" + (latestVersion + 1) + ".ini");
                        }
                            
                    }


                    currloc.ConfigFileVersion = latestVersion + 1;

                    db.SaveChanges();
                    return "Config files published successfully. Please check the files at the location path(s).";
                }
                catch (Exception e)
                {
                    return "Something went wrong.Please try later." + "\n" + "Error Message: " + e.Message;
                }
            }
            else   //publishAll
            {
                //List<Location> locations = db.Location.Where(o => o.LockVersion != -1).ToList();
                String errorLog = "";
                //foreach (Location ltn in lc)
                //{
                try
                {
                   

                    File.Delete(tempPath + "accLoc.ini");
                    File.Delete(tempPath + "accLoc.xml");
                    System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@22" + tempPath);
                    iniFile = new IniFile(tempPath + "accLoc.ini");
                    xmlWriter = XmlWriter.Create(tempPath + "accLoc.xml");



                   

                    //ini file!!!!!!!!not a good choice to use ini files....xmls are used nowadays
                    String names = "";
                    List<String> inactiveSites = new List<string>();

                    foreach (Location l in lc)
                    {
                        names += l.Name + " ";
                        iniFile.WriteSetting("Paths", l.Name, l.Path);
                        iniFile.WriteSetting("Codes", l.Name, l.Code);
                        iniFile.WriteSetting("RetentionPeriod", l.Name, l.RetentionPeriod.ToString());
                        iniFile.WriteSetting("Diagnostic", l.Name, l.DiagnosticLevel.ToString());
                        //iniFile.WriteSetting("Retention", l.Name, l.RetentionPeriod.ToString());
                        iniFile.WriteSetting("Status", l.Name, l.Status);
                        iniFile.WriteSetting("Accession", l.Name, string.Join(",", from item in Location.ListOfCodes(l) select item));

                        if (string.Equals(l.Status, "Inactive", StringComparison.CurrentCultureIgnoreCase))
                        {
                            inactiveSites.Add(l.Name);
                        }
                    }
                    iniFile.WriteSetting("SiteStatus", "InactiveSites", string.Join(",", from item in inactiveSites select item));
                    iniFile.WriteSetting("TouchCodes", "Touch", string.Join(",", from item in AccessionCodes.GetTouchCodes() select item.Code));
                    names = String.Join(",", names.Trim().Split(" "));
                    iniFile.WriteSetting("Labs", "LabList", names);

                    //xml files


                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("locationlist");
                    foreach (Location loc in lc)
                    {
                        xmlWriter.WriteStartElement("location");
                        xmlWriter.WriteAttributeString("LocationID", "loc1");

                        xmlWriter.WriteStartElement("name");
                        xmlWriter.WriteString(loc.Name);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("status");
                        xmlWriter.WriteString(loc.Status);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("code");
                        xmlWriter.WriteString(loc.Code);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("path");
                        xmlWriter.WriteString(loc.Path);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("server");
                        xmlWriter.WriteString(loc.Server);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("retentionperiod");
                        xmlWriter.WriteString(loc.RetentionPeriod.ToString());
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("diagnosticlevel");
                        xmlWriter.WriteString(loc.DiagnosticLevel.ToString());
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("configfileversion");
                        xmlWriter.WriteString(loc.ConfigFileVersion.ToString());
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("host");
                        xmlWriter.WriteString(loc.Host);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("port");
                        xmlWriter.WriteString(loc.Port);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("username");
                        xmlWriter.WriteString(loc.Username);
                        xmlWriter.WriteEndElement();


                        xmlWriter.WriteStartElement("password");
                        xmlWriter.WriteString(loc.Passwrd);
                        xmlWriter.WriteEndElement();


                        xmlWriter.WriteStartElement("isFTP");
                        xmlWriter.WriteString(loc.IsFTP);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("directory");
                        xmlWriter.WriteString(loc.DirectoryPath);
                        xmlWriter.WriteEndElement();





                        List<AccLoc> acl = db.AccLoc.Where(o => o.LocCode == loc.Code).ToList();

                        foreach (AccLoc code in acl)
                        {
                            xmlWriter.WriteStartElement("accession");
                            if (AccessionCodes.CheckIfTouch(code.AccCode))
                                xmlWriter.WriteAttributeString("isTouch", "Y");
                            else
                                xmlWriter.WriteAttributeString("isTouch", "N");
                            xmlWriter.WriteString(code.AccCode);
                            xmlWriter.WriteEndElement();

                        }
                        xmlWriter.WriteEndElement(); //for location

                    }
                    xmlWriter.WriteEndElement(); //for locationlist
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Close();
                    foreach (Location ltn in lc)
                    {
                        try
                        {
                            if (ltn.IsFTP == "Y")
                            {
                                Ftp(ltn.DirectoryPath, "accloc.xml", ltn.Host, ltn.Port, ltn.Username, ltn.Passwrd, tempPath + "accloc.xml");
                                Ftp(ltn.DirectoryPath, "accLoc.ini", ltn.Host, ltn.Port, ltn.Username, ltn.Passwrd, tempPath + "accLoc.ini");


                            }
                            else
                            {
                                File.Copy(tempPath + "accLoc.xml", ltn.Path + "\\" + "accLoc.xml", true);
                                File.Copy(tempPath + "accLoc.ini", ltn.Path + "\\" + "accLoc.ini", true);
                            }
                            ltn.ConfigFileVersion = latestVersion + 1;
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {

                            errorLog += "Location : " + ltn.Name + "\n" + "Error Message: " + e.Message + "\n" + "\n";


                        }
                    }
                    if (archive != null)
                    {
                        File.Copy(tempPath + "accLoc.xml", arcPath + @"\history\" + "accLoc_" + (latestVersion + 1) + ".xml", true);
                        File.Copy(tempPath + "accLoc.ini", arcPath + @"\history\" + "accLoc_" + (latestVersion + 1) + ".ini", true);
                    }
                       
                    File.Delete(tempPath + "accLoc.ini");
                    File.Delete(tempPath + "accLoc.xml");

                }
                catch (Exception e)
                {
                    errorLog += "Some error occurred" + "\n" + e.Message;

                }



                if (errorLog == "")
                    return "Config files published successfully at all sites. Please check the files at the location path(s).";
                else
                    return "Config files published successfully." + "\n" + "There were errors while publishing at some sites. Please refer to the errors below:" + "\n" + "\n" + errorLog;
            }

        }

        public String PingAllSites()
        {
            String errorLog = "";
            List<Location> locations = db.Location.ToList();

            PingReply reply;

            foreach (Location l in locations)
            {

                Ping myPing = new Ping();
                try
                {
                    if (l.IsFTP == "N" ||l.IsFTP==null)
                        reply = myPing.Send(l.Server.ToString(), 200);  //200ms timeout 
                    else
                        reply = myPing.Send(l.Host.ToString(), 200);
                }
                catch (PingException ex)
                {
                    errorLog += "Ping Failed at : " + l.Name + "\n" + "Reason : " + ex.InnerException.Message + "\n";
                }




            }
            return errorLog;
        }

        public String PingSelectedSite(string server)
        {
            String response = "";
            try
            {
                Ping myPing = new Ping();

                PingReply reply = myPing.Send(server, 1000);  //timeout time 1000ms
                if (reply != null)
                {
                    response = "Status :  " + reply.Status + "\n" + "Time : " + reply.RoundtripTime.ToString() + "(ms)" + "\n" + "Address : " + reply.Address;


                }
            }
            catch (PingException ex)
            {
                response = "Status : Failed  " + "\n" + "Message : " + ex.InnerException.Message;

            }
            return response;
        }


        public void RemoveLocation(string site)
        {


            var result = db.Location.Where(o => o.Code == site).First();
            db.Remove(result);
            //result.LockVersion = -1;
            //result.UpdatedAt = DateTime.Now;
            //var result2 = db.AccLoc.Where(o => o.LocCode == site);
            //foreach (var al in result2)
            //{
            //    al.LockVersion = -1;
            //    al.UpdatedAt = DateTime.Now;

            //}
            db.SaveChanges();

            //removing the unused codes
            List<String> accLocLeft = db.AccLoc.Select(o => o.AccCode).ToList();
            List<AccessionCodes> toBeDel = db.AccessionCodes.Where(o => !accLocLeft.Contains(o.Code)).ToList();
            foreach (AccessionCodes ac in toBeDel)
            {
                db.AccessionCodes.Remove(ac);
                db.SaveChanges();
            }


        }
        internal void Ftp(string directory, string fileToBeCreated, string host, string port, string username, string password, string fileToBeTransferred)
        {
            //try
            //{
            //string directory = "/test";   //it should be there
            //string filename = "/abc2.txt";  //the file which would be created there
            //string host = "10.105.198.84";
            //string port = "21";

            username = username == null ? "" : username.ToString();
            password = password == null ? "" : password.ToString();
            directory = directory == null ? "" : directory.ToString() + "/";

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + host + ":" + port + "/" + directory + fileToBeCreated);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(username, password);

            // Copy the contents of the file to the request stream.
            byte[] fileContents;
            using (StreamReader sourceStream = new StreamReader(fileToBeTransferred))
            {
                fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            }

            request.ContentLength = fileContents.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileContents, 0, fileContents.Length);
            }

            //using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            //{
            //    return $"Upload File Complete, status {response.StatusDescription}";
            //}
            //}
            //catch (Exception e)
            //{
            //    return "Error : " + e.Message;
            //}

        }
        private bool CheckIfFileExistsOnServer(string fileName, Location l)
        {
            string username = l.Username == null ? "" : l.Username.ToString();
            string password = l.Passwrd == null ? "" : l.Passwrd.ToString();
            string directory = l.DirectoryPath == null ? "" : l.DirectoryPath.ToString() + "/";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + l.Host + ":" + l.Port + "/" + directory + fileName);
            request.Credentials = new NetworkCredential(username, password);
            request.Method = WebRequestMethods.Ftp.GetFileSize;


            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            return true;


        }
    }
}