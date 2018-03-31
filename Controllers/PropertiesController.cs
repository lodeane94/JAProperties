using BotDetect.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SS.Code;
using SS.Core;
using SS.Models;
using SS.SignalR;
using SS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace SS.Controllers
{
    public class PropertiesController : Controller
    {
        private string filterString = string.Empty;
        private string conditionToBeRemoved = string.Empty;

        //TODO get function to reload user id upon restart of application
        /* PropertiesController()
         {
             var userId = MiscellaneousHelper.getLoggedInUser();

             if (!userId.Equals(new Guid()))
             {
                 Session["userId"] = userId;
             }
         }*/

        public ActionResult getProperties(PropertySearchViewModel model)
        {
            var userId = Guid.Empty;
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = null;

            if (Session["userId"] != null)
            {
                userId = (Guid)Session["userId"];
            }

            model.PgNo = model.PgNo > 0 ? model.PgNo - 1 : 0;

            if (String.IsNullOrEmpty(model.SearchTerm) && PropertyHelper.createFilterList(model).Count() < 1 && model.Tags == null)
                return RedirectToAction("/", "home");

            featuredPropertiesSlideViewModelList = PropertyHelper.PopulatePropertiesViewModel(model, userId);

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
            String header = "Showing results ";

            if (!String.IsNullOrEmpty(model.SearchTerm))
            {
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

        public ActionResult getNearbyProperties(String distanceMtxInfo, double distanceRadius = 20.0)
        {
            var userId = Guid.Empty;
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = null;
            var searchViewModel = (PropertySearchViewModel)Session["PropertySearchViewModel"];

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
                var revisedModel = PropertyHelper.NarrowSearchResultsToDistanceRadius(model, distanceRadius);

                searchViewModel.PgNo = searchViewModel.PgNo > 0 ? searchViewModel.PgNo - 1 : 0;
                featuredPropertiesSlideViewModelList = PropertyHelper.PopulatePropertiesViewModel(revisedModel, searchViewModel, userId);

                ViewBag.SearchTerm = searchViewModel.SearchTerm;
                ViewBag.SearchType = searchViewModel.SearchType;
                ViewBag.DistanceRadius = distanceRadius;

                initPropertiesTags(searchViewModel);
                initPropertiesPgValues(searchViewModel);
            }

            return View("getProperties", featuredPropertiesSlideViewModelList);
        }

        [HttpGet]
        public JsonResult GetPropertiesCoordinates(PropertySearchViewModel model)
        {
            Array propertyCoordinates = PropertyHelper.PopulateModelForPropertyCoordinates(model);

            Session["PropertySearchViewModel"] = model;

            return Json(propertyCoordinates, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getProperty(Guid id)
        {
            var searchViewModel = (PropertySearchViewModel)Session["PropertySearchViewModel"];

            ViewBag.searchViewModel = searchViewModel;

            return View(PropertyHelper.GetProperty(id));
        }

        /// <summary>
        /// returns the tags associated with the search along with
        /// initializing the checked state on these tags
        /// </summary>
        /// <returns></returns>
        private Dictionary<String, Boolean> setPropertyTags()
        {
            var uncheckedTags = new Dictionary<String, Boolean>();
            var tags = PropertyHelper.GetSearchResultPropertyTags();

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
            var tags = PropertyHelper.GetSearchResultPropertyTags();
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
        /*
         * makes requisition for the property that the user selected if they wanted to use the system*/

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
                        if (Session["userId"] != null)
                        {
                            UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                            Guid userId = (Guid)Session["userId"];

                            if (contactPurpose.Equals("requisition"))
                            {
                                PropertyRequisition requisition = new PropertyRequisition()
                                {
                                    ID = Guid.NewGuid(),
                                    UserID = userId,
                                    PropertyID = request.PropertyID,
                                    Msg = request.Msg,
                                    IsAccepted = false,
                                    ExpiryDate = DateTime.Now.AddDays(7),//requisition should last for a week
                                    DateTCreated = DateTime.Now
                                };

                                unitOfWork.PropertyRequisition.Add(requisition);
                                unitOfWork.save();

                                var userTo = unitOfWork.Property.GetPropertyOwnerByPropID(request.PropertyID).User;
                                DashboardHub.alertRequisition(userTo.Email);
                            }
                            else
                            {
                                var userTo = unitOfWork.Property.GetPropertyOwnerByPropID(request.PropertyID).User;

                                Message message = new Message()
                                {
                                    ID = Guid.NewGuid(),
                                    To = userTo.ID,
                                    From = userId,
                                    Msg = request.Msg,
                                    Seen = false,
                                    DateTCreated = DateTime.Now
                                };

                                unitOfWork.Message.Add(message);
                                unitOfWork.save();

                                DashboardHub.BroadcastUserMessages(userTo.Email);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorModel = MiscellaneousHelper.PopulateErrorModel(ModelState);
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

            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];
                result = PropertyHelper.SaveLikedProperty(userId, propertyId).ToString();
            }
            else
                result = "Sign in to add property to your liked list";

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}