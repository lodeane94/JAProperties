using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SS.Models;

namespace SS.Code
{
    public class UtilityHelper
    {
        
        public UtilityHelper() { }

        public static List<TennantModel> loadTennants(string username)
        {
            /*
            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();

            List<TennantModel> tennantModelList = new List<TennantModel>();

            var landlordID = dbCtx.LANDLORDS.Where(l => l.EMAIL == username)
                                .Select(l => l.ID).Single();
            //getting all tennants that are in an accommodation owned by this landlord

            var allTennants = dbCtx.sp_get_tennants_partial_info(landlordID);
            
            foreach (var tennant in allTennants)
            {
                TennantModel tennantModel = new TennantModel()
                {
                    AccommodationID = tennant.ACCOMMODATION_ID.ToString(),
                    ID = tennant.ID.ToString(),
                    FirstName = tennant.FIRST_NAME,
                    LastName = tennant.LAST_NAME,
                    PictureUrl = tennant.PICTURE_URL
                };
                tennantModelList.Add(tennantModel);
            }
            return tennantModelList;*/
            return null;
        }
    }
}