using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public SubscriptionRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<Subscription> GetSubscriptionsByTypeCode(string code)
        {
            return EasyFindPropertiesEntities.Subscription
                .Where(x => x.TypeCode.Equals(code)).ToList();
        }

        public Subscription GetActiveSubscriptionByOwnerID(Guid ID)
        {
            return EasyFindPropertiesEntities.Subscription
                .Where(x => x.OwnerID.Equals(ID) 
                && x.IsActive == true
                && x.Period > 0).SingleOrDefault();
        }

        public Subscription GetSubscriptionByOwnerID(Guid ID)
        {
            return EasyFindPropertiesEntities.Subscription
                .Where(x => x.OwnerID.Equals(ID) && x.Period > 0)
                .OrderByDescending(x => x.DateTCreated)
                .FirstOrDefault();
        }

        public bool IsSubscriptionActive(Guid ID)
        {
            return EasyFindPropertiesEntities.Subscription
                .Where(x => x.ID.Equals(ID) && x.IsActive == true)
                .Any();
        }
    }
}