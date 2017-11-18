using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class MeetingViewModel
    {
        public Guid ID { get; set; }
        public Guid InviterUserID { get; set; }
        public string Location { get; set; }
        public String MeetingTitle { get; set; }
        public DateTime MeetingDate { get; set; }
        public TimeSpan MeetingTime { get; set; }
        public string Purpose { get; set; }
        public List<Guid> MeetingMemberUserIDs { get; set; }
    }
}