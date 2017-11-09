using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class OwnerRepository : Repository<Owner>, IOwnerRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public OwnerRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public Owner GetOwnerByUserID(Guid ID)
        {
            return EasyFindPropertiesEntities.Owner.Where(x => x.UserID.Equals(ID)).Single();
        }

        /* public bool DoesOwnerExist(string cellNum)
         {
             int count = EasyFindPropertiesEntities.Owner.Where(x => x.CellNum.Equals(cellNum)).Count();

             return count > 0 ? true : false;
         }

         public Guid GetOwnerIDByCellNum(string cellNum)
         {
             return EasyFindPropertiesEntities.Owner.Where(x => x.CellNum.Equals(cellNum)).Select(x => x.ID).Single();
         }*/
    }
}