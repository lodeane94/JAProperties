using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class SubscriptionExtensionRepository : Repository<SubscriptionExtension>, ISubscriptionExtensionRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public SubscriptionExtensionRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public bool HasSubscriptionExtensionByPaymentID(Guid paymentID)
        {
            return EasyFindPropertiesEntities.SubscriptionExtension
                .Where(x => x.PaymentID.Equals(paymentID))
                .Count() > 0 ? true : false;
        }

        public SubscriptionExtension GetSubscriptionExtByPaymentID(Guid paymentID)
        {
            return EasyFindPropertiesEntities.SubscriptionExtension
                .Where(x => x.PaymentID.Equals(paymentID))
                .Single();
        }
    }
}