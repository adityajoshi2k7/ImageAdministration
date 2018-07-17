using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

namespace MVCNetAdmin.Models
{
    public partial class LocationNew
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public string Server { get; set; }
        public string Path { get; set; }
        public int? RetentionPeriod { get; set; }
        public int? DiagnosticLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ConfigFileVersion { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string DirectoryPath { get; set; }
        public string Passwrd { get; set; }
        public string IsFtp { get; set; }

        internal static string Recover(string xmlpath)
        {
            //try
            //{
                NetAdminContext db = new NetAdminContext();
            db.Database.ExecuteSqlCommand("delete from AccLoc");
            db.Database.ExecuteSqlCommand("delete from Location");
           
          
        
            XElement xelement = XElement.Load("C:\\Site2\\accLoc.xml");
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
                if (location.Element("retentionperiod").Value != null && location.Element("retentionperiod").Value.Trim() != "")
                    l.RetentionPeriod = Convert.ToInt32(location.Element("retentionperiod").Value);
                if (location.Element("diagnosticlevel").Value != null && location.Element("diagnosticlevel").Value.Trim() != "")
                    l.DiagnosticLevel = Convert.ToInt32(location.Element("diagnosticlevel").Value);
                if (location.Element("username").Value != null && location.Element("username").Value.Trim() != "")
                    l.Username = location.Element("username").Value;
                if (location.Element("password").Value != null && location.Element("password").Value.Trim() != "")
                    l.Passwrd = location.Element("password").Value;
                if (location.Element("host").Value != null && location.Element("host").Value.Trim() != "")
                    l.Host = location.Element("host").Value;
                if (location.Element("port").Value != null && location.Element("port").Value.Trim() != "")
                    l.Port = location.Element("port").Value;
                if (location.Element("directory").Value != null && location.Element("directory").Value.Trim() != "")
                    l.DirectoryPath = location.Element("directory").Value;
                l.IsFTP = location.Element("isFTP").Value;
                l.ConfigFileVersion = Convert.ToInt32(location.Element("configfileversion").Value);
                

                l.CreatedAt = DateTime.Now;
                l.UpdatedAt = DateTime.Now;
                db.Location.Add(l);
                db.SaveChanges();
                if (location.Elements("accession") != null)
                {

                    foreach (var ac in location.Elements("accession"))
                    {
                        AccLoc acl = new AccLoc();
                        acl.LocCode = location.Element("code").Value;
                        acl.AccCode = ac.Value;
                        acl.CreatedAt = DateTime.Now;
                        acl.UpdatedAt = DateTime.Now;
                        db.AccLoc.Add(acl);
                        System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@");
                        db.SaveChanges();
                    }

                }

                // System.Diagnostics.Debug.WriteLine(location.Element("name").Value);


            }




            return "Database Restored Successfully";
            //}
            //catch (Exception e)
            //{
            //    return e.Message;
            //}

        }
    }
}
