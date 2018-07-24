using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels.WebInfo
{
    public class ContactFormViewModel
    {
        public String Subject { get; set; }
        public String FName { get; set; }
        public String LName { get; set; }
        public String Email { get; set; }
        public String Message { get; set; }
    }
}