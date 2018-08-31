using log4net;
using SS.Models;
using SS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SS.ViewModels.RazorHelperModels
{
    public class PropertyViewHelper
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly PaymentService paymentService;

        public PropertyViewHelper()
        {
            paymentService = new PaymentService();
        }

        public static List<SelectListItem> GenderPreferenceCode = new List<SelectListItem>()
        {
            new SelectListItem{ Text = "Gender Preference", Value="na", Selected = true, Disabled = true},
            new SelectListItem{ Text = "Both", Value="B"},
            new SelectListItem{ Text = "Male", Value="M"},
            new SelectListItem{ Text = "Female", Value="F"}
        };

        public static List<SelectListItem> IsReviewable = new List<SelectListItem>()
        {
            new SelectListItem{ Text = "Allow Reviews", Value="na", Selected = true, Disabled = true},
            new SelectListItem{ Text = "Yes", Value="true"},
            new SelectListItem{ Text = "No", Value="false"}
        };

        /// <summary>
        /// Retrieves the payment methods and populates an selectlistitem
        /// object
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public List<SelectListItem> GetPaymentMethods(UnitOfWork unitOfWork)
        {
            List<SelectListItem> paymentMethodItems = new List<SelectListItem>();

            var methods = paymentService.GetPaymentMethods(unitOfWork);
            SelectListItem defaultItem = new SelectListItem() { Text = "Select Payment Method", Value = "", Selected = true, Disabled = true };
            paymentMethodItems.Add(defaultItem);

            foreach (var method in methods)
            {
                SelectListItem item = new SelectListItem() { Text = method.Name, Value = method.ID };
                paymentMethodItems.Add(item);
            }

            return paymentMethodItems;
        }
    }
}