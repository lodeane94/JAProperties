using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Code
{
    public class HouseModel : IPropertiesModel
    {
        public string ID { get; set; }
        public string StreetAddress { get; set; }
        public string Parish { get; set; }
        public string Price { get; set; }
        public string ImageURL { get; set; }
        public string City { get; set; }
        public string Description { get; set; }
        public string Purpose { get; set; }
        public string BedroomAmount { get; set; }
        public string BathroomAmount { get; set; }
        public string isFurnished { get; set; }
        public OwnerModel ownerModel = new OwnerModel();

        public HouseModel() { }

        public HouseModel(string id, string sa, string parish, string price, string imageURL, string city, string desc, 
            string purpose,string bedroomAmount, string bathroomAmt, string furnished)
        {
            ID = id;
            StreetAddress = sa;
            Parish = parish;
            Price = price;
            ImageURL = imageURL;
            City = city;
            Description = desc;
            Purpose = purpose;
            BedroomAmount = bedroomAmount;
            BathroomAmount = bathroomAmt;
            isFurnished = furnished;
        }

        public void setModel(string id, string sa, string parish, string price, string imageURL, string occupancy, string bedroomAmount, string area)
        {
            ID = id;
            StreetAddress = sa;
            Parish = parish;
            Price = price;
            ImageURL = imageURL;
            BedroomAmount = bedroomAmount;
        }

        public object getModel()
        {
            return this;
        }
    }
}