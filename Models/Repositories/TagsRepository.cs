using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class TagsRepository : Repository<Tags>, ITagsRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public TagsRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }
    }
}