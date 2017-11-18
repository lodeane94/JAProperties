using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class InviteeViewModel
    {
        public Guid UserID { get; set; }
        public String FullName { get; set; }
        public String ImageUrl { get; set; }
        public String inviteeType { get; set; }
    }
}