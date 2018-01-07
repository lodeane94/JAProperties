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

namespace SS.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Home()
        {
            int take = 8; //amount of featured properties to be retieved per category

            return View(PropertyHelper.PopulatePropertiesViewModel(take));
        }        
        
    }

}