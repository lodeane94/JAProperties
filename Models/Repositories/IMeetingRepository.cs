using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface IMeetingRepository : IRepository<Meeting>
    {
        IEnumerable<Meeting> GetMeetingsByUserId(Guid Id);
    }
}