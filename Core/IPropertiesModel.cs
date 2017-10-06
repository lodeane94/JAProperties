using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Code
{
    public interface IPropertiesModel
    {
        void setModel(string id, string sa, string parish, string price, string imageURL, string occupancy, string bedroomAmount, string area);
        Object getModel();
    }
}