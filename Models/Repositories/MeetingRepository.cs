using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class MeetingRepository : Repository<Meeting>, IMeetingRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public MeetingRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<Meeting> GetMeetingsByUserId(Guid Id)
        {
            return EasyFindPropertiesEntities.Meeting.Where(x => x.InviterUserID.Equals(Id)).ToList();
        }
    }
}