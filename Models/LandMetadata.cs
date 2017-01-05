using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SS.Models
{
    public class LandMetadata
    {
        [Required(ErrorMessage = "*")]
        public string STREET_ADDRESS { get; set; }
        [Required(ErrorMessage = "*")]
        public string CITY { get; set; }
        [Required(ErrorMessage = "*")]
        public string PARISH { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "Price")]
        [DataType(DataType.Currency, ErrorMessage = "Only accept numeral values")]
        public decimal PRICE { get; set; }
        [Required(ErrorMessage = "*")]
        public string PURPOSE { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name="Area")]
        [DataType(DataType.Currency)]
        public string AREA { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Parishes { get; set; }
    }
}