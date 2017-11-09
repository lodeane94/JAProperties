using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class MessageViewModel
    {
        public Guid ID { get; set; }
        public String From { get; set; }
        public String To { get; set; }
        public String CellNum { get; set; }
        public String Email { get; set; }
        public String Msg { get; set; }
        public bool Seen { get; set; }
        public String DateTCreated { get; set; }
    }
}