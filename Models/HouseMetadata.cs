using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SS.Models
{
    public class HouseMetadata
    {
        [Required(ErrorMessage="*")]
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
        [Display(Name="Bedroom Amount")]
        [DataType(DataType.Currency,ErrorMessage="Only accept numbers")]
        public string BED_ROOM_AMOUNT { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "livingroom Amount")]
        [DataType(DataType.Currency, ErrorMessage = "Only accept numbers")]
        public string LIVING_ROOM_AMOUNT { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "Bathroom Amount")]
        [DataType(DataType.Currency, ErrorMessage = "Only accept numbers")]
        public string BATH_ROOM_AMOUNT { get; set; }
        [Required(ErrorMessage = "*")]
        public string PURPOSE { get; set; }
        [Required(ErrorMessage = "*")]
        public bool ISFURNISHED { get; set; }
        [Required(ErrorMessage = "*")]
        public string DESCRIPTION { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Parishes { get; set; }
    }
}