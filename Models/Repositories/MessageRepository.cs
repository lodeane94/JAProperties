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

        public IEnumerable<Message> GetMsgsForID(Guid Id, int take = 0)
        {//todo order by latest message
            if (take > 0)
            {
                return EasyFindPropertiesEntities.Message.Where(x => x.To.Equals(Id))
                    .OrderByDescending(x => x.DateTCreated)
                    .GroupBy(x => x.From)
                    .Select(x => x.FirstOrDefault())
                    .OrderByDescending(x => x.DateTCreated)
                    .Take(take)
                    .ToList();
            }
            else
            {
                return EasyFindPropertiesEntities.Message.Where(x => x.To.Equals(Id))
                    .OrderByDescending(x => x.DateTCreated)
                    .GroupBy(x => x.From)
                    .Select(x => x.FirstOrDefault())
                //    .OrderByDescending(x => x.DateTCreated)
                    .ToList();
            }
        }

        public IEnumerable<Message> GetMsgThreadByMsgID(Guid msgId, Guid userId)
        {
            var from = EasyFindPropertiesEntities.Message.Where(x => x.ID.Equals(msgId)).Select(x => x.From).Single();

            return EasyFindPropertiesEntities.Message
                .Where(x => (x.From.Equals(from)
                    && x.To.Equals(userId))
                    ||(x.From.Equals(userId)
                    && x.To.Equals(from)))
                .OrderBy(x => x.DateTCreated)
                .ToList();
        }
    }
}