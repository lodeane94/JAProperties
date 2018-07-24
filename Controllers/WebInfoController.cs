using log4net;
using SS.Core;
using SS.ViewModels.WebInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SS.Controllers
{
    public class WebInfoController : Controller
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult HowTo()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        public ActionResult ContactAdmin(ContactFormViewModel model)
        {
            RequestModel requestModel = new RequestModel();

            if (ModelState.IsValid)
            {
                try
                {
                    MiscellaneousHelper.SendMessageToAdmin(model, requestModel);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
            else
                requestModel.AddErrorMessage("An invalid field was entered");

            TempData["response"] = requestModel;

            return View("Contact");
        }

    }
}