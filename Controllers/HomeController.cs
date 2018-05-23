using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SS.Models;
using SS.Code;
using System.Collections;
using System.Web.Security;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using Microsoft.CSharp.RuntimeBinder;
using SS.ViewModels;
using System.Net;
using System.IO;
using SS.Core;
using log4net;

namespace SS.Controllers
{
    public class HomeController : Controller
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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