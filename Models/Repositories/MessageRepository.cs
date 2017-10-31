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
        {
            if (take > 0)
            {
                return EasyFindPropertiesEntities.Message.Where(x => x.To.Equals(Id))
                    .OrderByDescending(x => x.DateTCreated)
                    .Take(take)
                    .ToList();
            }
            else
            {
                return EasyFindPropertiesEntities.Message.Where(x => x.To.Equals(Id))
                    .OrderByDescending(x => x.DateTCreated)
                    .ToList();
            }
        }
    }
}