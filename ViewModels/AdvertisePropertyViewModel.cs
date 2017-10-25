using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class AdvertisePropertyViewModel
    {
        public String Title { get; set; }//n
        public String PropertyCategory { get; set; }
        public String PropertyPurpose { get; set; }
        public String PropertyType { get; set; }
        public String AdvertismentType { get; set; }
        public String AdvertismentPriority { get; set; }
        public String SubscriptionType { get; set; }
        public int SubscriptionPeriod { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String CellNum { get; set; }
        public String Email { get; set; }
        public String Organization { get; set; }//n
        public String Password { get; set; }
        public String ConfirmPassword { get; set; }
        public String StreetAddress { get; set; }
        public String Country { get; set; }
        public String Division { get; set; }
        public String Community { get; set; }
        public String NearBy { get; set; } //n
        public decimal Price { get; set; }
        public decimal SecurityDeposit { get; set; }
        public int Occupancy { get; set; }
        public String GenderPreferenceCode { get; set; }
        public int TotAvailableBathroom { get; set; }
        public int TotRooms { get; set; }
        public decimal Area { get; set; }
        public bool IsReviewable { get; set; }
        public String Description { get; set; }
        public String EnrolmentKey { get; set; }
        public String TermsAgreement { get; set; }
        public String[] selectedTags { get; set; }
        //street address location
        public String saCoordinateLat { get; set; }//n
        public String saCoordinateLng { get; set; }//n
        //community location
        public String cCoordinateLat { get; set; }//n
        public String cCoordinateLng { get; set; }//n
        //establishment near by location
        public String nearByCoordinateLat { get; set; }//n
        public String nearByCoordinateLng { get; set; }//n
        public HttpPostedFileBase organizationLogo { get; set; }//n
        public HttpPostedFileBase[] flPropertyPics { get; set; }
    }
}