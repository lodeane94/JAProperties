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
            return EasyFindPropertiesEntities.Property.Where(x => x.OwnerID.Equals(Id)).ToList();
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
            if (predicate != null)
            {
                return EasyFindPropertiesEntities.Property
                   .Where(predicate)
                   .OrderBy(x => x.AdPriority.Value)
                   .ThenBy(x => x.DateTCreated)
                   .Skip(pgNo * take)
                   .Take(take)
                   .ToList();
            }
            else
                return new List<Property>();
        }

        public IEnumerable<Property> FindPropertiesByCategoryCode(string categoryCode, int take = 0, int pgNo = 0)
        {
            if (take > 0)
            {
                return EasyFindPropertiesEntities.Property
                .Where(x => x.Availability.Equals(true) && x.PropertyType.CategoryCode.Equals(categoryCode))
                .OrderBy(x => x.AdPriority.Value)
                .ThenBy(x => x.DateTCreated)
                .Skip(pgNo * take)
                .Take(take)
                .ToList();
            }
            else
            {
                return EasyFindPropertiesEntities.Property
                .Where(x => x.Availability.Equals(true) && x.PropertyType.CategoryCode.Equals(categoryCode))
                .OrderBy(x => x.AdPriority.Value)
                .ThenBy(x => x.DateTCreated)
                .Skip(pgNo * take)
                .ToList();
            }

        }

        public IEnumerable<Property> FindPropertiesBySearchTerm(string searchTerm, String propertyCategory, int take = 0, int pgNo = 0)
        {
            IEnumerable<Property> properties = null;

            if (!string.IsNullOrEmpty(propertyCategory))
            {
                properties = (from p in EasyFindPropertiesEntities.Property
                              join t in EasyFindPropertiesEntities.Tags on p.ID equals t.PropertyID into g
                              from t in g.DefaultIfEmpty()
                              where p.Availability.Equals(true)
                                  && (p.PropertyPurpose.Name.Contains(searchTerm)
                                      || p.AdType.Name.Contains(searchTerm)
                                      || p.PropertyType.Name.Contains(searchTerm)
                                      || p.PropertyCategory.Name.Contains(searchTerm)
                                      || p.StreetAddress.Contains(searchTerm)
                                      || p.Division.Contains(searchTerm)
                                      || p.Community.Contains(searchTerm)
                                      || p.Country.Contains(searchTerm)
                                      || p.Description.Contains(searchTerm)
                                      || t.TagType.Name.Contains(searchTerm))
                                      && p.CategoryCode.Equals(propertyCategory)
                              orderby p.AdPriority.Value, p.DateTCreated
                              select p);
            }
            else
            {
                properties = (from p in EasyFindPropertiesEntities.Property
                              join t in EasyFindPropertiesEntities.Tags on p.ID equals t.PropertyID into g
                              from t in g.DefaultIfEmpty()
                              where p.Availability.Equals(true)
                                  && (p.PropertyPurpose.Name.Contains(searchTerm)
                                      || p.AdType.Name.Contains(searchTerm)
                                      || p.PropertyType.Name.Contains(searchTerm)
                                      || p.PropertyCategory.Name.Contains(searchTerm)
                                      || p.StreetAddress.Contains(searchTerm)
                                      || p.Division.Contains(searchTerm)
                                      || p.Community.Contains(searchTerm)
                                      || p.Country.Contains(searchTerm)
                                      || p.Description.Contains(searchTerm)
                                      || t.TagType.Name.Contains(searchTerm))
                              orderby p.AdPriority.Value, p.DateTCreated
                              select p);
            }

            if (take > 0)
            {
                return properties.Skip(pgNo * take)
                    .Distinct().Take(take)
                    .ToList();
            }
            else
                return properties.Skip(pgNo * take)
                    .Distinct()
                    .ToList();
        }

        public Owner GetPropertyOwnerByPropID(Guid Id)
        {
            return EasyFindPropertiesEntities.Property.Where(x => x.ID.Equals(Id)).Select(x => x.Owner).Single();
        }

        public string GetEnrolmentKeyByPropID(Guid Id)
        {
            return EasyFindPropertiesEntities.Property.Where(x => x.ID.Equals(Id)).Select(x => x.EnrolmentKey).Single();
        }

        public Array FindPropertiesCoordinates(Expression<Func<Property, bool>> predicate)
        {
            if (predicate != null)
            {
                return EasyFindPropertiesEntities.Property
                   .Where(predicate)
                   .Select(x => new { x.Latitude, x.Longitude }).ToArray();
            }
            else
            {
                return EasyFindPropertiesEntities.Property
                    .Where(x => x.Availability == true)
                   .Select(x => new { x.Latitude, x.Longitude }).ToArray();
            }
        }

        public IEnumerable<Property> FindPropertiesByStreetAddress(List<NearbyPropertySearchModel> model, int take = 16, int pgNo = 0)
        {
            //TODO use another alternative to AsEnumerable as this will execute slowly
            var properties = (from p in EasyFindPropertiesEntities.Property.AsEnumerable()
                              where model.Select(x => x.StreetAddress).Contains(p.StreetAddress)
                              select p
                              )
                              .OrderBy(x => x.AdPriority.Value)
                              .ThenBy(x => x.DateTCreated)
                              .Skip(pgNo * take)
                              .Take(take)
                              .ToList();

            return properties;
        }

        public IEnumerable<Property> FilterPropertiesByTagNames(IEnumerable<Property> properties, IEnumerable<String> tags)
        {
            List<Property> taggedProperties = new List<Property>();

            foreach (var property in properties)
            {
                bool hasTags = property.Tags.Any(x => tags.Contains(x.TagType.Name));
                if (hasTags)
                    taggedProperties.Add(property);
            }

            return taggedProperties;
        }

        public bool IsPropertyAvailable(Guid Id)
        {
            return EasyFindPropertiesEntities.Property
                .Any(x => x.ID.Equals(Id) && x.Availability.Equals(true));
        }

        public int GetCount(Guid ownerId)
        {
            return EasyFindPropertiesEntities.Property
                .Where(x => x.OwnerID.Equals(ownerId))
                .Count();
        }
    }
}