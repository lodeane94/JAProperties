
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface IAdPriorityRepository : IRepository<AdPriority>
    {
        String GetAdPriorityNameById(String Id);
        String GetAdPriorityDescriptionById(String Id);
        IEnumerable<String> GetAllAdPriorityNames();
        Decimal GetAdPriorityCostById(String Id);
    }
}
