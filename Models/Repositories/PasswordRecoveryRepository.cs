using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class PasswordRecoveryRepository : Repository<PasswordRecoveryRequest>, IPasswordRecoveryRequestRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public PasswordRecoveryRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public bool DoesAccessCodeExistForUser(Guid userId, string accessCode)
        {
            return EasyFindPropertiesEntities.PasswordRecoveryRequest
                .Any(x => x.UserID.Equals(userId)
                && x.AccessCode.Equals(accessCode)
                && x.ExpiryDate > DateTime.Now);
        }
    }
}