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
        IEnumerable<Property> GetFeaturedProperties(int take);
        IEnumerable<Property> FindProperties(Expression<Func<Property, bool>> predicate, int take = 16, int pgNo = 0);
        IEnumerable<Property> FindPropertiesByCategoryCode(String categoryCode, int take = 16, int pgNo = 0);
        IEnumerable<Property> FindPropertiesBySearchTerm(String searchTerm, int take = 16, int pgNo = 0);
        IEnumerable<Property> FindPropertiesByStreetAddress(List<Core.NearbyPropertySearchModel> model, int take = 16, int pgNo = 0);
        Array FindPropertiesCoordinates(Expression<Func<Property, bool>> predicate);
        Owner GetPropertyOwnerByPropID(Guid Id);
        String GetEnrolmentKeyByPropID(Guid Id);
    }
}
