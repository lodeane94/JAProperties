using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class DivisionRepository : Repository<Division>, IDivisionRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public DivisionRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<string> GetAllDivisionNames()
        {
            return EasyFindPropertiesEntities.Division
                .OrderBy(x => x.Name)
                .Select(x => x.Name)
                .ToList();
        }
    }
}