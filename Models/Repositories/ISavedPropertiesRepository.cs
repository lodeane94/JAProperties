using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface ISavedPropertiesRepository : IRepository<SavedProperties>
    {
        IEnumerable<PropertyImage> GetSavedPropertiesImagesByUserId(Guid userId);
        IEnumerable<SavedProperties> GetSavedPropertiesByUserId(Guid userId);
        bool IsPropertySavedForUser(Guid userId, Guid propertyId);
        SavedProperties GetSavedProperty(Guid userId, Guid propertyId);
    }
}