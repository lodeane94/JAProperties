using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Core
{
    public static class EFPConstants
    {
        public static class UserType
        {
            public static readonly String Consumer = "C";
            public static readonly String PropertyOwner = "O";
            public static readonly String Tennant = "T";
        }

        public static class PropertyCategory
        {
            public static readonly String  RealEstate = "R";
            public static readonly String Lot = "L";
            public static readonly String Machinery = "M";
        }

        public static class PropertySubscriptionType
        {
            public static readonly String Basic = "B";
            public static readonly String Landlord = "L";
            public static readonly String Realtor = "R";
        }

        public static class PropertyPurpose
        {
            public static readonly String Commercial = "C";
            public static readonly String Industrial = "I";
            public static readonly String Residential = "R";
        }

        public static class PropertyAdType
        {
            public static readonly String Lease = "L";
            public static readonly String Rent = "R";
            public static readonly String Sale ="S";
        }


        public static class PropertyAdPriority
        {
            public static readonly String AdPremium = "M";
            public static readonly String AdPro = "P";
            public static readonly String Regular = "R";
        }

        public static class PropertyCondition
        {
            public static readonly String Bad = "B";
            public static readonly String Good = "G";
            public static readonly String Fair = "F";
            public static readonly String Excellent = "E";
            public static readonly String NotSurveyed = "N";
        }

        public enum RoleNames
        {
            Landlord,
            Realtor,
            Tennant,
            Basic,
            Consumer
        }

    }
}