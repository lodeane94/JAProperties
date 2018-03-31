using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface IPasswordRecoveryRequestRepository : IRepository<PasswordRecoveryRequest>
    {
        bool DoesAccessCodeExistForUser(Guid userId, string accessCode);
    }
}