using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class MessageTrashRepository : Repository<MessageTrash>, IMessageTrashRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public MessageTrashRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }
    }
}