using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SS.Models
{
    public class AccommodationMetadata
    {
        [Required(ErrorMessage = "*")]
        public string STREET_ADDRESS { get; set; }
        [Required(ErrorMessage = "*")]
        public string CITY { get; set; }
        [Required(ErrorMessage = "*")]
        public string PARISH { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name="cost")]
        [DataType(DataType.Currency,ErrorMessage="Must be a decimal value")]
        public decimal PRICE { get; set; }
        [Required(ErrorMessage="*")]
        [Display(Name = "security deposit")]
        [DataType(DataType.Currency, ErrorMessage = "Must be a decimal value")]
        public decimal SECURITY_DEPOSIT { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "occupancy")]
        [DataType(DataType.Currency, ErrorMessage = "Must be a integer value")]
        public short OCCUPANCY { get; set; }
        [Required(ErrorMessage = "*")]
        public string GENDER_PREFERENCE { get; set; }
        [Required(ErrorMessage = "*")]
        public string DESCRIPTION { get; set; }
        public string ENROLMENT_KEY { get; set; }
        [Required(ErrorMessage = "*")]
        public bool IS_STUDENT_ACC { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "bathroom amount")]
        [DataType(DataType.Currency, ErrorMessage = "Must be a decimal value")]
        public short HOUSE_BATHROOM_AMOUNT { get; set; }
      //  public IEnumerable<System.Web.Mvc.SelectListItem> Parishes { get; set; }
    }
}