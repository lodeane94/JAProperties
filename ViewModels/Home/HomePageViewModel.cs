using SS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class HomePageViewModel
    {
        public HomePageViewModel()
        {
            FeaturedRental = new List<FeaturedProperty>();
            FeaturedSale = new List<FeaturedProperty>();
            FeaturedLease = new List<FeaturedProperty>();
        }

        public List<FeaturedProperty> FeaturedRental { get; set; }
        public List<FeaturedProperty> FeaturedSale { get; set; }
        public List<FeaturedProperty> FeaturedLease { get; set; }
    }
}