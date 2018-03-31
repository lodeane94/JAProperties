using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class TagsRepository : Repository<Tags>, ITagsRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public TagsRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<string> GetTagNamesByPropertyId(Guid id)
        {
            return EasyFindPropertiesEntities.Tags.Where(x => x.PropertyID.Equals(id)).Select(x => x.TagType.Name).ToList();
        }

        public List<string> GetTagNamesByProperties(IEnumerable<Property> properties)
        {
            List<Guid> propertyIDs = new List<Guid>();

            foreach (var property in properties)
            {
                propertyIDs.Add(property.ID);
            }

            return EasyFindPropertiesEntities.Tags
                .Where(x => propertyIDs.Contains(x.PropertyID))
                .Select(x => x.TagType.Name).Distinct().ToList();
        }
    }
}