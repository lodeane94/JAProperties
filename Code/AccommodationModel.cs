using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Code
{
    public class AccommodationModel : IPropertiesModel
    {
        public string ID { get; set; }
        public string StreetAddress { get; set; }
        public string Parish { get; set; }
        public string Price { get; set; }
        public string ImageURL { get; set; }
        public string City { get; set; }
        public string Description { get; set; }
        public string SecurityDeposit { get; set; }
        public string Occupancy { get; set; }
        public string BathroomAmount { get; set; }
        public string Water { get; set; }
        public string Electricity { get; set; }
        public string Cable { get; set; }
        public string Gas { get; set; }
        public string Internet { get; set; }
        public string Availability { get; set; }
        public string TermsAgreement { get; set; }
        public string GenderPreference { get; set; }
        public OwnerModel ownerModel = new OwnerModel();

        public AccommodationModel() { }

        public AccommodationModel(string id, string sa, string parish, string price, string imageURL, string city, string desc,
            string securityDeposit, string occupancy, string bathRoomAmt, string water, string electricity, string cable,
            string gas, string internet, string availability, string termsAgreement, string genderPref)
        {
            ID = id;
            StreetAddress = sa;
            Parish = parish;
            Price = price;
            ImageURL = imageURL;
            City = city;
            Description = desc;
            SecurityDeposit = securityDeposit;
            Occupancy = occupancy;
            BathroomAmount = bathRoomAmt;
            Water = water;
            Electricity = electricity;
            Cable = cable;
            Gas = gas;
            Internet = internet;
            Availability = availability;
            TermsAgreement = termsAgreement;
            GenderPreference = genderPref;
        }

        public void setModel(string id, string sa, string parish, string price, string imageURL, string occupancy,string bedroomAmount, string area)
        {
            this.ID = id;
            this.StreetAddress = sa;
            this.Parish = parish;
            this.Price = price;
            this.ImageURL = imageURL;
            this.Occupancy = occupancy;
        }

        public Object getModel()
        {
            return this;
        }

    }
}