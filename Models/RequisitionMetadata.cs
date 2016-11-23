using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SS.Models
{
    public class RequisitionMetadata
    {
        [Required(ErrorMessage="*")]
        public string FIRST_NAME { get; set; }
        [Required(ErrorMessage = "*")]
        public string LAST_NAME { get; set; }
        public string EMAIL { get; set; }
        [Required(ErrorMessage = "*")]
        public string CELL { get; set; }
        [Required(ErrorMessage = "*")]
        public string GENDER { get; set; }
    }
}