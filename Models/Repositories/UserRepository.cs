using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public UserRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public bool DoesUserExist(string email)
        {
            int count = EasyFindPropertiesEntities.User.Where(x => x.Email.Equals(email)).Count();

            return count > 0 ? true : false;
        }

        public User GetUserByEmail(string email)
        {
            return EasyFindPropertiesEntities.User.Where(x => x.Email.Equals(email)).SingleOrDefault();
        }

        public User GetUserByCellNum(string cellnum)
        {
            return EasyFindPropertiesEntities.User.Where(x => x.CellNum.Equals(cellnum)).SingleOrDefault();
        }
    }
}