using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface ISubscriptionTypeRepository : IRepository<SubscriptionType>
    {
        String GetSubsciptionNameById(String Id);
        String GetSubsciptionDescriptionById(String Id);
        Decimal GetSubsciptionMonthlyCostById(String Id);
    }
}
