using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface IMeetingRepository : IRepository<Meeting>
    {
        IEnumerable GetMeetingsByUserId(Guid Id);
    }
}