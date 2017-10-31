using SS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class RequisitionViewModel
    {
        public String ImageUrl { get; set; }
        public PropertyRequisition PropertyRequisition;

        public RequisitionViewModel()
        {
            this.PropertyRequisition = new PropertyRequisition();
        }
    }
}