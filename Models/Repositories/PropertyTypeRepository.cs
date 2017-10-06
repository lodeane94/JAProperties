using SS.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class PropertyTypeRepository : Repository<PropertyType>, IPropertyTypeRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public PropertyTypeRepository(EasyFindPropertiesEntities dbCtx)
            :base(dbCtx)
        { }

        public IEnumerable<String> GetPropertyTypesByCategoryCode(String code)
        {
            return EasyFindPropertiesEntities.PropertyType.Where(s => s.CategoryCode.Equals(code)).Select(s => s.Name).ToList();
        }

        public IEnumerable<string> GetAllPropertyTypeNames()
        {
            throw new NotImplementedException();
        }

        public Guid GetPropertyTypeIDByName(string name)
        {
            return EasyFindPropertiesEntities.PropertyType.Where(s => s.Name.Equals(name)).Select(s => s.ID).Single();
        }
    }
}