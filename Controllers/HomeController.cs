using System;
using System.Web.Mvc;
using SS.Core;
using log4net;

namespace SS.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Home()
        {
            var userId = Guid.Empty;

            if (Session["userId"] != null)
            {
                userId = (Guid)Session["userId"];
            }

            int take = 8; //amount of featured properties to be retieved per category
            
            return View(PropertyHelper.PopulateHomePageViewModel(take, userId));
        }        
        
    }

}