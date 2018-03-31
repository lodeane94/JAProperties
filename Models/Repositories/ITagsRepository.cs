using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface ITagsRepository : IRepository<Tags> 
    {
        IEnumerable<String> GetTagNamesByPropertyId(Guid id);
        List<string> GetTagNamesByProperties(IEnumerable<Property> properties);
    }
}