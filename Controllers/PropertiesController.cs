using BotDetect.Web.Mvc;
using SS.Code;
using SS.Core;
using SS.Models;
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
            model.take = 1;
            model.PgNo = model.PgNo > 0 ? model.PgNo - 1 : 0;//this is done to since page number should start at 0

            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = null;
            IEnumerable<Property> filteredProperties = null;
            IEnumerable<Property> searchTermProperties = null;
            IEnumerable<Property> properties = null;

            IEnumerable<Property> filteredPropertiesCount = null;
            IEnumerable<Property> searchTermPropertiesCount = null;
            int propertiesCount = 0;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                List<Core.Filter> filters = createFilterList(model, unitOfWork);
                var deleg = ExpressionBuilder.GetExpression<Property>(filters);

                filteredProperties = unitOfWork.Property.FindProperties(deleg, model.take, model.PgNo);
                searchTermProperties = unitOfWork.Property.FindPropertiesBySearchTerm(model.SearchTerm, model.take, model.PgNo);
                properties = filteredProperties.Concat(searchTermProperties).Distinct();
                //TODO find a more efficient way to get count of total properties
                filteredPropertiesCount = unitOfWork.Property.FindProperties(deleg);
                searchTermPropertiesCount = unitOfWork.Property.FindPropertiesBySearchTerm(model.SearchTerm);
                propertiesCount = filteredPropertiesCount.Concat(searchTermPropertiesCount).Distinct().Count();

                featuredPropertiesSlideViewModelList = PropertyHelper.PopulatePropertiesViewModel(properties, unitOfWork, "Properties");
            }

            ViewBag.activeNavigation = PropertyHelper.mapPropertyCategoryCodeToName(model.PropertyCategory);
            ViewBag.searchViewModel = model;
            ViewBag.totalItemsFound = propertiesCount;
            ViewBag.fetchAmount = model.take;
            ViewBag.pageNumber = model.PgNo + 1;

            return View(featuredPropertiesSlideViewModelList);
        }

        public ActionResult getProperty(Guid id)
        {
            FeaturedPropertiesSlideViewModel propertyInformation = null;

            //  using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            //  {
            EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities();

            UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

            propertyInformation = new FeaturedPropertiesSlideViewModel();

            propertyInformation.property = unitOfWork.Property.Get(id);
            propertyInformation.owner = unitOfWork.Owner.Get(propertyInformation.property.OwnerID);
            propertyInformation.tags = unitOfWork.Tags.GetTagNamesByPropertyId(id);
            propertyInformation.propertyImageURLs = unitOfWork.PropertyImage.GetImageURLsByPropertyId(id, 0);
            propertyInformation.propertyPrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(id);

            propertyInformation.propRatings = unitOfWork.PropertyRating.GetPropertyRatingsByPropertyId(id);
            propertyInformation.averageRating = propertyInformation.propRatings.Count() > 0 ? (int)propertyInformation.propRatings.Select(x => x.Ratings).Average() : 0;

            return View(propertyInformation);
            //  }

        }

        /// <summary>
        /// Create a filter list to be used for property searching purposes
        /// </summary>
        /// <param name="model"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        private List<Core.Filter> createFilterList(PropertySearchViewModel model, UnitOfWork unitOfWork)
        {
            List<Core.Filter> filters = new List<Core.Filter>();

            //country
            if (!string.IsNullOrEmpty(model.Country))
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "Country",
                    Operation = Op.Equals,
                    Value = model.Country
                };

                filters.Add(filter);
            }

            //division
            if (!string.IsNullOrEmpty(model.Division))
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "Division",
                    Operation = Op.Equals,
                    Value = model.Division
                };

                filters.Add(filter);
            }

            //property category
            if (!string.IsNullOrEmpty(model.PropertyCategory))
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "CategoryCode",
                    Operation = Op.Equals,
                    Value = model.PropertyCategory
                };

                filters.Add(filter);
            }

            //property type
            if (!string.IsNullOrEmpty(model.PropertyType))
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "TypeID",
                    Operation = Op.Equals,
                    Value = unitOfWork.PropertyType.GetPropertyTypeIDByName(model.PropertyType)
                };

                filters.Add(filter);
            }

            //property purpose
            if (!string.IsNullOrEmpty(model.PropertyPurpose))
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "PurposeCode",
                    Operation = Op.Equals,
                    Value = PropertyHelper.mapPropertyPurposeNameToCode(model.PropertyPurpose)
                };

                filters.Add(filter);
            }

            //price range
            if (model.MinPrice >= 0 && model.MaxPrice > 0)
            {
                Core.Filter filter1 = new Core.Filter()
                {
                    PropertyName = "Price",
                    Operation = Op.GreaterThanOrEqual,
                    Value = model.MinPrice
                };

                Core.Filter filter2 = new Core.Filter()
                {
                    PropertyName = "price",
                    Operation = Op.LessThanOrEqual,
                    Value = model.MaxPrice
                };
                filters.Add(filter1);
                filters.Add(filter2);
            }
            ///////////////TODO implement Or conditions for these///////////////////////
            //ad type sale
            if (model.ChkBuyProperty)
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "AdTypeCode",
                    Operation = Op.Equals,
                    Value = EFPConstants.PropertyAdType.Sale
                };

                filters.Add(filter);
            }

            //ad type rent
            if (model.ChkRentProperty)
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "AdTypeCode",
                    Operation = Op.Equals,
                    Value = EFPConstants.PropertyAdType.Rent
                };

                filters.Add(filter);
            }

            //ad type lease
            if (model.ChkLeasedProperty)
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "AdTypeCode",
                    Operation = Op.Equals,
                    Value = EFPConstants.PropertyAdType.Lease
                };

                filters.Add(filter);
            }

            return filters;
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
                            }
                            else
                            {
                                Message message = new Message()
                                {
                                    ID = Guid.NewGuid(),
                                    To = unitOfWork.Property.GetPropertyOwnerByPropID(request.PropertyID).User.ID,
                                    From = userId,
                                    Msg = request.Msg,
                                    Seen = false,
                                    DateTCreated = DateTime.Now
                                };

                                unitOfWork.Message.Add(message);
                            }

                            unitOfWork.save();
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
        }/*
        //sends mail
        public bool sendMail(string emailTo, string body, string subject)
        {
            MailModel mailModel = new MailModel()
            {
                To = emailTo,
                Subject = subject,
                From = "jamprops@hotmail.com",
                Body = body
            };

            //setting mail requirements
            MailMessage mail = new MailMessage();
            mail.To.Add(mailModel.To);
            mail.From = new MailAddress(mailModel.From);
            mail.Subject = mailModel.Subject;
            mail.Body = mailModel.Body;
            mail.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp-mail.outlook.com";
            smtp.Port = 587;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential("jamprops@hotmail.com", "Daveyot88*");
            smtp.EnableSsl = true;
            smtp.Send(mail);

            return true;
        }
        /*
         * retrieves the information for the property that the user selected 
         
        public JsonResult RetrieveSelectedAccommodation(Guid property_id)
        {
            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();

            List<AccommodationModel> AInfo = new List<AccommodationModel>();

            var accommodations = dbCtx.ACCOMMODATIONS.Where(a => a.ID == property_id).Select(a => new { a.ID, a.HOUSE_BATHROOM_AMOUNT, a.IMAGE_URL, a.WATER, a.TERMS_AGREEMENT, a.AVAILABILITY, a.INTERNET, a.OCCUPANCY, a.PRICE, a.SECURITY_DEPOSIT, a.CABLE, a.DESCRIPTION, a.ELECTRICITY, a.GAS, a.GENDER_PREFERENCE, a.STREET_ADDRESS, a.PARISH, a.CITY });

            var propertyOwner = dbCtx.ACCOMMODATIONS.Where(a => a.ID == property_id).Select(a => new { a.LANDLORDS.ID, a.LANDLORDS.FIRST_NAME, a.LANDLORDS.LAST_NAME, a.LANDLORDS.EMAIL, a.LANDLORDS.CELL, a.LANDLORDS.GENDER });

            AccommodationModel accommodationModel = new AccommodationModel();

            foreach (var accommodation in accommodations)
            {
                accommodationModel.ID = accommodation.ID.ToString();
                accommodationModel.Availability = accommodation.AVAILABILITY == true ? "Yes" : "No";
                accommodationModel.Cable = accommodation.CABLE == true ? "Yes" : "No";
                accommodationModel.Description = accommodation.DESCRIPTION;
                accommodationModel.Electricity = accommodation.ELECTRICITY == true ? "Yes" : "No";
                accommodationModel.Gas = accommodation.GAS == true ? "Yes" : "No";
                accommodationModel.Internet = accommodation.INTERNET == true ? "Yes" : "No";
                accommodationModel.Occupancy = accommodation.OCCUPANCY.ToString();
                accommodationModel.Price = "$" + accommodation.PRICE.ToString() + " JMD";
                accommodationModel.TermsAgreement = accommodation.TERMS_AGREEMENT;
                accommodationModel.SecurityDeposit = "$" + accommodation.SECURITY_DEPOSIT.ToString() + " JMD";
                accommodationModel.Water = accommodation.WATER == true ? "Yes" : "No";
                accommodationModel.ImageURL = accommodation.IMAGE_URL;
                accommodationModel.BathroomAmount = accommodation.HOUSE_BATHROOM_AMOUNT.ToString();
                accommodationModel.StreetAddress = accommodation.STREET_ADDRESS;
                accommodationModel.City = accommodation.CITY;
                accommodationModel.Parish = accommodation.PARISH;

                switch (accommodation.GENDER_PREFERENCE)
                {
                    case "M":
                        accommodationModel.GenderPreference = "Males Only";
                        break;
                    case "F":
                        accommodationModel.GenderPreference = "Females Only";
                        break;
                    case "B":
                        accommodationModel.GenderPreference = "Both Genders";
                        break;
                }
            }

            foreach (var owner in propertyOwner)
            {
                accommodationModel.ownerModel.ID = owner.ID.ToString();
                accommodationModel.ownerModel.FirstName = owner.FIRST_NAME;
                accommodationModel.ownerModel.LastName = owner.LAST_NAME;
                accommodationModel.ownerModel.Gender = owner.GENDER == "M" ? "Male" : "Female";
                accommodationModel.ownerModel.Email = owner.EMAIL;
                accommodationModel.ownerModel.Cell = owner.CELL;
            }

            AInfo.Add(accommodationModel);

            return Json(AInfo, JsonRequestBehavior.AllowGet);
        }
        /*
         * retrieves the information for the property that the user selected 
         
        public JsonResult RetrieveSelectedHouse(Guid property_id)
        {
            List<HouseModel> HInfo = new List<HouseModel>();

            using (JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities())
            {
                try
                {

                    var houses = dbCtx.HOUSE.Where(h => h.ID == property_id).Select(h => new { h.ID, h.STREET_ADDRESS, h.PARISH, h.CITY, h.IMAGE_URL, h.ISFURNISHED, h.PRICE, h.PURPOSE, h.BATH_ROOM_AMOUNT, h.BED_ROOM_AMOUNT, h.DESCRIPTION });

                    var propertyOwner = dbCtx.HOUSE.Where(a => a.ID == property_id).Select(a => new { a.LANDLORDS.ID, a.LANDLORDS.FIRST_NAME, a.LANDLORDS.LAST_NAME, a.LANDLORDS.EMAIL, a.LANDLORDS.CELL, a.LANDLORDS.GENDER });

                    HouseModel houseModel = new HouseModel();

                    foreach (var house in houses)
                    {
                        houseModel.ID = house.ID.ToString();
                        houseModel.BathroomAmount = house.BATH_ROOM_AMOUNT;
                        houseModel.BedroomAmount = house.BED_ROOM_AMOUNT;
                        houseModel.Price = "$" + house.PRICE.ToString() + " JMD";
                        houseModel.isFurnished = house.ISFURNISHED ? "Yes" : "No";
                        houseModel.Purpose = house.PURPOSE;
                        houseModel.Description = house.DESCRIPTION;
                        houseModel.ImageURL = house.IMAGE_URL;
                        houseModel.StreetAddress = house.STREET_ADDRESS;
                        houseModel.City = house.CITY;
                        houseModel.Parish = house.PARISH;
                    }

                    foreach (var owner in propertyOwner)
                    {
                        houseModel.ownerModel.ID = owner.ID.ToString();
                        houseModel.ownerModel.FirstName = owner.FIRST_NAME;
                        houseModel.ownerModel.LastName = owner.LAST_NAME;
                        houseModel.ownerModel.Gender = owner.GENDER == "M" ? "Male" : "Female";
                        houseModel.ownerModel.Email = owner.EMAIL;
                        houseModel.ownerModel.Cell = owner.CELL;
                    }

                    HInfo.Add(houseModel);
                }
                catch (Exception ex) { }
            }

            return Json(HInfo, JsonRequestBehavior.AllowGet);
        }
        /*
         * retrieves the information for the property that the user selected 
         
        public JsonResult RetrieveSelectedLand(Guid property_id)
        {
            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();

            List<LandModel> LInfo = new List<LandModel>();

            var lands = dbCtx.LAND.Where(l => l.ID == property_id).Select(l => new { l.ID, l.DESCRIPTION, l.STREET_ADDRESS, l.PARISH, l.CITY, l.IMAGE_URL, l.PRICE, l.PURPOSE, l.AREA });

            var propertyOwner = dbCtx.LAND.Where(a => a.ID == property_id).Select(a => new { a.LANDLORDS.ID, a.LANDLORDS.FIRST_NAME, a.LANDLORDS.LAST_NAME, a.LANDLORDS.EMAIL, a.LANDLORDS.CELL, a.LANDLORDS.GENDER });

            LandModel landModel = new LandModel();

            foreach (var land in lands)
            {
                landModel.ID = land.ID.ToString();
                landModel.Area = land.AREA.ToString() + " Acre";
                landModel.Price = "$" + land.PRICE.ToString() + " JMD";
                landModel.Purpose = land.PURPOSE;
                landModel.ImageURL = land.IMAGE_URL;
                landModel.StreetAddress = land.STREET_ADDRESS;
                landModel.City = land.CITY;
                landModel.Parish = land.PARISH;
                landModel.Description = land.DESCRIPTION;
            }

            foreach (var owner in propertyOwner)
            {
                landModel.ownerModel.ID = owner.ID.ToString();
                landModel.ownerModel.FirstName = owner.FIRST_NAME;
                landModel.ownerModel.LastName = owner.LAST_NAME;
                landModel.ownerModel.Gender = owner.GENDER == "M" ? "Male" : "Female";
                landModel.ownerModel.Email = owner.EMAIL;
                landModel.ownerModel.Cell = owner.CELL;
            }

            LInfo.Add(landModel);

            return Json(LInfo, JsonRequestBehavior.AllowGet);
        }*/
    }

}