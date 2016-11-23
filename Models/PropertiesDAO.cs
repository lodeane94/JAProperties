using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SS.Code;
using static SS.Code.PropertyConstants;

namespace SS.Models
{
    public static class PropertiesDAO
    {
        private static JAHomesEntities dbCtx = new JAHomesEntities();

        public static PropertyType getPropertyType(Guid propertyID)
        {
            /*
                     * checking the ID of the property that is being returned. 
                     * depending on the property, the appropriate image will be loaded
                     */
            var allAccommodationOwnedCount = dbCtx.ACCOMMODATIONS.Where(x => x.ID == propertyID).Count();
            var allHouseOwnedCount = dbCtx.HOUSE.Where(x => x.ID == propertyID).Count();
            var allLandOwnedCount = dbCtx.LAND.Where(x => x.ID == propertyID).Count();

            if (allAccommodationOwnedCount > 0)
                return PropertyType.accommodation;
            else if (allHouseOwnedCount > 0)
                return PropertyType.house;
            else
                return PropertyType.land;
        }
    }
}