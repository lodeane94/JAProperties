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
            return EasyFindPropertiesEntities.PropertyRequisition
                .Where(x => x.PropertyID.Equals(Id)
                && x.IsAccepted.HasValue.Equals(true)
                && x.ExpiryDate > DateTime.Now)
                .OrderByDescending(x => x.DateTCreated)
                .ToList();
        }

        public IEnumerable<PropertyRequisition> GetRequestsByOwnerId(Guid Id)
        {
            return EasyFindPropertiesEntities.PropertyRequisition
                .Where(x => x.Property.OwnerID.Equals(Id)
                && x.IsAccepted.HasValue.Equals(true)
                && x.ExpiryDate > DateTime.Now)
                .OrderByDescending(x => x.DateTCreated)
                .ToList();
        }

        public IEnumerable<PropertyRequisition> GetAcceptedRequestsByOwnerId(Guid Id)
        {
            return EasyFindPropertiesEntities.PropertyRequisition
                .Where(x => x.Property.OwnerID.Equals(Id)
                && x.IsAccepted.HasValue.Equals(true)
                && x.IsAccepted.Value.Equals(true)
                && x.ExpiryDate > DateTime.Now)
                .OrderByDescending(x => x.DateTCreated)
                .ToList();
        }

        public IEnumerable<PropertyRequisition> GetRequestsMadeByUserId(Guid Id)
        {
            return EasyFindPropertiesEntities.PropertyRequisition
                .Where(x => x.UserID.Equals(Id)
                && x.IsAccepted.HasValue.Equals(true)
                && x.ExpiryDate > DateTime.Now)
                .OrderByDescending(x => x.DateTCreated)
                .ToList();
        }
    }
}