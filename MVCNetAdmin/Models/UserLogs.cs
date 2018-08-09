using System;
using System.Collections.Generic;

namespace MVCNetAdmin.Models
{
    public partial class UserLogs
    {
        static NetAdminContext db;
        public UserLogs(NetAdminContext context)
        {

            db = context;
        }

        public UserLogs()
        {
        }
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Ipaddress { get; set; }
        public string Action { get; set; }
        public DateTime? CreatedAt { get; set; }




        public  void LogDetails(string userId,string IPAddress,string action)
        {
            UserLogs log= new UserLogs();
            log.UserId = userId;
            log.Ipaddress = IPAddress;
            log.Action = action;
            log.CreatedAt = DateTime.Now;
            db.UserLogs.Add(log);
            db.SaveChanges();

           

        }



    }
}
