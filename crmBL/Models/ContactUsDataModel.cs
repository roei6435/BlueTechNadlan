using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crmBL.Models
{
    public enum LeadType            //סוג פונה- קונה או מוכר נכס
    {
        buyer= 913200000, seller = 913200001
    }
    public enum DealType            //סוג הדיל המתבקש בנכס - השכרה או רכישה
    {
        buying = 913200000, renting = 913200001
    }
    public class ContactUsDataModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string TitleOfAsset { get; set; }
        public decimal AssetPrice  { get; set; }
        public string FullAddress { get; set; }
        public string City { get; set; }
        public decimal NumburRooms { get; set; }
        public LeadType LeadType { get; set; }
        public DealType DealType { get; set; }


    }
}
