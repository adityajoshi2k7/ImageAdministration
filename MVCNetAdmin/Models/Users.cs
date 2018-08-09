using System;
using System.Collections.Generic;

namespace MVCNetAdmin.Models
{
    public partial class Users
    {
        static NetAdminContext db;

        

        public Users(NetAdminContext context)
        {

            db = context;
        }

        public Users()
        {
        }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
