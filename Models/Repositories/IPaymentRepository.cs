using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        IEnumerable<Payment> GetPaymentsByOwnerID(Guid ID, int take = 16, int pgNo = 0);
        IEnumerable<Payment> GetVerifiedPaymentsByOwnerID(Guid ID, int take = 16, int pgNo = 0);
        IEnumerable<Payment> GetAllPaymentsOrdered(int take = 16, int pgNo = 0);
        int GetPaymentsByOwnerIDCount(Guid ID);
        int GetAllPaymentsCount();
        bool IsPaymentVerified(Guid ID);
    }
}