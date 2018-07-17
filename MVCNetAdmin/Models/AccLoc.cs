using System;
using System.Collections.Generic;

namespace MVCNetAdmin.Models
{
    public partial class AccLoc
    {
        public string LocCode { get; set; }
        public string AccCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        // public int LockVersion { get; set; }

        public AccessionCodes AccCodeNavigation { get; set; }
        public Location LocCodeNavigation { get; set; }
    }
}