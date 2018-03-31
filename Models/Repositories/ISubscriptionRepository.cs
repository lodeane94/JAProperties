using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface ISubscriptionRepository : IRepository<Subscription>
    {
        IEnumerable<Subscription> GetSubscriptionsByTypeCode(String code);
        Subscription GetSubscriptionByOwnerID(Guid ID);
    }
}
