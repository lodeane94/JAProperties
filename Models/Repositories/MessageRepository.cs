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

        public IEnumerable<Message> GetAllMsgsForID(Guid Id)
        {
            return EasyFindPropertiesEntities.Message.Where(x => x.To.Equals(Id)).ToList();
        }
    }
}