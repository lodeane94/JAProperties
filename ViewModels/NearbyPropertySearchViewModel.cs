using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class NearbyPropertySearchViewModel
    {
        public dynamic OriginAddress { get; set; }
        public dynamic DestinationInformation { get; set; }
    }
}