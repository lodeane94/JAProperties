using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class SubscriptionTypeRepository : Repository<SubscriptionType>, ISubscriptionTypeRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public SubscriptionTypeRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public string GetSubsciptionNameById(string Id)
        {
            return EasyFindPropertiesEntities.SubscriptionType.Find(Id).Name;
        }

        public string GetSubsciptionDescriptionById(string Id)
        {
            return EasyFindPropertiesEntities.SubscriptionType.Find(Id).Description;
        }

        public decimal GetSubsciptionMonthlyCostById(string Id)
        {
            return EasyFindPropertiesEntities.SubscriptionType.Find(Id).MonthlyCost;
        }
    }
}