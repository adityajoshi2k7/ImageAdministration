using System;
using System.Collections.Generic;
using System.Linq;

namespace MVCNetAdmin.Models
{
    public partial class AccessionCodes
    {
        NetAdminContext db = new NetAdminContext();
        static NetAdminContext db2 = new NetAdminContext();
        public AccessionCodes()
        {
            AccLoc = new HashSet<AccLoc>();

        }

        public string Code { get; set; }
        public string IsTouch { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        // public int LockVersion { get; set; }

        public ICollection<AccLoc> AccLoc { get; set; }


        public string ViewTouchCodes()   //View Touch Codes
        {

            String listOfCodes = "";
            var result = db.AccessionCodes.Where(o => o.IsTouch == "Y").Select(o => o.Code);
            //if result.count()>0
            foreach (String codes in result)
            {
                listOfCodes += codes + " ";
            }

            return listOfCodes;
        }

        public static List<AccessionCodes> GetTouchCodes()
        {

            List<AccessionCodes> touchCodeList = db2.AccessionCodes.Where(o => o.IsTouch.Trim() == "Y").ToList();
            return touchCodeList;

        }

        public string ViewALLCodes()   //View all Codes
        {

            String listOfCodes = "";
            var result = db.AccessionCodes.Select(o => o.Code);
            //if result.count()>0
            foreach (String codes in result)
            {
                listOfCodes += codes + " ";
            }

            return listOfCodes;
        }

        public string SearchCode(string code)   //search a given code
        {

            String result = "";
            var Details = from r in db.AccLoc
                          join p in db.Location on r.LocCode equals p.Code
                          where r.AccCode.Trim() == code
                          select new { name = p.Name };
            //if result.count()>0
            if (Details.Count() > 0)
            {
                foreach (var locations in Details)
                {
                    result += locations.name + "\n";
                }
            }
            else
                result = "No Locations Found for this code";


            return result;
        }

        internal static bool CheckIfTouch(string accCode)
        {
            AccessionCodes ac= db2.AccessionCodes.Where(o => o.Code == accCode).FirstOrDefault();
            System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@"+ accCode);
            System.Diagnostics.Debug.WriteLine(ac.IsTouch);
            if (ac != null)
            {
                if (ac.IsTouch.Trim() == "Y")
                    return true;
                return false;
            }
            return false;
        }
    }
}