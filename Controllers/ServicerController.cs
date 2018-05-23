using SS.Core;
using SS.Models;
using SS.ViewModels;
using SS.ViewModels.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SS.Controllers
{
    public class ServicerController : Controller
    {
        [HttpGet]
        public JsonResult RequestJsonDataFromUrl(String Url)
        {
            try
            {
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var jsonObject = serializer.DeserializeObject(PropertyHelper.MakeHttpRequest(Url));

                return Json(jsonObject, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.Write(ex.StackTrace);
            }

            return null;
        }

        /// <summary>
        /// Function gets property type name by it's category code name
        /// </summary>
        /// <param name="propertyCategoryName"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetPropertyTypesByCategoryName(String propertyCategoryName)
        {
            IEnumerable<String> results = null;
            /*mapping property category name to property code*/
            String propertyCategoryCode = PropertyHelper.mapPropertyCategoryNameToCode(propertyCategoryName);

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                var unitOfWork = new UnitOfWork(dbCtx);
                results = unitOfWork.PropertyType.GetPropertyTypesByCategoryCode(propertyCategoryCode);
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Function gets property type name by it's category code
        /// </summary>
        /// <param name="propertyCategoryCode"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetPropertyTypesByCategoryCode(String propertyCategoryCode)
        {
            IEnumerable<String> results = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                var unitOfWork = new UnitOfWork(dbCtx);
                results = unitOfWork.PropertyType.GetPropertyTypesByCategoryCode(propertyCategoryCode);
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Retrieve all property types for the properties
        /// </summary>
        /// <returns>JsonResult</returns>
        [HttpGet]
        public JsonResult GetAllPropertyTypeNames()
        {
            IEnumerable<String> results = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                var unitOfWork = new UnitOfWork(dbCtx);
                results = unitOfWork.PropertyType.GetAllPropertyTypeNames();
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Retrieve all property types for the properties
        /// </summary>
        /// <returns>JsonResult</returns>
        [HttpGet]
        public JsonResult GetAllPropertyPurposeNames()
        {
            IEnumerable<String> results = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                var unitOfWork = new UnitOfWork(dbCtx);
                results = unitOfWork.PropertyPurpose.GetAllPurposeNames();
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets tag names by the property category code
        /// </summary>
        /// <param name="propertyCategoryName"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetTagNamesByPropertyCategoryCode(String propertyCategoryName)
        {
            IEnumerable<String> results = null;
            /*mapping property category name to property code*/
            String propertyCategoryCode = PropertyHelper.mapPropertyCategoryNameToCode(propertyCategoryName);

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                var unitOfWork = new UnitOfWork(dbCtx);
                results = unitOfWork.TagType.GetTagNamesByPropertyCategoryCode(propertyCategoryCode);
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetVisitorsCount()
        {
            return Json((int)HttpContext.Application["TotalOnlineUsers"], JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns the _partialMvcCaptcha view
        /// </summary>
        [HttpGet]
        public ActionResult GetMvcCaptchaView()
        {
            return PartialView("_PartialMvcCaptcha");
        }

        /// <summary>
        /// Returns the _partialAdvertiseProperty view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetAdvertisePropertyView()
        {
            Owner owner = null;
            AdvertisePropertyViewModel newModel = null;
            Guid userId;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                if (Session["userId"] != null)
                {
                    userId = (Guid)Session["userId"];

                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    owner = unitOfWork.Owner.GetOwnerByUserID(userId);
                }

                newModel = new AdvertisePropertyViewModel()
                {
                    FirstName = owner.User.FirstName,
                    LastName = owner.User.LastName,
                    CellNum = owner.User.CellNum,
                    Email = owner.User.Email,
                    StreetAddress = "12 Coolshade Drive",
                    Country = "Jamaica",
                    Division = "Kingston 19",
                    Community = "Havendale",
                    Price = 4000,
                    SecurityDeposit = 4000,
                    TermsAgreement = "Terms",
                    TotRooms = 1,
                    IsReviewable = true,
                    Description = "Very good property"
                };
            }
            return PartialView("_partialAdvertiseProperty", newModel);
        }

        /// <summary>
        /// Returns the _partialModalUpdateProperty view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetModalUpdatePropertyView(Guid ID)
        {
            return PartialView("_partialModalUpdateProperty", PropertyHelper.GetUpdatePropertyVM(ID));
        }

        /// <summary>
        /// retrieves the names of the divisions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetDivisionNames()
        {
            return Json(PropertyHelper.GetDivisionNames(), JsonRequestBehavior.AllowGet);
        }
    }
}