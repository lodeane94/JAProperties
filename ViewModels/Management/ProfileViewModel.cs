using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SS.Models;

namespace SS.ViewModels.Management
{
    public class ProfileViewModel
    {
        public Guid ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CellNum { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }
        public string LogoUrl { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public HttpPostedFileBase organizationLogo { get; set; }
    }
}