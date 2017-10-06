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
        //loads registration view
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
                            createRolesIfNotExist();

                            var unitOfWork = new UnitOfWork(dbCtx);

                            bool ownerExist = doesOwnerExist(unitOfWork, model.CellNum);
                            insertProperty(model, unitOfWork, ownerExist);

                            if(!ownerExist)
                                createUserAccount(model);

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

                            return Json(errorModel);
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
        /// Creates a membership account for the property owner
        /// </summary>
        /// <param name="model"></param>
        private void createUserAccount(AdvertisePropertyViewModel model)
        {
            MembershipCreateStatus status = new MembershipCreateStatus();
            //create account for non-basic subscriptions
            if (!model.SubscriptionType.Equals(nameof(EFPConstants.PropertySubscriptionType.Basic)))
            {
                MembershipUser newUser = Membership.CreateUser(model.Email, model.Password, model.Email, "null", "null", true, out status);
                if (newUser != null)
                {
                    addUserToRespectedRole(model.Email, model.SubscriptionType);
                }
                else
                    throw new Exception(GetMembershipErrorMessage(status));
            }
        }

        /// <summary>
        /// Inserts the property along with it's owner, subscription period and also the property images
        /// </summary>
        /// <param name="model"></param>
        private void insertProperty(AdvertisePropertyViewModel model, UnitOfWork unitOfWork, bool ownerExist)
        {
            Guid ownerID = ownerExist ? unitOfWork.Owner.GetOwnerIDByCellNum(model.CellNum) : Guid.NewGuid();
            Guid propertyID = Guid.NewGuid();

            //generate enrolment key for users with Landlord subscription
            if (model.SubscriptionType.Equals(nameof(EFPConstants.PropertySubscriptionType.Landlord)))
            {
                model.EnrolmentKey = getRandomKey(6);
            }

            Property property = new Property()
            {
                ID = propertyID,
                OwnerID = ownerID,
                PurposeCode = PropertyHelper.mapPropertyPurposeNameToCode(model.PropertyPurpose),
                TypeID = unitOfWork.PropertyType.GetPropertyTypeIDByName(model.PropertyType),
                AdTypeCode = PropertyHelper.mapPropertyAdTypeNameToCode(model.AdvertismentType),
                AdPriorityCode = PropertyHelper.mapPropertyAdpriorityNameToCode(model.AdvertismentPriority),
                ConditionCode = EFPConstants.PropertyCondition.NotSurveyed,
                StreetAddress = model.StreetAddress,
                Division = model.Division,
                Community = model.Community,
                Country = model.Country,
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

            if (!ownerExist)
            {
                Owner owner = new Owner()
                {
                    ID = ownerID,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CellNum = model.CellNum,
                    Email = model.Email,
                    DateTCreated = DateTime.Now
                };

                unitOfWork.Owner.Add(owner);
            }
            unitOfWork.Property.Add(property);
            unitOfWork.Subscription.Add(subscription);

            associateTagsWithProperty(unitOfWork, propertyID, model.selectedTags);
            associateImagesWithProperty(unitOfWork, model.flPropertyPics, propertyID);

            unitOfWork.save();
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
        /// adds user to it's respected role and generate enrolment key if necessary
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subscriptionType"></param>
        private void addUserToRespectedRole(string email, string subscriptionType)
        {
            if (subscriptionType.Equals(nameof(EFPConstants.PropertySubscriptionType.Landlord)))
            {
                Roles.AddUserToRole(email, EFPConstants.RoleNames.Landlord.ToString());
            }
            else if (subscriptionType.Equals(nameof(EFPConstants.PropertySubscriptionType.Realtor)))
            {
                Roles.AddUserToRole(email, EFPConstants.RoleNames.Realtor.ToString());
            }
        }

        /// <summary>
        ///  checks if a property owner exists before creating a new one
        /// </summary>
        /// <returns></returns>
        private bool doesOwnerExist(UnitOfWork unitOfWork, String cellNum)
        {
            return unitOfWork.Owner.DoesOwnerExist(cellNum);
        }

        /// <summary>
        /// creates lanlord,tennant and realtor roles if they dont exist
        /// </summary>
        private void createRolesIfNotExist()
        {
            if (!Roles.RoleExists("Landlord")
                    || !Roles.RoleExists("Tennant")
                    || !Roles.RoleExists("Realtor"))
            {
                Roles.CreateRole(EFPConstants.RoleNames.Landlord.ToString());
                Roles.CreateRole(EFPConstants.RoleNames.Tennant.ToString());
                Roles.CreateRole(EFPConstants.RoleNames.Realtor.ToString());
            }
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

                uploadedImageNames.Add(fileName);
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
            }
            else
                throw new Exception("Upload an image file");
        }

        //used for membership error message
        public string GetMembershipErrorMessage(MembershipCreateStatus status)
        {
            switch (status)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Email address already exists. Please sign into your portal to add more properties";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.\nThe passwords should be 4 characters long";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }
}