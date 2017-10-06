using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Code
{
    public class Complaints
    {
        public string ID { get; set; }
        public string ComplainerID { get; set; }
        public string Recipient { get; set; }
        public string Complaint { get; set; }
        public string Date { get; set; }
    }
}