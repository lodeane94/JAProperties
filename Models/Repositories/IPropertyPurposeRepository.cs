using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface IPropertyPurposeRepository : IRepository<PropertyPurpose>
    {
        String GetPurposeNameById(String Id);
        IEnumerable<String> GetAllPurposeNames();
        String GetPurposeCodeByName(String name);
    }
}
