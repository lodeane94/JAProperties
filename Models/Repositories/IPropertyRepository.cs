using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface IPropertyRepository : IRepository<Property>
    {
        IEnumerable<Property> GetPropertiesByOwnerId(Guid Id);
        Task<IEnumerable<Property>> GetFeaturedProperties(int take);
        Task<IEnumerable<Property>> FindProperties(Expression<Func<Property, bool>> predicate, int take = 10, int pgNo = 0);
        IEnumerable<Property> FindPropertiesByCategoryCode(String categoryCode, int take = 10, int pgNo = 0);
        Task<IEnumerable<Property>> FindPropertiesBySearchTerm(String searchTerm, string propertyCategory, int take = 10, int pgNo = 0);
        Task<IEnumerable<Property>> FindPropertiesByStreetAddress(List<Core.NearbyPropertySearchModel> model, int take = 10, int pgNo = 0);
        Task<Array> FindPropertiesCoordinates(Expression<Func<Property, bool>> predicate);
        Owner GetPropertyOwnerByPropID(Guid Id);
        String GetEnrolmentKeyByPropID(Guid Id);
        IEnumerable<Property> FilterPropertiesByTagNames(IEnumerable<Property> properties, IEnumerable<String> tags);
        bool IsPropertyAvailable(Guid Id);
        int GetCount(Guid ownerId);
    }
}
