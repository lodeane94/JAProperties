using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface ISubscriptionExtensionRepository : IRepository<SubscriptionExtension>
    {
        bool HasSubscriptionExtensionByPaymentID(Guid paymentID);
        SubscriptionExtension GetSubscriptionExtByPaymentID(Guid paymentID);
    }
}