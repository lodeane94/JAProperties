using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface IPropertyCategoryRepository : IRepository<PropertyCategory>
    {
        String GetCategoryNameById(String Id);
        IEnumerable<String> GeAllCategoryNames();
        String GetCategoryDescriptionById(String Id);
    }
}
