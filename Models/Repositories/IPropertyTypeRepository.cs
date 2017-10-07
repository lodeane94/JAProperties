using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface IPropertyTypeRepository : IRepository<PropertyType>
    {
        IEnumerable<String> GetPropertyTypesByCategoryCode(String code);
        IEnumerable<String> GetAllPropertyTypeNames();
        Guid GetPropertyTypeIDByName(String name);
        String GetPopertyTypeCategoryCodeByName(String name);
    }
}