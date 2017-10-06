using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface IAdTypeRepository : IRepository<AdType>
    {
        String GetAdTypeNameById(String Id);
        String GetAdTypeDescriptionById(String Id);
        IEnumerable<String> GetAllAdTypeNames();
    }
}
