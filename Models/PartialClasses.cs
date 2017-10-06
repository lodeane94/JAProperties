using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SS.Models
{
    [MetadataType(typeof(OwnerMetadata))]
    public partial class Owner
    {
    }

    [MetadataType(typeof(AccommodationMetadata))]
    public partial class ACCOMMODATIONS
    { 
    }

    [MetadataType(typeof(LandMetadata))]
    public partial class LAND
    {
    }

    [MetadataType(typeof(HouseMetadata))]
    public partial class HOUSE
    { 
    }

    [MetadataType(typeof(RequisitionMetadata))]
    public partial class REQUISITION
    { 
    }

    

}