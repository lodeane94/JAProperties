using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface IUserTypeAssocRepository : IRepository<UserTypeAssoc>
    {
        IEnumerable<String> GetUserTypesByUserID(Guid ID);
        IEnumerable<UserTypeAssoc> GetUserTypeAssocsByUserID(Guid ID);
        UserTypeAssoc GetTennantUserTypeAssocByUserID(Guid ID);
    }
}