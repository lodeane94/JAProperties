using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface IPropertyRequisitionRepository : IRepository<PropertyRequisition>
    {
        IEnumerable<PropertyRequisition> GetRequestsByPropertyId(Guid Id);
        IEnumerable<PropertyRequisition> GetRequestsByOwnerId(Guid Id);
        IEnumerable<PropertyRequisition> GetRequestsMadeByUserId(Guid Id);
        IEnumerable<PropertyRequisition> GetAcceptedRequestsByOwnerId(Guid Id);
        IEnumerable<User> GetRequestedPropertyUsersByOwnerId(Guid Id);
        IEnumerable<User> GetRequestedPropertyUsers(Guid RequesteeUserId);
        IEnumerable<PropertyRequisition> GetRequestsHistoryByUserId(Guid Id, int take = 10, int pgNo = 0);
    }
}
