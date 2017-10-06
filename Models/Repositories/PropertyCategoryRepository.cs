using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class PropertyCategoryRepository : Repository<PropertyCategory>, IPropertyCategoryRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public PropertyCategoryRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public String GetCategoryNameById(string Id)
        {
            return EasyFindPropertiesEntities.PropertyCategory.Find(Id).Name;
        }

        public IEnumerable<String> GeAllCategoryNames()
        {
            return EasyFindPropertiesEntities.PropertyCategory.Select(x => x.Name).ToList();
        }

        public String GetCategoryDescriptionById(string Id)
        {
            return EasyFindPropertiesEntities.PropertyCategory.Find(Id).Description;
        }
    }
}