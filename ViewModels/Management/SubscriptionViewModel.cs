using SS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels.Management
{
    public class SubscriptionViewModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public decimal MonthlyCost { get; set; }
        public int Period { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}