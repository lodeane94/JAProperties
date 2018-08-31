using BotDetect.Web.Mvc;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SS.Code;
using SS.Core;
using SS.Models;
using SS.Services;
using SS.SignalR;
using SS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SS.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly PropertyService propertyService;

        public PropertiesController()
        {
            propertyService = new PropertyService();
        }

        public async Task<ActionResult> getProperties(PropertySearchViewModel model)
        {
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var userId = Guid.Empty;

                if (Session["userId"] != null)
                {
                    userId = (Guid)Session["userId"];
                }

                model.PgNo = model.PgNo > 0 ? model.PgNo - 1 : 0;

                if (String.IsNullOrEmpty(model.SearchTerm) && PropertyHelper.CreateFilterList(model, unitOfWork).Count() < 1 && model.Tags == null)
                    return RedirectToAction("/", "home");

                featuredPropertiesSlideViewModelList = await propertyService.PopulatePropertiesViewModel(model, userId, unitOfWork);
            }
            initPropertiesTags(model);
            initPropertiesPgValues(model);

            Session["PropertySearchViewModel"] = model;

            return View(featuredPropertiesSlideViewModelList);
        }

        /// <summary>
        /// set the tag viewbag field with tags associated with the 
        /// returned propertes
        /// </summary>
        /// <param name="model"></param>
        private void initPropertiesTags(PropertySearchViewModel model)
        {
            if (model.Tags == null || (model.Tags != null && model.Tags.Count() == 0))
                ViewBag.tags = setPropertyTags();
            else
                ViewBag.tags = setPropertyTags(model.Tags);
        }

        /// <summary>
        /// initialize the viewbag fields on the getProperties page
        /// </summary>
        /// <param name="model"></param>
        private void initPropertiesPgValues(PropertySearchViewModel model)
        {
            ViewBag.activeNavigation = model.IsStudentAccommodationCat ? "isStudentAccommodationCat" : PropertyHelper.mapPropertyCategoryCodeToName(model.PropertyCategory);
            ViewBag.isStudentAccommodationCat = model.IsStudentAccommodationCat;
            ViewBag.category = PropertyHelper.mapPropertyCategoryCodeToName(model.PropertyCategory);
            ViewBag.runningHeader = getRunningHeader(model);
            ViewBag.searchViewModel = model;
            ViewBag.fetchAmount = model.take;
            ViewBag.pageNumber = model.PgNo + 1;
        }
        /// <summary>
        /// Creates the running header for each properties searched result page
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private String getRunningHeader(PropertySearchViewModel model)
        {
            String header = "Results ";

            if (!String.IsNullOrEmpty(model.SearchTerm))
            {
                if (model.DistanceRadius > 0)
                    header += "for properties that are " + model.DistanceRadius + " KM from " + model.SearchTerm;
                else
                    header += "for the search : " + model.SearchTerm;
            }
            else if (!String.IsNullOrEmpty(model.PropertyCategory))
            {
                if (model.IsStudentAccommodationCat)
                {
                    header += "for properties in the Student Accommodation category";
                }
                else
                {
                    if (EFPConstants.PropertyCategory.RealEstate.Equals(model.PropertyCategory))
                    {
                        header += "for properties in the Real Estate category";
                    }
                    else
                        header += "for properties in the " + PropertyHelper.mapPropertyCategoryCodeToName(model.PropertyCategory) + " category";
                }
            }

            return header;
        }

        public async Task<ActionResult> getNearbyProperties(String distanceMtxInfo, int? pgNo, double? distanceRadius)
        {
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = null;
            var userId = Guid.Empty;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var searchViewModel = (PropertySearchViewModel)Session["PropertySearchViewModel"];

                if (pgNo.HasValue)
                    searchViewModel.PgNo = pgNo.Value;

                if (!distanceRadius.HasValue)
                    searchViewModel.DistanceRadius = 20;
                else
                    searchViewModel.DistanceRadius = distanceRadius.Value;

                if (Session["userId"] != null)
                {
                    userId = (Guid)Session["userId"];
                }

                if (!String.IsNullOrEmpty(distanceMtxInfo) && !distanceMtxInfo.Equals("null"))
                    Session["distanceMtxInfo"] = distanceMtxInfo;
                else if (Session["distanceMtxInfo"] != null)
                    distanceMtxInfo = (String)Session["distanceMtxInfo"];
                else
                    return RedirectToAction("/", "home");

                if (searchViewModel == null)
                    return RedirectToAction("/", "home");

                if (!String.IsNullOrEmpty(distanceMtxInfo))
                {
                    //distance radius is in KM TODO give option to convert to miles
                    //will be used to eliminate properties that are of further distance
                    var model = JsonConvert.DeserializeObject<NearbyPropertySearchViewModel>(distanceMtxInfo);
                    var revisedModel = propertyService.NarrowSearchResultsToDistanceRadius(model, searchViewModel.DistanceRadius);

                    searchViewModel.PgNo = searchViewModel.PgNo > 0 ? searchViewModel.PgNo - 1 : 0;
                    featuredPropertiesSlideViewModelList = await propertyService.PopulatePropertiesViewModel(revisedModel, searchViewModel, userId, unitOfWork);

                    ViewBag.SearchTerm = searchViewModel.SearchTerm;
                    ViewBag.SearchType = searchViewModel.SearchType;
                    ViewBag.DistanceRadius = searchViewModel.DistanceRadius;
                    ViewBag.NearByPropModel = revisedModel;

                    initPropertiesTags(searchViewModel);
                    initPropertiesPgValues(searchViewModel);
                }
            }

            return View("getProperties", featuredPropertiesSlideViewModelList);
        }

        [HttpGet]
        public async Task<JsonResult> GetPropertiesCoordinates(PropertySearchViewModel model)
        {
            Array propertyCoordinates = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                propertyCoordinates = await propertyService.PopulateModelForPropertyCoordinates(model, unitOfWork);

                Session["PropertySearchViewModel"] = model;
            }

            return Json(propertyCoordinates, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> GetPropertiesCounts(PropertySearchViewModel model)
        {
            int count = 0;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                
                count = await propertyService.GetPropertiesCount(model, unitOfWork);
            }

            return Json(count, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> GetNearbyPropertiesCounts(List<NearbyPropertySearchModel> model, PropertySearchViewModel searchViewModel)
        {
            int count = 0;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (model != null)
                {
                    count = await propertyService.GetNearbyPropertiesCount(model, searchViewModel, unitOfWork);
                }
            }

            return Json(count, JsonRequestBehavior.AllowGet); ;
        }

        public ActionResult getProperty(Guid id)
        {
            SelectedPropertyViewModel property = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var searchViewModel = (PropertySearchViewModel)Session["PropertySearchViewModel"];

                ViewBag.searchViewModel = searchViewModel;

                property = propertyService.GetProperty(id, unitOfWork);
            }

            return View(property);
        }

        /// <summary>
        /// returns the tags associated with the search along with
        /// initializing the checked state on these tags
        /// </summary>
        /// <returns></returns>
        private Dictionary<String, Boolean> setPropertyTags()
        {
            var uncheckedTags = new Dictionary<String, Boolean>();
            var tags = propertyService.GetSearchResultPropertyTags();

            foreach (var tag in tags)
            {
                uncheckedTags.Add(tag, false);
            }

            return uncheckedTags;
        }

        /// <summary>
        /// returns the tags associated with the search along with
        /// initializing the checked state on these tags
        /// </summary>
        /// <returns></returns>
        private Dictionary<String, Boolean> setPropertyTags(Dictionary<String, Boolean> checkedTags)
        {
            var checkedPTags = new Dictionary<String, Boolean>();
            var tags = propertyService.GetSearchResultPropertyTags();
            bool isChecked;

            foreach (var tag in tags)
            {
                isChecked = false;

                foreach (var checkT in checkedTags.Keys)
                {
                    if (tag.Equals(checkT))
                    {
                        checkedPTags.Add(tag, true);
                        isChecked = true;
                        break;
                    }
                }

                if (!isChecked)
                    checkedPTags.Add(tag, false);
            }

            return checkedPTags;
        }

        /// <summary>
        /// Allows viewers to request the property or ask a question about the property
        /// </summary>
        /// <param name="request"></param>
        /// <param name="contactPurpose"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [CaptchaValidation("captchaCode", "captcha", "CAPTCHA code is incorrect")]
        public JsonResult RequestProperty(PropertyRequisition request, String contactPurpose)
        {
            ErrorModel errorModel = new ErrorModel();

            if (ModelState.IsValid)
            {
                try
                {
                    using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                    {
                        UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                        if (Session["userId"] != null)
                        {
                            Guid userId = (Guid)Session["userId"];

                            propertyService.ContactPropertyOwner(request, contactPurpose, userId, errorModel, unitOfWork);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
            else
            {
                errorModel = MiscellaneousHelper.PopulateErrorModel(ModelState);
            }

            MvcCaptcha.ResetCaptcha("captcha");//TODO find a way to reload captcha image after returning the response
            return Json(errorModel);
        }

        /// <summary>
        /// Used for saving properties to be viewed on the dashboard/portal
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult SaveLikedProperty(Guid propertyId)
        {
            string result = "False";

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];
                    result = propertyService.SaveLikedProperty(userId, propertyId, unitOfWork).ToString();
                }
                else
                    result = "Sign in to add property to your liked list";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}