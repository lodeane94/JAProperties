using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Models.Repositories
{
    public interface IPropertyImageRepository : IRepository<PropertyImage>
    {
        IEnumerable<String> GetImageURLsByPropertyId(Guid Id, int take);
        IEnumerable<PropertyImage> GetAllImagesByPropertyId(Guid Id, int take);
        PropertyImage GetPrimaryImageByPropertyId(Guid Id);
        String GetPrimaryImageURLByPropertyId(Guid Id);
        String GetImageURLById(Guid Id);
        IEnumerable<PropertyImage> GetAllPrimaryPropertyImageByOwnerId(Guid Id);
    }
}
