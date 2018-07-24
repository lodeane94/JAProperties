using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels.Management
{
    public class UpdatePropertyViewModel
    {
        public Guid ID { get; set; }
        public String Title { get; set; }
        public String PropertyCategory { get; set; }
        public String AdType { get; set; }
        public decimal Price { get; set; }
        public decimal SecurityDeposit { get; set; }
        public int Occupancy { get; set; }
        public String GenderPreferenceCode { get; set; }
        public int TotAvailableBathroom { get; set; }
        public int TotRooms { get; set; }
        public decimal Area { get; set; }
        public bool IsReviewable { get; set; }
        public String TermsAgreement { get; set; }
        public String Description { get; set; }
        public String PrimaryImageUrl { get; set; }
        public IEnumerable<Models.PropertyImage> PropertyImages { get; set; }
        public HttpPostedFileBase[] PropertyImageFiles { get; set; }
    }
}