using SS.Core;
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
    }
}