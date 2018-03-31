using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface IDivisionRepository : IRepository<Division>
    {
        IEnumerable<string> GetAllDivisionNames();
    }
}