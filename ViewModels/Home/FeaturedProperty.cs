using SS.Models;

namespace SS.ViewModels
{
    public class FeaturedProperty : Property
    {
        public string PrimaryImageURL { get; set; }
        public int AverageRating { get; set; }
        public bool ShowRating { get; set; }
        public bool isFurnished { get; set; }
        public string OwnerContactNum { get; set; }
        public bool IsPropertySaved { get; set; }
        public string DateAddedModified { get; set; }
    }
}