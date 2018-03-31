using SS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class FeaturedPropertiesSlideViewModel
    {
       /* public Property property { get; set; }
        public Owner owner { get; set; }
        public String propertyPrimaryImageURL { get; set; }
        public int averageRating { get; set; }
        public String Distance { get; set; }
        public String Duration { get; set; }*/

        public String ID { get; set; }
        public User User { get; set; }
        public String PropertyPrimaryImageURL { get; set; }
        public String PropertyType { get; set; }
        public String PropertyPurpose { get; set; }
        public String StreetNumber { get; set; }
        public String StreetAddress { get; set; }
        public String Community { get; set; }
        public decimal SecurityDeposit { get; set; }
        public decimal Price { get; set; }
        public String Division { get; set; }
        public String Country { get; set; }
        public String Description { get; set; }
        public IEnumerable<PropertyRating> PropRatings { get; set; }
        public int PropertyAverageRatings { get; set; }
        public int TotalBedrooms { get; set; }
        public int TotalBathrooms { get; set; }
        public String Occupancy { get; set; }
        public bool isFurnished { get; set; }
        public decimal Area { get; set; }
        public String DistanceFromSearchedAddress { get; set; }
        public String DuratiionFromSearchedAddress { get; set; }
        public bool IsPropertySaved { get; set; }
        public List<string> tags { get; set; }
    }
}