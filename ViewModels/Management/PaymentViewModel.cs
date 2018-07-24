using SS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels.Management
{
    public class PaymentViewModel
    {
        public Guid ID { get; set; }
        public Guid SubscriptionID { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentMethodID { get; set; }
        public string Email { get; set; }
        public int Period { get; set; }
        public decimal Amount { get; set; }
        public string VoucherNumber { get; set; }
        public bool IsExtension { get; set; } = false;
        public bool IsVerified { get; set; }
        public DateTime DateTCreated { get; set; }
        public DateTime DateTModified { get; set; }
    }
}