using System;
using System.Web.Mvc;

namespace SS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public ActionResult VerifyPayments(int pgNo = 0)
        {
            int pgTake = 16;

            var paymentsVM = Core.PropertyHelper.GetPayments(pgTake, pgNo,null);
            ViewBag.pgNo = pgNo;
            ViewBag.pgTake = pgTake;
            ViewBag.itemsCount = Core.PropertyHelper.GetPaymentsCount(null);
            ViewBag.isAdmin = true;

            return View(paymentsVM);
        }

        [HttpPost]
        public JsonResult VerifyPayment(Guid paymentID)
        {
            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];
                return Json(Core.PropertyHelper.VerifyPayment(paymentID, userId));
            }
            return Json(false);
        }
    }
}