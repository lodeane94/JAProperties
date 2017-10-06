using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SS.Models
{
    public class OwnerMetadata
    {
        [Required(ErrorMessage = "*")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "*")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "*")]
        public string CellNum { get; set; }

        [DataType(DataType.EmailAddress,ErrorMessage="Enter valid email address")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string PASSWORD { get; set; }

        [DataType(DataType.Password)]
        public string PASSWORD_CONFIRMED { get; set; }
    }
}