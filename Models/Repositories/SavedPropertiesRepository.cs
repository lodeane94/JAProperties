using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class SavedPropertiesRepository : Repository<SavedProperties>, ISavedPropertiesRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public SavedPropertiesRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<SavedProperties> GetSavedPropertiesByUserId(Guid userId)
        {
            return EasyFindPropertiesEntities.SavedProperties.Where(x => x.UserID.Equals(userId)).ToList();
        }

        public IEnumerable<PropertyImage> GetSavedPropertiesImagesByUserId(Guid userId)
        {
            var propertyImages = (from p in EasyFindPropertiesEntities.Property
                                 join pi in EasyFindPropertiesEntities.PropertyImage on p.ID equals pi.PropertyID
                                 join s in EasyFindPropertiesEntities.SavedProperties on p.ID equals s.PropertyID
                                 where s.UserID.Equals(userId) && pi.IsPrimaryDisplay == true
                                 select pi).ToList();

            return propertyImages;
        }

        public bool IsPropertySavedForUser(Guid userId, Guid propertyId)
        {
            var isSavedCount = EasyFindPropertiesEntities.SavedProperties
                .Where(x => x.UserID.Equals(userId)
                && x.PropertyID.Equals(propertyId))
                .Count();

            return isSavedCount > 0 ? true : false;
        }

        public SavedProperties GetSavedProperty(Guid userId, Guid propertyId)
        {
            return EasyFindPropertiesEntities.SavedProperties
                .Where(x => x.UserID.Equals(userId)
                && x.PropertyID.Equals(propertyId))
                .SingleOrDefault();
        }
    }
}