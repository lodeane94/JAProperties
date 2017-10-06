using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class AdPriorityRepository : Repository<AdPriority>, IAdPriorityRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public AdPriorityRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        {}

        public decimal GetAdPriorityCostById(string Id)
        {
            return EasyFindPropertiesEntities.AdPriority.Find(Id).Cost;
        }

        public string GetAdPriorityDescriptionById(string Id)
        {
            return EasyFindPropertiesEntities.AdPriority.Find(Id).Description;
        }

        public string GetAdPriorityNameById(string Id)
        {
            return EasyFindPropertiesEntities.AdPriority.Find(Id).Name;
        }

        public IEnumerable<string> GetAllAdPriorityNames()
        {
            return EasyFindPropertiesEntities.AdPriority.Select(x => x.Name).ToList();
        }
    }
}