using System;
using System.Collections.Generic;

namespace MVCNetAdmin.Models
{
    public partial class AccLoc
    {
        static NetAdminContext db;

        public AccLoc(NetAdminContext context)
        {
           
            db = context;
        }

        public AccLoc()
        {
        }
        public string LocCode { get; set; }
        public string AccCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        // public int LockVersion { get; set; }

        public AccessionCodes AccCodeNavigation { get; set; }
        public Location LocCodeNavigation { get; set; }
    }
}