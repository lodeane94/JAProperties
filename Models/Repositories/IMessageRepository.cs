using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface IMessageRepository : IRepository<Message>
    {
        IEnumerable<Message> GetMsgsForUserID(Guid Id, int take = 0);
        IEnumerable<Message> GetMsgThreadByMsgID(Guid Id, Guid userId);
        IEnumerable<User> GetMsgUsers(Guid userId);
        Guid GetThreadIdForUser(Guid from, Guid to);
    }
}