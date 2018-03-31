using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class PaymentRepository :Repository<Payment>, IPaymentRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public PaymentRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<Payment> GetPaymentsByOwnerID(Guid ID, int take = 16, int pgNo = 0)
        {
            return EasyFindPropertiesEntities.Payment
                .Where(x => x.Subscription.OwnerID.Equals(ID))
                .OrderByDescending(x => x.DateTCreated)
                .ThenByDescending(x => x.DateTModified)
                .Skip(pgNo * take)
                .Take(take)
                .ToList();
        }

        public IEnumerable<Payment> GetVerifiedPaymentsByOwnerID(Guid ID, int take = 16, int pgNo = 0)
        {
            return EasyFindPropertiesEntities.Payment
                .Where(x => x.Subscription.OwnerID.Equals(ID) && x.IsVerified == true)
                .OrderByDescending(x => x.DateTCreated)
                .ThenByDescending(x => x.DateTModified)
                .Skip(pgNo * take)
                .Take(take)
                .ToList();
        }

        public IEnumerable<Payment> GetAllPaymentsOrdered(int take = 16, int pgNo = 0)
        {
            return EasyFindPropertiesEntities.Payment
                .OrderByDescending(x => x.DateTCreated)
                .ThenByDescending(x => x.DateTModified)
                .Skip(pgNo * take)
                .Take(take)
                .ToList();
        }

        public int GetPaymentsByOwnerIDCount(Guid ID)
        {
            return EasyFindPropertiesEntities.Payment
                .Where(x => x.Subscription.OwnerID.Equals(ID))
                .Count();
        }

        public bool IsPaymentVerified(Guid ID)
        {
            return EasyFindPropertiesEntities.Payment
                .Where(x => x.ID.Equals(ID) && x.IsVerified == true)
                .Any();
        }

        public int GetAllPaymentsCount()
        {
            return EasyFindPropertiesEntities.Payment.Count();
        }
    }
}