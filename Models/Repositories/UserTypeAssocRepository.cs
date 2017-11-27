using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SS.Core;

namespace SS.Models.Repositories
{
    public class UserTypeAssocRepository : Repository<UserTypeAssoc>, IUserTypeAssocRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public UserTypeAssocRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<String> GetUserTypesByUserID(Guid ID)
        {
            return EasyFindPropertiesEntities.UserTypeAssoc.Where(x => x.UserID.Equals(ID)).Select(x => x.UserTypeCode).ToList();
        }

        public IEnumerable<UserTypeAssoc> GetUserTypeAssocsByUserID(Guid ID)
        {
            return EasyFindPropertiesEntities.UserTypeAssoc.Where(x => x.UserID.Equals(ID)).ToList();
        }

        public UserTypeAssoc GetTennantUserTypeAssocByUserID(Guid ID)
        {
            return EasyFindPropertiesEntities.UserTypeAssoc
                .Where(x => x.UserID.Equals(ID) && x.UserTypeCode.Equals(EFPConstants.UserType.Tennant))
                .Single();
        }
    }
}