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
        private String propertyType;
        private bool chkBuyProperty;
        private bool chkRentProperty;
        private bool chkLeasedProperty;
        /*
        private String country;
        private String division;
        private String propertyPurpose;
        private Decimal minPrice;
        private Decimal maxPrice;
        private String searchTerm;
        private String[] tags;
        private int pgNo;*/

        public String Country { get; set; }
        public String Division { get; set; }
        public String PropertyCategory
        {
            get { return propertyCategory; }
            set { propertyCategory = Core.PropertyHelper.mapPropertyCategoryNameToCode(value); }
        }
        public String PropertyType
        {
            get { return propertyType; }
            set{ propertyType = value; }
        }
        public String PropertyPurpose { get; set; }
        public Decimal MinPrice { get; set; }
        public Decimal MaxPrice { get; set; }
        public String SearchTerm { get; set; }
        public String[] Tags { get; set; }
        public bool ChkBuyProperty
        {
            get { return chkBuyProperty; }
            set { chkBuyProperty = value; }
        }
        public bool ChkRentProperty
        {
            get { return chkRentProperty; }
            set { chkRentProperty = value; }
        }
        public bool ChkLeasedProperty
        {
            get { return chkLeasedProperty; }
            set { chkLeasedProperty = value; }
        }
        public int PgNo { get; set; }
    }
}