using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SS.Models;
using System.IO;
using System.Web.Security;
using SS.Code;
using System.Globalization;
using System.Text;
using SS.Core;
using SS.ViewModels;

namespace SS.Controllers
{
    public class AccountsController : Controller
    {
        private List<String> uploadedImageNames = new List<string>();//used to store names of uploaded images. Needed in the case of removing uploaded images during rollback

        //action that signout users of the system
        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Home", "Home");
        }
        public ActionResult SignIn()
        {
            if (!string.IsNullOrEmpty(HttpContext.User.Identity.Name))
            {
                return RedirectToAction("Dashboard", "LandlordManagement");
            }

            return View();
        }
        //action that signin user into the system
        [HttpPost]
        public ActionResult SignIn(string username, string password)
        {
            try
            {
                //validating user using the contact number of that person
                if (Membership.ValidateUser(username, password))
                {
                    FormsAuthentication.SetAuthCookie(username, true);

                    return RedirectToAction("Dashboard", "LandlordManagement");
                }
                else
                    throw new Exception("Incorrect cellphone number or password combination was inserted.");
            }
            catch (Exception ex)
            {
                Session["invalidUser"] = ex.Message;
            }
            return View();
        }

        public ActionResult SignUp()
        {
            return View();
        }

        /// <summary>
        /// Signs up regular user in order for them to make property requisition and comments
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="confirmPassword"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SignUp(User user, String password, String confirmPassword)
        {
            ErrorModel errorModel = new ErrorModel();

            if (ModelState.IsValid)
            {
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    using (var dbCtxTran = dbCtx.Database.BeginTransaction())
                    {
                        try
                        {
                            if (!password.Equals(confirmPassword))
                                throw new Exception("The fields Password and Confirm Password are not equal");

                            PropertyHelper.createRolesIfNotExist();

                            var unitOfWork = new UnitOfWork(dbCtx);

                            var doesUserExist = unitOfWork.User.DoesUserExist(user.Email);

                            if (!doesUserExist)
                            {
                                user = PropertyHelper.createUser(unitOfWork, EFPConstants.UserType.Consumer, "", user.Email, user.FirstName,
                                       user.LastName, user.CellNum, DateTime.MinValue);

                                PropertyHelper.createUserAccount(unitOfWork, user.Email, password);
                            }

                            else
                                throw new Exception("This email address already exists");

                            unitOfWork.save();
                            dbCtxTran.Commit();
                        }
                        catch (Exception ex)
                        {
                            dbCtxTran.Rollback();

                            errorModel.hasErrors = true;
                            errorModel.ErrorMessages = new List<string>();
                            errorModel.ErrorMessages.Add(ex.Message);
                        }
                    }
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

                errorModel.hasErrors = true;
                errorModel.ErrorMessages = new List<string>();
                errorModel.ErrorMessages.AddRange(errors);
            }

            return Json(errorModel);
        }

        //loads advertise property view
        public ActionResult AdvertiseProperty()
        {
            if (!String.IsNullOrEmpty(HttpContext.User.Identity.Name))
            {
                Session["userSignedInCheck"] = "Click add property to add a new property";

                return RedirectToAction("Dashboard", "LandlordManagement");
            }

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

            return View(Newmodel);
        }

        [HttpPost]
        public JsonResult AdvertiseProperty(AdvertisePropertyViewModel model)
        {
            ErrorModel errorModel = new ErrorModel();

            if (ModelState.IsValid)
            {
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    using (var dbCtxTran = dbCtx.Database.BeginTransaction())
                    {
                        try
                        {
                            if (!model.Password.Equals(model.ConfirmPassword))
                                throw new Exception("The fields Password and Confirm Password are not equal");

                            PropertyHelper.createRolesIfNotExist();

                            var unitOfWork = new UnitOfWork(dbCtx);

                            var doesUserExist = unitOfWork.User.DoesUserExist(model.Email);

                            var user = doesUserExist ? unitOfWork.User.GetUserByEmail(model.Email) : null;
                            //if user already exists and they are not a property owner, then associate user with that user type as well
                            //TODO: user's role will have to be manipulated as well
                            if (user != null)
                            {
                                var userTypes = unitOfWork.UserTypeAssoc.GetUserTypesByUserID(user.ID);
                                bool isUserPropOwner = PropertyHelper.isUserOfType(userTypes, EFPConstants.UserType.PropertyOwner);

                                if (!isUserPropOwner)
                                    PropertyHelper.associateUserWithUserType(unitOfWork, user.ID, EFPConstants.UserType.PropertyOwner);
                            }
                            else
                            {
                                user = PropertyHelper.createUser(unitOfWork, EFPConstants.UserType.PropertyOwner, model.SubscriptionType, model.Email, model.FirstName,
                                model.LastName, model.CellNum, DateTime.MinValue);

                                PropertyHelper.createUserAccount(unitOfWork, model.Email, model.Password);
                            }


                            insertProperty(model, unitOfWork, user);

                            unitOfWork.save();
                            dbCtxTran.Commit();
                        }
                        catch (Exception ex)
                        {
                            dbCtxTran.Rollback();

                            if (uploadedImageNames != null && uploadedImageNames.Count > 0)
                            {
                                PropertyHelper.removeUploadedImages(uploadedImageNames);
                            }

                            errorModel.hasErrors = true;
                            errorModel.ErrorMessages = new List<string>();
                            errorModel.ErrorMessages.Add(ex.Message);
                        }
                    }
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

                errorModel.hasErrors = true;
                errorModel.ErrorMessages = new List<string>();
                errorModel.ErrorMessages.AddRange(errors);
            }

            return Json(errorModel);
        }

        /// <summary>
        /// Inserts the property along with it's owner, subscription period and also the property images
        /// </summary>
        /// <param name="model"></param>
        private void insertProperty(AdvertisePropertyViewModel model, UnitOfWork unitOfWork, User user)
        {
            bool doesOwnerExist = user != null && user.Owner.Select(x => x.ID).Count() > 0 ? true : false;
            Guid ownerID = doesOwnerExist ? user.Owner.Select(x => x.ID).Single() : Guid.NewGuid();

            Guid propertyID = Guid.NewGuid();
            String lat = String.Empty;
            String lng = String.Empty;

            //generate enrolment key for users with Landlord subscription
            if (model.SubscriptionType.Equals(nameof(EFPConstants.PropertySubscriptionType.Landlord)))
            {
                model.EnrolmentKey = getRandomKey(6);
            }

            //Coordinate priority : streetaddress then community
            if (!String.IsNullOrEmpty(model.saCoordinateLat) && !String.IsNullOrEmpty(model.saCoordinateLng))
            {
                lat = model.saCoordinateLat;
                lng = model.saCoordinateLng;
            }
            else if (!String.IsNullOrEmpty(model.cCoordinateLat) && !String.IsNullOrEmpty(model.cCoordinateLng))
            {
                //check if lat and lng is already set; if they are then dont using community
                if (string.IsNullOrEmpty(lat) && string.IsNullOrEmpty(lng))
                {
                    lat = model.saCoordinateLat;
                    lng = model.saCoordinateLng;
                }
            }

            Property property = new Property()
            {
                ID = propertyID,
                Title = model.Title,
                OwnerID = ownerID,
                PurposeCode = PropertyHelper.mapPropertyPurposeNameToCode(model.PropertyPurpose),
                TypeID = unitOfWork.PropertyType.GetPropertyTypeIDByName(model.PropertyType),
                AdTypeCode = PropertyHelper.mapPropertyAdTypeNameToCode(model.AdvertismentType),
                AdPriorityCode = PropertyHelper.mapPropertyAdpriorityNameToCode(model.AdvertismentPriority),
                ConditionCode = EFPConstants.PropertyCondition.NotSurveyed,
                CategoryCode = unitOfWork.PropertyType.GetPopertyTypeCategoryCodeByName(model.PropertyType),
                StreetAddress = model.StreetAddress,
                Division = model.Division,
                Community = model.Community,
                NearByEstablishment = model.NearBy,
                Country = model.Country,
                Latitude = lat,
                Longitude = lng,
                NearByEstablishmentLat = model.nearByCoordinateLat,
                NearByEstablishmentLng = model.nearByCoordinateLng,
                Price = model.Price,
                SecurityDeposit = model.SecurityDeposit,
                Occupancy = model.Occupancy,
                GenderPreferenceCode = model.GenderPreferenceCode,
                Description = model.Description,
                Availability = true,
                EnrolmentKey = model.EnrolmentKey,
                TermsAgreement = model.TermsAgreement,
                TotAvailableBathroom = model.TotAvailableBathroom,
                TotRooms = model.TotRooms,
                Area = model.Area,
                IsReviewable = model.IsReviewable,
                DateTCreated = DateTime.Now
            };

            Subscription subscription = new Subscription()
            {
                ID = Guid.NewGuid(),
                OwnerID = ownerID,
                TypeCode = PropertyHelper.mapPropertySubscriptionTypeToCode(model.SubscriptionType),
                Period = model.SubscriptionPeriod,
                DateTCreated = DateTime.Now
            };

            if (!doesOwnerExist)
            {
                Guid guid = Guid.NewGuid();
                String fileName = String.Empty;

                if (model.organizationLogo != null)
                {
                    fileName = guid.ToString() + Path.GetExtension(model.organizationLogo.FileName);
                    uploadPropertyImages(model.organizationLogo, fileName);
                }

                Owner owner = new Owner()
                {
                    ID = ownerID,
                    UserID = user.ID,
                    Organization = model.Organization,
                    LogoUrl = fileName,
                    DateTCreated = DateTime.Now
                };

                unitOfWork.Owner.Add(owner);
            }
            unitOfWork.Property.Add(property);
            unitOfWork.Subscription.Add(subscription);

            associateTagsWithProperty(unitOfWork, propertyID, model.selectedTags);
            associateImagesWithProperty(unitOfWork, model.flPropertyPics, propertyID);
        }

        /// <summary>
        /// Associates a property with the selected tags
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="propertyID"></param>
        /// <param name="selectedTags"></param>
        private void associateTagsWithProperty(UnitOfWork unitOfWork, Guid propertyID, string[] selectedTags)
        {
            if (selectedTags != null)
            {
                foreach (var tag in selectedTags)
                {
                    Tags tags = new Tags
                    {
                        ID = Guid.NewGuid(),
                        PropertyID = propertyID,
                        TypeID = unitOfWork.TagType.GetTagTypeIDByTagName(tag),
                        DateTCreated = DateTime.Now
                    };

                    unitOfWork.Tags.Add(tags);
                }
            }
        }

        /// <summary>
        ///  checks if a user exists before creating a new one
        /// </summary>
        /// <returns></returns>
        private bool doesUserExist(UnitOfWork unitOfWork, String email)
        {
            return unitOfWork.User.DoesUserExist(email);
        }

        /// <summary>
        /// used to produce a random key for the enrolment key 
        /// </summary>
        private string getRandomKey(int size)
        {
            string input = "abcdefghijklmnopqrstuvwxyz0123456789";

            Random random = new Random();

            var chars = Enumerable.Range(0, size)
                        .Select(x => input[random.Next(0, input.Length)]);

            return new string(chars.ToArray());
        }

        //loads the premium feature view
        public ActionResult PremiumFeature()
        {
            return View();
        }

        /// <summary>
        /// associates the image with the property that was uploaded
        /// </summary>
        /// <param name="file"></param>
        private void associateImagesWithProperty(UnitOfWork unitOfWork, HttpPostedFileBase[] files, Guid propertyID)
        {
            bool isPrimaryDisplay = true;//the first uploaded will be selected as the primary display

            foreach (var file in files)
            {
                string fileName = string.Empty;

                Guid guid = Guid.NewGuid();

                fileName = guid.ToString() + Path.GetExtension(file.FileName);

                PropertyImage propertyImage = new PropertyImage()
                {
                    ID = guid,
                    PropertyID = propertyID,
                    ImageURL = fileName,
                    IsPrimaryDisplay = isPrimaryDisplay,
                    DateTCreated = DateTime.Now
                };

                if (isPrimaryDisplay == true)
                    isPrimaryDisplay = false;

                unitOfWork.PropertyImage.Add(propertyImage);

                uploadPropertyImages(file, fileName);
            }
        }

        /// <summary>
        /// uploads the property image to the server 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        private void uploadPropertyImages(HttpPostedFileBase file, String fileName)
        {
            //ensures file is not empty and is a valid image
            if (file.ContentLength > 0 && file.ContentType.Contains("image"))
            {
                string path = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                file.SaveAs(path);

                PropertyHelper.resizeFile(fileName);

                uploadedImageNames.Add(fileName);
            }
            else
                throw new Exception("Upload an image file");
        }

        public ActionResult Requisition()
        {
            return View();
        }

        /// <summary>
        /// Enrolls tennant into a property
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Enroll(EnrolmentViewModel model)
        {
            ErrorModel errorModel = new ErrorModel();

            if (ModelState.IsValid)
            {
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    using (var dbCtxTran = dbCtx.Database.BeginTransaction())
                    {
                        try
                        {
                            if (!model.Password.Equals(model.ConfirmPassword))
                                throw new Exception("The fields Password and Confirm Password are not equal");

                            var unitOfWork = new UnitOfWork(dbCtx);

                            if (model.ReqID != new Guid())
                            {
                                EnrollTennantByRequisition(unitOfWork, model);
                            }
                            else
                            {
                                EnrollNewTennant(unitOfWork, model);
                            }

                            unitOfWork.save();
                            dbCtxTran.Commit();
                        }
                        catch (Exception ex)
                        {
                            dbCtxTran.UnderlyingTransaction.Rollback();

                            errorModel.hasErrors = true;
                            errorModel.ErrorMessages = new List<string>();
                            errorModel.ErrorMessages.Add(ex.Message);
                        }
                    }
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

                errorModel.hasErrors = true;
                errorModel.ErrorMessages = new List<string>();
                errorModel.ErrorMessages.AddRange(errors);
            }

            return Json(errorModel);
        }

        /// <summary>
        /// Enrolls tennant by the requisition functionality
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="model"></param>
        private void EnrollTennantByRequisition(UnitOfWork unitOfWork, EnrolmentViewModel model)
        {
            var requisition = unitOfWork.PropertyRequisition.Get(model.ReqID);

            bool isAccepted = requisition.IsAccepted.HasValue ? requisition.IsAccepted.Value : false;

            if (isAccepted)
            {
                string enrolKey = unitOfWork.Property.GetEnrolmentKeyByPropID(model.PropertyID);

                if (enrolKey.Equals(model.EnrolmentKey))
                {

                    PropertyHelper.createRolesIfNotExist();

                    var user = requisition.User;
                    var property = requisition.Property;
                    //assigning user as tennant if he/she isn't
                    var userTypes = unitOfWork.UserTypeAssoc.GetUserTypesByUserID(user.ID);
                    bool isUserTennant = PropertyHelper.isUserOfType(userTypes, EFPConstants.UserType.Tennant);

                    if (!isUserTennant)
                    {
                        PropertyHelper.associateUserWithUserType(unitOfWork, user.ID, EFPConstants.UserType.Tennant);
                    }

                    Tennant tennant = new Tennant()
                    {
                        ID = Guid.NewGuid(),
                        UserID = user.ID,
                        PropertyID = model.PropertyID,
                        RentAmt = property.Price,
                        SettlementPeriod = model.SettlementPeriod,
                        InstitutionName = model.InstitutionName,
                        ProgrammeName = model.ProgrammeName,
                        ProgrammeStartDate = DateTime.ParseExact(model.ProgrammeStartDate, "MM/dd/yyyy", CultureInfo.InvariantCulture),
                        ProgrammeEndDate = DateTime.ParseExact(model.ProgrammeEndDate, "MM/dd/yyyy", CultureInfo.InvariantCulture),
                        PhotoUrl = null,
                        ReferencedLetterURL = null,
                        DateTCreated = DateTime.Now
                    };

                    user.DOB = DateTime.ParseExact(model.DOB, "MM/dd/yyyy", CultureInfo.InvariantCulture);

                    unitOfWork.Tennant.Add(tennant);
                }
                else
                    throw new Exception("Enrolment key does not match the property being requested");
            }
            else
                throw new Exception("Requistion was not accepted by the property owner");
        }

        /// <summary>
        /// Enrolls a new user tennant and account for the user
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="model"></param>
        private void EnrollNewTennant(UnitOfWork unitOfWork, EnrolmentViewModel model)
        {
            string enrolKey = unitOfWork.Property.GetEnrolmentKeyByPropID(model.PropertyID);

            if (enrolKey.Equals(model.EnrolmentKey))
            {
                var user = PropertyHelper.createUser(unitOfWork, EFPConstants.UserType.Tennant, "", model.Email, model.FirstName,
                    model.LastName, model.CellNum, DateTime.Parse(model.DOB));

                PropertyHelper.createUserAccount(unitOfWork, EFPConstants.UserType.Tennant, model.Password);

                var propertyPrice = unitOfWork.Property.Get(model.PropertyID).Price;

                Tennant tennant = new Tennant()
                {
                    ID = Guid.NewGuid(),
                    UserID = user.ID,
                    PropertyID = model.PropertyID,
                    RentAmt = propertyPrice,
                    SettlementPeriod = model.SettlementPeriod,
                    InstitutionName = model.InstitutionName,
                    ProgrammeName = model.ProgrammeName,
                    ProgrammeStartDate = DateTime.Parse(model.ProgrammeEndDate),
                    ProgrammeEndDate = DateTime.Parse(model.ProgrammeEndDate),
                    PhotoUrl = null,
                    ReferencedLetterURL = null,
                    DateTCreated = DateTime.Now
                };

                unitOfWork.Tennant.Add(tennant);
            }
            else
                throw new Exception("Enrolment key does not match the property being requested");
        }
    }
}