using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface ITennantRepository : IRepository<Tennant>
    {
        IEnumerable<Tennant> GetTennantsByPropertyId(Guid Id);
        IEnumerable<Tennant> GetTennantsByOwnerId(Guid Id);
        Tennant GetTennantByUserId(Guid Id);
    }
}
