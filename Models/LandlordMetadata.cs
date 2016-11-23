using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SS.Models
{
    public class LandlordMetadata
    {
        [Required(ErrorMessage = "*")]
        public string FIRST_NAME { get; set; }
        [Required(ErrorMessage = "*")]
        public string MIDDLE_NAME { get; set; }
        [Required(ErrorMessage = "*")]
        public string LAST_NAME { get; set; }
        [Required(ErrorMessage = "*")]
        public string GENDER { get; set; }
        [Required(ErrorMessage = "*")]
        public string CELL { get; set; }
        [DataType(DataType.EmailAddress,ErrorMessage="Enter valid email address")]
        public string EMAIL { get; set; }
        [Display(Name = "password")]
        [Required(ErrorMessage = "*")]
        [DataType(DataType.Password)]
        public string PASSWORD { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "confirm password")]
        [DataType(DataType.Password)]
        public string PASSWORD_CONFIRMED { get; set; }

    }
}