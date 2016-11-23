using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.App_Code
{
    public class BillsModel
    {
        public string ID { get; set; }
        public string BType { get; set; }
        public string BAmount { get; set; }
        public string Description { get; set; }
        public string DateIssued { get; set; }
        public string DateDue { get; set; }
        public string BillURL { get; set; }
        public string AccommodationID { get; set; }
    }
}