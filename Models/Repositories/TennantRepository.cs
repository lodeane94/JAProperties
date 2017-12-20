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

        public IEnumerable<Tennant> GetTennantsByPropertyId(Guid Id)
        {
            return EasyFindPropertiesEntities.Tennant.Where(x => x.PropertyID.Equals(Id)).ToList();
        }

        public IEnumerable<Tennant> GetTennantsByOwnerId(Guid Id)
        {
            var ownerPropertyIds = EasyFindPropertiesEntities.Property
                .Where(x => x.OwnerID.Equals(Id))
                .Select(x => x.ID).ToList();

            return EasyFindPropertiesEntities.Tennant.Where(x => ownerPropertyIds.Contains(x.PropertyID)).ToList();
        }

        public Tennant GetTennantByUserId(Guid Id)
        {
            return EasyFindPropertiesEntities.Tennant
                .Where(x => x.UserID.Equals(Id))
                .Single();
        }
    }
}