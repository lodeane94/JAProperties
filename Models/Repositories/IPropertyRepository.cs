using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface IPropertyRepository : IRepository<Property>
    {
        IEnumerable<Property> GetPropertiesByOwnerId(Guid Id);
        IEnumerable<Property> GetFeaturedProperties(int take);
    }
}
