using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SS.Code;
using System.Collections;

namespace SS.Models
{
    public static class HomeDAO
    {
        private static EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities();
        /// <summary>
        /// get property information for the home page
        /// </summary>
        /// <param name="fetchAmount"></param>
        /// <param name="parish"></param>
        /// <returns></returns>
       /*
        public static PropertiesInformation getHomeProperties(short fetchAmount, string parish)
        {
            PropertiesInformation propertiesInformation = new PropertiesInformation();
            dynamic accommodationsInformation = (dynamic)null;
            dynamic housesInformation = (dynamic)null;
            dynamic landsInformation = (dynamic)null;

            if (!string.IsNullOrEmpty(parish))
            {
                accommodationsInformation = (from a in dbCtx.ACCOMMODATIONS
                                             where a.PARISH == parish
                                             orderby a.DATE_ADDED descending
                                             select new
                                             {
                                                 ID = a.ID.ToString() ?? "",
                                                 Parish = a.PARISH ?? "",
                                                 Price = a.PRICE.ToString() ?? "",
                                                 StreetAddress = a.STREET_ADDRESS ?? "",
                                                 IMGURL = a.IMAGE_URL ?? "",
                                                 //getting these items to allow view to distinctly identify each property type
                                                 Occupancy = a.OCCUPANCY.ToString() ?? ""
                                             }).Take(fetchAmount).ToList();

                housesInformation = (from h in dbCtx.HOUSE
                                     where h.PARISH == parish
                                     orderby h.DATE_ADDED descending
                                     select new
                                     {
                                         ID = h.ID.ToString() ?? "",
                                         Parish = h.PARISH ?? "",
                                         Price = h.PRICE.ToString() ?? "",
                                         StreetAddress = h.STREET_ADDRESS ?? "",
                                         IMGURL = h.IMAGE_URL ?? "",
                                         //getting these items to allow view to distinctly identify each property type
                                         BedroomAmt = h.BED_ROOM_AMOUNT ?? "",
                                     }).Take(fetchAmount).ToList();

                landsInformation = (from l in dbCtx.LAND
                                    where l.PARISH == parish
                                    orderby l.DATE_ADDED descending
                                    select new
                                    {
                                        ID = l.ID.ToString() ?? "",
                                        Parish = l.PARISH ?? "",
                                        Price = l.PRICE.ToString() ?? "",
                                        StreetAddress = l.STREET_ADDRESS ?? "",
                                        IMGURL = l.IMAGE_URL ?? "",
                                        //getting these items to allow view to distinctly identify each property type
                                        Area = l.AREA.ToString() ?? "",
                                    }).Take(fetchAmount).ToList();
            }
            else
            {
                accommodationsInformation = (from a in dbCtx.ACCOMMODATIONS
                                             orderby a.DATE_ADDED descending
                                             select new
                                             {
                                                 ID = a.ID.ToString() ?? "",
                                                 Parish = a.PARISH ?? "",
                                                 Price = a.PRICE.ToString() ?? "",
                                                 StreetAddress = a.STREET_ADDRESS ?? "",
                                                 IMGURL = a.IMAGE_URL ?? "",
                                                 //getting these items to allow view to distinctly identify each property type
                                                 Occupancy = a.OCCUPANCY.ToString() ?? ""
                                             }).Take(fetchAmount).ToList();

                housesInformation = (from h in dbCtx.HOUSE
                                     orderby h.DATE_ADDED descending
                                     select new
                                     {
                                         ID = h.ID.ToString() ?? "",
                                         Parish = h.PARISH ?? "",
                                         Price = h.PRICE.ToString() ?? "",
                                         StreetAddress = h.STREET_ADDRESS ?? "",
                                         IMGURL = h.IMAGE_URL ?? "",
                                         //getting these items to allow view to distinctly identify each property type
                                         BedroomAmt = h.BED_ROOM_AMOUNT ?? "",
                                     }).Take(fetchAmount).ToList();

                landsInformation = (from l in dbCtx.LAND
                                    orderby l.DATE_ADDED descending
                                    select new
                                    {
                                        ID = l.ID.ToString() ?? "",
                                        Parish = l.PARISH ?? "",
                                        Price = l.PRICE.ToString() ?? "",
                                        StreetAddress = l.STREET_ADDRESS ?? "",
                                        IMGURL = l.IMAGE_URL ?? "",
                                        //getting these items to allow view to distinctly identify each property type
                                        Area = l.AREA.ToString() ?? "",
                                    }).Take(fetchAmount).ToList();
            }

            if (!ReferenceEquals(accommodationsInformation, null))
                //setting accommodation information on propertiesInformation object
                foreach (var a in accommodationsInformation)
                {
                    propertiesInformation.accommodation = new AccommodationModel();

                    propertiesInformation.accommodation.setModel(a.ID, a.StreetAddress, a.Parish, a.Price,
                         a.IMGURL, a.Occupancy, null, null);

                    propertiesInformation.accommodationList.Add(propertiesInformation.accommodation);
                }
            if (!ReferenceEquals(housesInformation, null))
                //setting house information on propertiesInformation object
                foreach (var h in housesInformation)
                {
                    propertiesInformation.house = new HouseModel();

                    propertiesInformation.house.setModel(h.ID, h.StreetAddress, h.Parish, h.Price,
                         h.IMGURL, null, h.BedroomAmt, null);

                    propertiesInformation.houseList.Add(propertiesInformation.house);
                }
            if (!ReferenceEquals(landsInformation, null))
                //setting landformation on propertiesInformation object
                foreach (var l in landsInformation)
                {
                    propertiesInformation.land = new LandModel();

                    propertiesInformation.land.setModel(l.ID, l.StreetAddress, l.Parish, l.Price,
                         l.IMGURL, null, null, l.Area);

                    propertiesInformation.landList.Add(propertiesInformation.land);
                }

            return propertiesInformation;
        }
        /// <summary>
        /// function used to retrieve paginated results of properties on the home page 
        /// </summary>
        /// <param name="fetchAmount"></param>
        /// <param name="parish"></param>
        /// <param name="propertyType"></param>
        /// <param name="pgNo"></param>
        /// <returns></returns>/
        public static PropertiesInformation getHomePropertiesPagination(short fetchAmount, string parish, string propertyType, int pgNo)
        {
            PropertiesInformation propertiesInformation = new PropertiesInformation();
            dynamic accommodationsInformation = (dynamic)null;
            dynamic housesInformation = (dynamic)null;
            dynamic landsInformation = (dynamic)null;

            if (!string.IsNullOrEmpty(parish))
            {
                switch (propertyType)
                {
                    case "accommodations":
                        accommodationsInformation = (from a in dbCtx.ACCOMMODATIONS
                                                     where a.PARISH == parish
                                                     orderby a.DATE_ADDED descending
                                                     select new
                                                     {
                                                         ID = a.ID.ToString() ?? "",
                                                         Parish = a.PARISH ?? "",
                                                         Price = a.PRICE.ToString() ?? "",
                                                         StreetAddress = a.STREET_ADDRESS ?? "",
                                                         IMGURL = a.IMAGE_URL ?? "",
                                                         //getting these items to allow view to distinctly identify each property type
                                                         Occupancy = a.OCCUPANCY.ToString() ?? ""
                                                     }).Skip(fetchAmount * pgNo).Take(fetchAmount).ToList();
                        break;
                    case "house":
                        housesInformation = (from h in dbCtx.HOUSE
                                             where h.PARISH == parish
                                             orderby h.DATE_ADDED descending
                                             select new
                                             {
                                                 ID = h.ID.ToString() ?? "",
                                                 Parish = h.PARISH ?? "",
                                                 Price = h.PRICE.ToString() ?? "",
                                                 StreetAddress = h.STREET_ADDRESS ?? "",
                                                 IMGURL = h.IMAGE_URL ?? "",
                                                 //getting these items to allow view to distinctly identify each property type
                                                 BedroomAmt = h.BED_ROOM_AMOUNT ?? "",
                                             }).Skip(fetchAmount * pgNo).Take(fetchAmount).ToList();
                        break;
                    case "land":
                        landsInformation = (from l in dbCtx.LAND
                                            where l.PARISH == parish
                                            orderby l.DATE_ADDED descending
                                            select new
                                            {
                                                ID = l.ID.ToString() ?? "",
                                                Parish = l.PARISH ?? "",
                                                Price = l.PRICE.ToString() ?? "",
                                                StreetAddress = l.STREET_ADDRESS ?? "",
                                                IMGURL = l.IMAGE_URL ?? "",
                                                //getting these items to allow view to distinctly identify each property type
                                                Area = l.AREA.ToString() ?? "",
                                            }).Skip(fetchAmount * pgNo).Take(fetchAmount).ToList();
                        break;
                }
            }
            else
            {
                switch (propertyType)
                {
                    case "accommodations":
                        accommodationsInformation = (from a in dbCtx.ACCOMMODATIONS
                                                     orderby a.DATE_ADDED descending
                                                     select new
                                                     {
                                                         ID = a.ID.ToString() ?? "",
                                                         Parish = a.PARISH ?? "",
                                                         Price = a.PRICE.ToString() ?? "",
                                                         StreetAddress = a.STREET_ADDRESS ?? "",
                                                         IMGURL = a.IMAGE_URL ?? "",
                                                         //getting these items to allow view to distinctly identify each property type
                                                         Occupancy = a.OCCUPANCY.ToString() ?? ""
                                                     }).Skip(fetchAmount * pgNo).Take(fetchAmount).ToList();
                        break;
                    case "house":
                        housesInformation = (from h in dbCtx.HOUSE
                                             orderby h.DATE_ADDED descending
                                             select new
                                             {
                                                 ID = h.ID.ToString() ?? "",
                                                 Parish = h.PARISH ?? "",
                                                 Price = h.PRICE.ToString() ?? "",
                                                 StreetAddress = h.STREET_ADDRESS ?? "",
                                                 IMGURL = h.IMAGE_URL ?? "",
                                                 //getting these items to allow view to distinctly identify each property type
                                                 BedroomAmt = h.BED_ROOM_AMOUNT ?? "",
                                             }).Skip(fetchAmount * pgNo).Take(fetchAmount).ToList();
                        break;
                    case "land":
                        landsInformation = (from l in dbCtx.LAND
                                            orderby l.DATE_ADDED descending
                                            select new
                                            {
                                                ID = l.ID.ToString() ?? "",
                                                Parish = l.PARISH ?? "",
                                                Price = l.PRICE.ToString() ?? "",
                                                StreetAddress = l.STREET_ADDRESS ?? "",
                                                IMGURL = l.IMAGE_URL ?? "",
                                                //getting these items to allow view to distinctly identify each property type
                                                Area = l.AREA.ToString() ?? "",
                                            }).Skip(fetchAmount * pgNo).Take(fetchAmount).ToList();
                        break;
                }
            }
            if (!ReferenceEquals(accommodationsInformation, null))
                //setting accommodation information on propertiesInformation object
                foreach (var a in accommodationsInformation)
                {
                    propertiesInformation.accommodation = new AccommodationModel();

                    propertiesInformation.accommodation.setModel(a.ID, a.StreetAddress, a.Parish, a.Price,
                         a.IMGURL, a.Occupancy, null, null);

                    propertiesInformation.accommodationList.Add(propertiesInformation.accommodation);
                }
            if (!ReferenceEquals(housesInformation, null))
                //setting house information on propertiesInformation object
                foreach (var h in housesInformation)
                {
                    propertiesInformation.house = new HouseModel();

                    propertiesInformation.house.setModel(h.ID, h.StreetAddress, h.Parish, h.Price,
                         h.IMGURL, null, h.BedroomAmt, null);

                    propertiesInformation.houseList.Add(propertiesInformation.house);
                }
            if (!ReferenceEquals(landsInformation, null))
                //setting landformation on propertiesInformation object
                foreach (var l in landsInformation)
                {
                    propertiesInformation.land = new LandModel();

                    propertiesInformation.land.setModel(l.ID, l.StreetAddress, l.Parish, l.Price,
                         l.IMGURL, null, null, l.Area);

                    propertiesInformation.landList.Add(propertiesInformation.land);
                }

            return propertiesInformation;
        }*/
    }
}