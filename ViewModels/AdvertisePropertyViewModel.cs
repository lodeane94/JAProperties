using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class AdvertisePropertyViewModel
    {
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
        public String Password { get; set; }
        public String ConfirmPassword { get; set; }
        public String StreetAddress { get; set; }
        public String Country { get; set; }
        public String Division { get; set; }
        public String Community { get; set; }
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
        public String coordinateLat { get; set; }
        public String coordinateLng { get; set; }
        public HttpPostedFileBase[] flPropertyPics { get; set; }
    }
}