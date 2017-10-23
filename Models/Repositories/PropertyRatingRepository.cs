using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class PropertyRatingRepository : Repository<PropertyRating>, IPropertyRatingRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public PropertyRatingRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<int> GetPropertyRatingsCountByPropertyId(Guid Id)
        {
            return EasyFindPropertiesEntities.PropertyRating.Where(x => x.PropertyID.Equals(Id)).Select(x => x.Ratings).ToList();
        }

        public IEnumerable<String> GetPropertyCommentsByPropertyId(Guid Id)
        {
            return EasyFindPropertiesEntities.PropertyRating.Where(x => x.PropertyID.Equals(Id)).Select(x => x.Comments).ToList();
        }

        public IEnumerable<PropertyRating> GetPropertyRatingsByPropertyId(Guid Id)
        {
            return EasyFindPropertiesEntities.PropertyRating.Where(x => x.PropertyID.Equals(Id)).ToList();
        }
    }
}