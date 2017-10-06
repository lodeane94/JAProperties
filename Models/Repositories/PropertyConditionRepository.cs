using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class PropertyConditionRepository : Repository<PropertyCondition>, IPropertyConditionRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public PropertyConditionRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public string GetConditionNameById(string Id)
        {
            return EasyFindPropertiesEntities.PropertyCondition.Find(Id).Name;
        }

        public string GetConditionDescriptionById(string Id)
        {
            return EasyFindPropertiesEntities.PropertyCondition.Find(Id).Description;
        }
    }
}