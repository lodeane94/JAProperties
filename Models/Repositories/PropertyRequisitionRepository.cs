using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class PropertyRequisitionRepository : Repository<PropertyRequisition>, IPropertyRequisitionRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public PropertyRequisitionRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<PropertyRequisition> GetRequestsByPropertyId(Guid Id)
        {
            return EasyFindPropertiesEntities.PropertyRequisition.Where(x => x.PropertyID.Equals(Id)).ToList();
        }

        /*TODO if necessary
        public IEnumerable<PropertyRequisition> GetRequestsByOwnerId(Guid Id)
        {
            return EasyFindPropertiesEntities.PropertyRequisition.Where(x => x..Equals(Id)).ToList();
        }*/
    }
}