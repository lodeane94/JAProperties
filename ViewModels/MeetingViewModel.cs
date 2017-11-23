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
        public String MeetingHour { get; set; }
        public String MeetingMinute { get; set; }
        public String MeetingPeriod { get; set; }
        public string Purpose { get; set; }
        public List<Guid> MeetingMemberUserIDs { get; set; }
    }
}