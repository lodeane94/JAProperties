using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface ITagTypeRepository : IRepository<TagType>
    {
        IEnumerable<String> GetTagNamesByPropertyCategoryCode(String code);
        IEnumerable<String> GetAllTagTypeNames();
        Guid GetTagTypeIDByTagName(String name);
    }
}
