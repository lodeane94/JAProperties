using SS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class FeaturedProperty : Property
    {
        public string PrimaryImageURL { get; set; }
        public int AverageRating { get; set; }
        public bool ShowRating { get; set; }
        public string FurnishedValue { get; set; }
        public string OwnerContactNum { get; set; }
    }
}