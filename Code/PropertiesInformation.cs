using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Code
{
    public class PropertiesInformation
    {
        public AccommodationModel accommodation;
        public HouseModel house;
        public LandModel land;
        public List<AccommodationModel> accommodationList = new List<AccommodationModel>();
        public List<HouseModel> houseList = new List<HouseModel>();
        public List<LandModel> landList = new List<LandModel>();

    }
}