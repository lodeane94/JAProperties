using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class SelectedPropertyViewModel : FeaturedPropertiesSlideViewModel
    {
        public string AdType { get; internal set; }
        public string OwnerLastName { get; internal set; }
        public string OwnerFirstName { get; internal set; }
        public string OwnerCellNumber { get; internal set; }
        public string PropertyCondition { get; internal set; }
    }
}