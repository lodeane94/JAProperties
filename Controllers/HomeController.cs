using System;
using System.Web.Mvc;
using SS.Core;
using log4net;
using System.Threading.Tasks;
using SS.Services;
using SS.Models;

namespace SS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly PropertyService propertyService;

        public HomeController()
        {
            propertyService = new PropertyService();
        }

        public async Task<ActionResult> Home()
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var userId = Guid.Empty;

                if (Session["userId"] != null)
                {
                    userId = (Guid)Session["userId"];
                }

                int take = 8; //amount of featured properties to be retieved per category

                return View(await propertyService.PopulateHomePageViewModel(take, userId, unitOfWork));
            }
        }        
        
    }

}