using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class PaymentMethodRepository : Repository<PaymentMethod>, IPaymentMethodRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public PaymentMethodRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public List<string> GetAllPaymentMethodNames()
        {
            return EasyFindPropertiesEntities.PaymentMethod.Select(x => x.Name).ToList();
        }
    }
}