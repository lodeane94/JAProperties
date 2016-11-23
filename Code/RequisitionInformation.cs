using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Code
{
    public class RequisitionInformation
    {
        public string ID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string Cell { get; set; }
        public string Date { get; set; }
        public string Image_URL { get; set; }
        public bool accepted { get; set; }
    }
}