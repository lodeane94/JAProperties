using SS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class PropertySearchViewModel
    {
        private String propertyCategory;
        public String PropertyCategory
        {
            get { return propertyCategory; }
            set { propertyCategory = Core.PropertyHelper.mapPropertyCategoryNameToCode(value); }
        }
        public String PropertyType { get; set; }
        public String Country { get; set; }
        public String Division { get; set; }
        public String RdoAdType { get; set; }
        public String PropertyPurpose { get; set; }
        public Decimal MinPrice { get; set; }
        public Decimal MaxPrice { get; set; }
        public String SearchTerm { get; set; }
        public String[] Tags { get; set; }
        public int PgNo { get; set; }
        public int take { get; set; }
        public String coordinateLat { get; set; }
        public String coordinateLng { get; set; }
        public String SearchType { get; set; }
    }
}