using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface IPropertyConditionRepository : IRepository<PropertyCondition>
    {
        String GetConditionNameById(String Id);
        String GetConditionDescriptionById(String Id);
    }
}
