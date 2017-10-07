﻿using SS.Core;
using SS.Models;
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
        /// Function gets property type name by it's category code
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
    }
}