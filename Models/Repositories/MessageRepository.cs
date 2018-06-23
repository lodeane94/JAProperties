using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public MessageRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<Message> GetMsgsForUserID(Guid Id, int take = 0)
        {
            var deletedMsgIds = EasyFindPropertiesEntities.MessageTrash.Where(x => x.UserID.Equals(Id)).Select(x => x.MessageID).ToList();
            var msgUsers = GetMsgUserIds(Id).ToList();

            return EasyFindPropertiesEntities.Message
                .Where(x => (msgUsers.Contains(x.To) || msgUsers.Contains(x.From))
                && (x.To.Equals(Id) || x.From.Equals(Id))
                && !deletedMsgIds.Contains(x.ID))
                .GroupBy(x => new { x.ThreadId })
                .Select(group => group.OrderByDescending(x => x.DateTCreated))
                .Select(x => x.FirstOrDefault())
                .ToList();

        }

        public IEnumerable<Message> GetMsgThreadByMsgID(Guid Id, Guid userId)
        {
            //var from = EasyFindPropertiesEntities.Message.Where(x => x.ID.Equals(msgId)).Select(x => x.From).Single();
            //var to = EasyFindPropertiesEntities.Message.Where(x => x.ID.Equals(msgId)).Select(x => x.To).Single();
            var deletedMsgIds = EasyFindPropertiesEntities.MessageTrash.Where(x => x.UserID.Equals(userId)).Select(x => x.MessageID).ToList();
            var threadId = EasyFindPropertiesEntities.Message.Where(x => x.ID.Equals(Id)).Select(x => x.ThreadId).SingleOrDefault();
            /* return EasyFindPropertiesEntities.Message
                 .Where(x => ((x.From.Equals(from)
                     && x.To.Equals(userId))
                     || (x.From.Equals(userId)
                     && x.To.Equals(to)))
                     && !deletedMsgIds.Contains(x.ID))
                 .OrderBy(x => x.DateTCreated)
                 .ToList();*/
            if (threadId != null)
            {
                return EasyFindPropertiesEntities.Message
               .Where(x => x.ThreadId.Equals(threadId)
                   && !deletedMsgIds.Contains(x.ID))
               .OrderBy(x => x.DateTCreated)
               .ToList();
            }

            return null;

        }

        public IEnumerable<User> GetMsgUsers(Guid userId)
        {
            var deletedMsgIds = EasyFindPropertiesEntities.MessageTrash.Where(x => x.UserID.Equals(userId)).Select(x => x.MessageID).ToList();

            var fromUserIds = EasyFindPropertiesEntities.Message
                .Where(x => x.To.Equals(userId) && !deletedMsgIds.Contains(x.ID))
                .Select(x => x.From).Distinct().ToList();

            var toUserIds = EasyFindPropertiesEntities.Message
                .Where(x => x.From.Equals(userId) && !deletedMsgIds.Contains(x.ID))
                .Select(x => x.To).Distinct().ToList();

            var userIds = fromUserIds.Concat(toUserIds);

            return EasyFindPropertiesEntities.User
                .Where(x => userIds.Contains(x.ID))
                .ToList();
        }

        private IEnumerable<Guid> GetMsgUserIds(Guid userId)
        {
            var deletedMsgIds = EasyFindPropertiesEntities.MessageTrash.Where(x => x.UserID.Equals(userId)).Select(x => x.MessageID).ToList();

            var fromUserIds = EasyFindPropertiesEntities.Message
                .Where(x => x.To.Equals(userId) && !deletedMsgIds.Contains(x.ID))
                .Select(x => x.From).Distinct().ToList();

            var toUserIds = EasyFindPropertiesEntities.Message
                .Where(x => x.From.Equals(userId) && !deletedMsgIds.Contains(x.ID))
                .Select(x => x.To).Distinct().ToList();

            return fromUserIds.Concat(toUserIds).Distinct();
        }

        public Guid GetThreadIdForUser(Guid from, Guid to)
        {
            var deletedMsgIds = EasyFindPropertiesEntities.MessageTrash.Where(x => x.UserID.Equals(from)).Select(x => x.MessageID).ToList();

            return EasyFindPropertiesEntities.Message
                 .Where(x => ((x.From.Equals(from)
                     && x.To.Equals(to))
                     || (x.From.Equals(to)
                     && x.To.Equals(from)))
                     && !deletedMsgIds.Contains(x.ID))
                     .Select(x => x.ThreadId)
                     .FirstOrDefault();

        }
    }
}