using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class UserTypeRepository : Repository<UserType>, IUserTypeRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public UserTypeRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }
    }
}