using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Code
{
    public class LandModel : IPropertiesModel
    {
        public string ID { get; set; }
        public string StreetAddress { get; set; }
        public string Parish { get; set; }
        public string Price { get; set; }
        public string ImageURL { get; set; }
        public string City { get; set; }
        public string Description { get; set; }
        public string Purpose { get; set; }
        public string Area { get; set; }
        public OwnerModel ownerModel = new OwnerModel();

        public LandModel() { }

        public void setModel(string id, string sa, string parish, string price, string imageURL, string occupancy, string bedroomAmount, string area)
        {
            ID = id;
            StreetAddress = sa;
            Parish = parish;
            Price = price;
            ImageURL = imageURL;
            Area = area;
        }

        public object getModel()
        {
            return this;
        }
    }
}