using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface IPropertyRatingRepository : IRepository<PropertyRating>
    {
        IEnumerable<int> GetPropertyRatingsCountByPropertyId(Guid Id);
        IEnumerable<String> GetPropertyCommentsByPropertyId(Guid Id);
        IEnumerable<PropertyRating> GetPropertyRatingsByPropertyId(Guid Id);
    }
}
