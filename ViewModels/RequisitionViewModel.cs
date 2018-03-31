using SS.Models;
using System;

namespace SS.ViewModels
{
    public class RequisitionViewModel
    {
        public String ImageUrl { get; set; }
        public PropertyRequisition PropertyRequisition;
        public bool isUserPropOwner { get; set; }

        public RequisitionViewModel()
        {
            this.PropertyRequisition = new PropertyRequisition();
        }
    }
}