using System;
using System.Web.Mvc;
using SS.Core;
using log4net;
using System.Threading.Tasks;

namespace SS.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Home()
        {
            var userId = Guid.Empty;

            if (Session["userId"] != null)
            {
                userId = (Guid)Session["userId"];
            }

            int take = 8; //amount of featured properties to be retieved per category
            
            return View(await PropertyHelper.PopulateHomePageViewModel(take, userId));
        }        
        
    }

}