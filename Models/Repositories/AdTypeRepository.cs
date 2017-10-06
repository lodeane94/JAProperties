using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class AdTypeRepository : Repository<AdType>, IAdTypeRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public AdTypeRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public string GetAdTypeDescriptionById(string Id)
        {
            return EasyFindPropertiesEntities.AdType.Find(Id).Description;
        }

        public string GetAdTypeNameById(string Id)
        {
            return EasyFindPropertiesEntities.AdType.Find(Id).Name;
        }

        public IEnumerable<string> GetAllAdTypeNames()
        {
            return EasyFindPropertiesEntities.AdType.Select(x => x.Name).ToList();
        }
    }
}