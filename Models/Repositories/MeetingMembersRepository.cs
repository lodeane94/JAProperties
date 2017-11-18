using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class MeetingMembersRepository : Repository<MeetingMembers>, IMeetingMembersRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public MeetingMembersRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }
    }
}