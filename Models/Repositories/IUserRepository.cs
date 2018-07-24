using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        bool DoesUserExist(string email);
        User GetUserByEmail(string email);
        User GetUserByCellNum(string cellnum);
    }
}