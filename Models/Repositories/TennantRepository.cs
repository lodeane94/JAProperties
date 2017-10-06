using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class TennantRepository : Repository<Tennant>, ITennantRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public TennantRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<Tennant> GetTennantsByPropertyId(string Id)
        {
            return EasyFindPropertiesEntities.Tennant.Where(x => x.PropertyID.Equals(Id)).ToList();
        }
    }
}