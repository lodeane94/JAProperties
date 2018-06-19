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
        {//todo order by latest message
            var deletedMsgIds = EasyFindPropertiesEntities.MessageTrash.Where(x => x.UserID.Equals(Id)).Select(x => x.MessageID).ToList();

            if (take > 0)
            {
                return EasyFindPropertiesEntities.Message
                    .Where(x => x.To.Equals(Id) && !deletedMsgIds.Contains(x.ID))
                    .OrderByDescending(x => x.DateTCreated)
                    .GroupBy(x => x.From)
                    .Select(x => x.FirstOrDefault())
                    .OrderByDescending(x => x.DateTCreated)
                    .Take(take)
                    .ToList();
            }
            else
            {
                return EasyFindPropertiesEntities.Message
                    .Where(x => x.To.Equals(Id) && !deletedMsgIds.Contains(x.ID))
                    .GroupBy(x => x.From)
                    .Select(group => group.OrderByDescending(x => x.DateTCreated))
                    .Select(x => x.FirstOrDefault())
                    .ToList();
            }
        }

        public IEnumerable<Message> GetMsgThreadByMsgID(Guid msgId, Guid userId)
        {
            var from = EasyFindPropertiesEntities.Message.Where(x => x.ID.Equals(msgId)).Select(x => x.From).Single();
            var deletedMsgIds = EasyFindPropertiesEntities.MessageTrash.Where(x => x.UserID.Equals(userId)).Select(x => x.MessageID).ToList();

            return EasyFindPropertiesEntities.Message
                .Where(x => ((x.From.Equals(from)
                    && x.To.Equals(userId))
                    || (x.From.Equals(userId)
                    && x.To.Equals(from)))
                    && !deletedMsgIds.Contains(x.ID))
                .OrderBy(x => x.DateTCreated)
                .ToList();
        }

        public IEnumerable<User> GetMsgUsers(Guid userId)
        {
            var fromUserIds = EasyFindPropertiesEntities.Message
                .Where(x => x.To.Equals(userId))
                .Select(x => x.From).Distinct().ToList();

            var toUserIds = EasyFindPropertiesEntities.Message
                .Where(x => x.From.Equals(userId))
                .Select(x => x.To).Distinct().ToList();

            var userIds = fromUserIds.Concat(toUserIds);

            return EasyFindPropertiesEntities.User
                .Where(x => userIds.Contains(x.ID))
                .ToList();
        }
    }
}