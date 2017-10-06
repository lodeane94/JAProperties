using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface ITennantRepository : IRepository<Tennant>
    {
        IEnumerable<Tennant> GetTennantsByPropertyId(String Id);
    }
}
