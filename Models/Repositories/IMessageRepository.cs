using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface IMessageRepository : IRepository<Message>
    {
        IEnumerable<Message> GetMsgsForID(Guid Id, int take = 0);
        IEnumerable<Message> GetMsgThreadByMsgID(Guid Id, Guid userId);
    }
}