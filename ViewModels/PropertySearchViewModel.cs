using SS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class PropertySearchViewModel
    {
        public String Country { get; set; }
        public String Division { get; set; }
        public String PropertyCategory { get; set; }
        public String PropertyType
        {
            get { return null; }
            set
            {
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                    PropertyCategory = unitOfWork.PropertyType.GetPopertyTypeCategoryCodeByName(value);
                    PropertyType = value;
                }
            }
        }
        public String PropertyPurpose { get; set; }
        public Decimal MinPrice { get; set; }
        public Decimal MaxPrice { get; set; }
        public String SearchTerm { get; set; }
        public String[] Tags { get; set; }
        public int pgNo { get; set; }
    }
}