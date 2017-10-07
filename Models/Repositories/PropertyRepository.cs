using SS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;

namespace SS.Models.Repositories
{
    public class PropertyRepository : Repository<Property>, IPropertyRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public PropertyRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<Property> GetPropertiesByOwnerId(Guid Id)
        {
            return EasyFindPropertiesEntities.Property.Where(x => x.ID.Equals(Id)).ToList();
        }

        public IEnumerable<Property> GetFeaturedProperties(int take)
        {
            /*ensuring that all property categories are retrieved*/
            IEnumerable<Property> realEstateProperties = EasyFindPropertiesEntities.Property
                .Where(x => x.Availability.Equals(true) && x.PropertyType.CategoryCode.Equals(EFPConstants.PropertyCategory.RealEstate))
                .OrderBy(x => x.AdPriority.Value)
                .ThenBy(x => x.DateTCreated)
                .Take(take)
                .ToList();

            IEnumerable<Property> lotProperties = EasyFindPropertiesEntities.Property
                .Where(x => x.Availability.Equals(true) && x.PropertyType.CategoryCode.Equals(EFPConstants.PropertyCategory.Lot))
                .OrderBy(x => x.AdPriority.Value)
                .ThenBy(x => x.DateTCreated)
                .Take(take)
                .ToList();

            IEnumerable<Property> machineryProperties = EasyFindPropertiesEntities.Property
                .Where(x => x.Availability.Equals(true) && x.PropertyType.CategoryCode.Equals(EFPConstants.PropertyCategory.Machinery))
                .OrderBy(x => x.AdPriority.Value)
                .ThenBy(x => x.DateTCreated)
                .Take(take)
                .ToList();

            return realEstateProperties.Concat(lotProperties).Concat(machineryProperties);
        }

        public IEnumerable<Property> FindProperties(Expression<Func<Property, bool>> predicate, int take = 16, int pgNo = 0)
        {
            return EasyFindPropertiesEntities.Property
               .Where(predicate)
               .OrderBy(x => x.AdPriority.Value)
               .ThenBy(x => x.DateTCreated)
               .Skip(pgNo * take)
               .Take(take)
               .ToList();
        }

        public IEnumerable<Property> FindPropertiesByCategoryCode(string categoryCode, int take = 16, int pgNo = 0)
        {
            return EasyFindPropertiesEntities.Property
                .Where(x => x.Availability.Equals(true) && x.PropertyType.CategoryCode.Equals(categoryCode))
                .OrderBy(x => x.AdPriority.Value)
                .ThenBy(x => x.DateTCreated)
                .Take(take)
                .ToList();
        }
    }
}