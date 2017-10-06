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
        private string filterString;

        public ActionResult Home()
        {
            int take = 4;//amount of featured properties to be retieved per category
            int slideTake = 5;
            int slideTakeCount = 0;//used to determine whether to get retrieve multiple images or one
            FeaturedPropertiesSlideViewModel featuredPropertiesSlideViewModel;
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = new List<FeaturedPropertiesSlideViewModel>();
            IEnumerable<Property> properties;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                properties = unitOfWork.Property.GetFeaturedProperties(take);

                foreach (var property in properties)
                {
                    slideTakeCount++;

                    IEnumerable<int> avgPropRatings = unitOfWork.PropertyRating.GetPropertyRatingsByPropertyId(property.ID);

                    featuredPropertiesSlideViewModel = new FeaturedPropertiesSlideViewModel()
                    {
                        property = property,
                        propertyImageURLs = slideTakeCount <= slideTake ? unitOfWork.PropertyImage.GetImageURLsByPropertyId(property.ID, slideTake) : null,
                        propertyPrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(property.ID),
                        averageRating = avgPropRatings.Count() > 0 ? (int)avgPropRatings.Average() : 0
                    };

                    featuredPropertiesSlideViewModelList.Add(featuredPropertiesSlideViewModel);
                }

            };

            return View(featuredPropertiesSlideViewModelList);
        }
        /// <summary>
        /// funtion returns all properties on the home page depending on the type of request was made
        /// </summary>
        /// <param name="fetchAmount"></param>
        /// <param name="parish"></param>
        /// <param name="isPagination"></param>
        /// <param name="propertyType"></param>
        /// <param name="pgNo"></param>
        /// <returns></returns>
        /*
        public JsonResult getProperties(short fetchAmount, string parish, bool isPagination, string propertyType, int pgNo)
        {
            ArrayList propertiesList = new ArrayList();
            PropertiesInformation propertiesInformation = new PropertiesInformation();
            try
            {
                if (isPagination)
                {
                    propertiesInformation = HomeDAO.getHomePropertiesPagination(fetchAmount, parish, propertyType, pgNo);
                }
                else
                    propertiesInformation = HomeDAO.getHomeProperties(fetchAmount, parish);
                //adding properties fir view
                propertiesList.Add(propertiesInformation.accommodationList);
                propertiesList.Add(propertiesInformation.houseList);
                propertiesList.Add(propertiesInformation.landList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Json(propertiesList, JsonRequestBehavior.AllowGet);
        }*/
    }

}