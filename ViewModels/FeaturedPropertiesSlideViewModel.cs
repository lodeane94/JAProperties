﻿using SS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class FeaturedPropertiesSlideViewModel
    {
        public Property property { get; set; }
        public Owner owner { get; set; }
        public IEnumerable<String> tags { get; set; }
        public IEnumerable<String> propertyImageURLs { get; set; }
        public String propertyPrimaryImageURL { get; set; }
        public IEnumerable<PropertyRating> propRatings { get; set; }
        public int averageRating { get; set; }
        public String OriginAddress { get; set; }
        public String Distance { get; set; }
        public String Duration { get; set; }
    }
}