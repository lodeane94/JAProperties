using System;
using System.Collections.Generic;

namespace SS.ViewModels
{
    public class SelectedPropertyViewModel : FeaturedPropertiesSlideViewModel
    {
        public string AdType { get; internal set; }
        public string OwnerLastName { get; internal set; }
        public string OwnerFirstName { get; internal set; }
        public string OwnerCellNumber { get; internal set; }
        public string PropertyCondition { get; internal set; }
        public string PropertyCategoryCode { get; internal set; }
        public IEnumerable<String> Tags { get; internal set; }
        public IEnumerable<String> PropertyImageURLs { get; internal set; }
    }
}