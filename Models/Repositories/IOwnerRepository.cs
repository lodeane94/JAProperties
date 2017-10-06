using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface IOwnerRepository : IRepository<Owner>
    {
        bool DoesOwnerExist(String cellNum);
        Guid GetOwnerIDByCellNum(String cellNum);
    }
}