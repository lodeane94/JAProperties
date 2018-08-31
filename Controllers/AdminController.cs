using log4net;
using SS.Models;
using SS.Services;
using SS.ViewModels.Management;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly PaymentService paymentService;

        public AdminController()
        {
            paymentService = new PaymentService();
        }

        public ActionResult VerifyPayments(int pgNo = 0)
        {
            IEnumerable<PaymentViewModel> paymentsVM = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                int pgTake = 16;

                paymentsVM = paymentService.GetPayments(pgTake, pgNo, null, unitOfWork);
                ViewBag.pgNo = pgNo;
                ViewBag.pgTake = pgTake;
                ViewBag.itemsCount = paymentService.GetPaymentsCount(null, unitOfWork);
                ViewBag.isAdmin = true;
            }

            return View(paymentsVM);
        }

        [HttpPost]
        public JsonResult VerifyPayment(Guid paymentID)
        {
            Core.ErrorModel errorModel;

            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];

                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    errorModel = paymentService.VerifyPayment(paymentID, userId, unitOfWork);
                }

                return Json(errorModel);
            }

            errorModel = new Core.ErrorModel();
            errorModel.AddErrorMessage("Session timed out. Please repeat signin process");

            return Json(errorModel);
        }
    }
}