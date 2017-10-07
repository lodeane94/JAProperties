
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class PropertyPurposeRepository :Repository<PropertyPurpose>, IPropertyPurposeRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public PropertyPurposeRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public string GetPurposeNameById(string Id)
        {
            return EasyFindPropertiesEntities.PropertyPurpose.Find(Id).Name;
        }

        public IEnumerable<string> GetAllPurposeNames()
        {
            return EasyFindPropertiesEntities.PropertyPurpose.Select(x => x.Name).ToList();
        }

        public string GetPurposeCodeByName(string name)
        {
            return EasyFindPropertiesEntities.PropertyPurpose.Where(x => x.Name.Equals(name)).Select(x => x.ID).Single();
        }
    }
}