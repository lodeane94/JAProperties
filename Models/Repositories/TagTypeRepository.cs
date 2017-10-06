using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class TagTypeRepository : Repository<TagType>, ITagTypeRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public TagTypeRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<string> GetTagNamesByPropertyCategoryCode(string code)
        {
            return EasyFindPropertiesEntities.TagType.Where(x => x.PropertyCategoryCode.Equals(code)).Select(x => x.Name).ToList();
        }

        public IEnumerable<string> GetAllTagTypeNames()
        {
            return EasyFindPropertiesEntities.TagType.Select(x => x.Name);
        }

        public Guid GetTagTypeIDByTagName(string name)
        {
            return EasyFindPropertiesEntities.TagType.Where(x => x.Name.Equals(name)).Select(x => x.ID).Single();
        }
    }
}