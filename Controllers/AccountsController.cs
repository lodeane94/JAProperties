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

        public ActionResult SignOut()
        {
            Session["userId"] = null;
            Session["username"] = null;
            Session["isUserPropOwner"] = null;
            Session["isUserConsumer"] = null;
            Session["isUserTennant"] = null;

            FormsAuthentication.SignOut();

            return RedirectToAction("home", "home");
        }

        public ActionResult SignIn()
        {
            string username = HttpContext.User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
            {
                startUserSessions();

                if (Roles.IsUserInRole(username, "admin"))
                {
                    return RedirectToAction("verifypayments", "admin");
                }

                return RedirectToAction("Dashboard", "LandlordManagement");
            }

            return View();
        }

        [HttpPost]
        public ActionResult SignIn(string username, string password)
        {
            try
            {
                MembershipUser user = Membership.GetUser(username);

                if (user != null && user.IsLockedOut)
                {
                    string errMessage = "You have exceeded the maximum number of failed attempts <br /> User account has been locked";
                    errMessage += " Reset your password using the link below";

                    Session["invalidUser"] = errMessage;

                    return View();
                }

                if (Membership.ValidateUser(username, password))
                {
                    FormsAuthentication.SetAuthCookie(username, true);

                    startUserSessions(username);

                    if (Roles.IsUserInRole(username, "admin"))
                    {
                        return RedirectToAction("verifypayments", "admin");
                    }

                    return RedirectToAction("Dashboard", "LandlordManagement");
                }
                else
                {
                    Session["invalidUser"] = "Incorrect username or password combination was inserted";
                    return View();
                }
            }
            catch (Exception ex)
            {
                Session["invalidUser"] = "An unexpected error occurred - please contact system administrator or try again later";
                return View();
            }
        }
        /// <summary>
        /// Set user information to sessions
        /// </summary>
        /// <param name="unitOfWork"></param>
        private void startUserSessions(string username = "")
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                Guid userId;

                if (String.IsNullOrEmpty(username))
                {
                    userId = unitOfWork.User.GetUserByEmail(HttpContext.User.Identity.Name).ID;
                }
                else
                {
                    userId = unitOfWork.User.GetUserByEmail(username).ID;
                }

                var userTypes = unitOfWork.UserTypeAssoc.GetUserTypesByUserID(userId);

                Session["userId"] = userId;
                Session["username"] = HttpContext.User.Identity.Name;
                Session["isUserPropOwner"] = PropertyHelper.isUserOfType(userTypes, EFPConstants.UserType.PropertyOwner);
                Session["isUserConsumer"] = PropertyHelper.isUserOfType(userTypes, EFPConstants.UserType.Consumer);
                Session["isUserTennant"] = PropertyHelper.isUserOfType(userTypes, EFPConstants.UserType.Tennant);
            }
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

                            errorModel.AddErrorMessage(ex.Message);
                        }
                    }
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                errorModel.AddErrorMessages(errors);
            }

            return Json(errorModel);
        }

        //loads advertise property view
        public ActionResult AdvertiseProperty()
        {
            AdvertisePropertyViewModel model = new AdvertisePropertyViewModel();

            if (!String.IsNullOrEmpty(HttpContext.User.Identity.Name) && TempData["calledByManagement"] == null)
            {
                Session["userSignedInCheck"] = "Click add property to add a new property";

                return RedirectToAction("Dashboard", "LandlordManagement");
            }

            if (TempData["layout"] != null && TempData["calledByManagement"] != null)
            {
                ViewBag.layout = (String)TempData["layout"];
                ViewBag.calledByManagement = (bool)TempData["calledByManagement"];
            }
            else
                ViewBag.layout = "~/Views/Shared/_Layout.cshtml";

            model.SubscriptionTypes = PropertyHelper.GetSubscriptionTypes();

            return View(model);
        }

        [HttpPost]
        public JsonResult AdvertiseProperty(AdvertisePropertyViewModel model)
        {
            ErrorModel errorModel = new ErrorModel();
            Guid userId = Guid.Empty;

            if (ModelState.IsValid)
            {
                if (Session["userId"] != null)
                {
                    userId = (Guid)Session["userId"];
                }
                PropertyHelper.AdvertiseProperty(model, userId, errorModel);
            }
            else
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                errorModel.AddErrorMessages(errors);
            }

            return Json(errorModel);
        }

        //loads the premium feature view
        public ActionResult PremiumFeature()
        {
            return View();
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

                            errorModel.AddErrorMessage(ex.Message);
                        }
                    }
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                errorModel.AddErrorMessages(errors);
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

        public ActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RecoverPassword(String email)
        {
            ErrorModel errorModel = new ErrorModel();

            if (ModelState.IsValid)
            {
                if (PropertyHelper.RecoverPassword(email, errorModel))
                {
                    TempData["success"] = "An email was sent to you containing a access code to reset your password";
                }
            }
            else
                errorModel.AddErrorMessage("Invalid form submitted");
                
            if (errorModel.GetHasErrors())
                TempData["errorMessage"] = errorModel.GetErrorMessages();

            return View();
        }

        [HttpGet]
        public ActionResult ResetPassword(Guid ? userId)
        {
            if (userId.HasValue)
            {
                ViewBag.userId = userId.Value;
                return View();
            }

            return RedirectToAction("signin", "accounts");
        }

        [HttpPost]
        public ActionResult ResetPassword(Guid userId, string password, string confirmPassword)
        {
            ErrorModel errorModel = new ErrorModel();

            if (ModelState.IsValid)
            {
                if (password.Equals(confirmPassword) )
                {
                    if (password.Length > 4)
                    {
                        if (PropertyHelper.ResetPassword(userId, password, errorModel))
                        {
                            TempData["pwdChangedSuccess"] = "Your password was changed successfully";
                            Session["accessCodeValid"] = null;
                        }
                        else
                            errorModel.AddErrorMessage("An error occurred");
                    }
                    else
                        errorModel.AddErrorMessage("The password length should be at least 5 characters");
                }
                else
                    errorModel.AddErrorMessage("The Password and Confirm Password fields must have the same value");
            }
            else
                errorModel.AddErrorMessage("Invalid form submitted");

            if (errorModel.GetHasErrors())
                TempData["errorMessage"] = errorModel.GetErrorMessages();

            ViewBag.userId = userId;
            return RedirectToAction("resetPassword", "accounts", new { userId = userId });
        }

        public ActionResult ValidateAccessCode(Guid userId, string accessCode)
        {
            ErrorModel errorModel = new ErrorModel();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(accessCode))
                {
                    if (PropertyHelper.ValidateAccessCode(userId, accessCode))
                        Session["accessCodeValid"] = 4;
                    else
                        errorModel.AddErrorMessage("This access code is not valid or has expired");
                }
                else
                    errorModel.AddErrorMessage("Access code cannot be null");
            }
            else
                errorModel.AddErrorMessage("Invalid form submitted");

            if (errorModel.GetHasErrors())
                TempData["errorMessage"] = errorModel.GetErrorMessages();

            ViewBag.userId = userId;
            return RedirectToAction("resetPassword", "accounts", new { userId = userId});
        }
    }
}