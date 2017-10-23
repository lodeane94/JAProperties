using SS.Core;
using SS.Models;
using SS.ViewModels;
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
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var jsonObject = serializer.DeserializeObject(PropertyHelper.MakeHttpRequest(Url));

            return Json(jsonObject, JsonRequestBehavior.AllowGet);
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
        /// <summary>
        /// Returns the _partialMvcCaptcha view
        /// </summary>
        public ActionResult GetMvcCaptchaView()
        {
            return PartialView("_PartialMvcCaptcha");
        }

        /// <summary>
        /// Returns the _partialAdvertiseProperty view
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAdvertisePropertyView()
        {
            AdvertisePropertyViewModel Newmodel = new AdvertisePropertyViewModel()
            {
                FirstName = "Lodeane",
                LastName = "Kelly",
                CellNum = "3912600",
                Email = "dean@g.com",
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

            return PartialView("_partialAdvertiseProperty", Newmodel);
        }
       
    }
}