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

namespace SS.Controllers
{
    public class AccountsController : Controller
    {
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
        public ActionResult SignIn(string cell, string password)
        {
            try
            {
                //validating user using the contact number of that person
                if (Membership.ValidateUser(cell, password))
                {
                    FormsAuthentication.SetAuthCookie(cell, true);

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
        public ActionResult Registration()
        {
            if (!String.IsNullOrEmpty(HttpContext.User.Identity.Name))
            {
                Session["userSignedInCheck"] = "Click add property to add a new property";

                return RedirectToAction("Dashboard", "LandlordManagement");
            }

            return View();
        }
        //loads the premium feature view
        public ActionResult PremiumFeature()
        {
            return View();
        }
        /*
         * used to register the property owner. If this call fails it will prevent all other calls for registration to halt.
         * If this call succeeds,  another request will be made to either register
         * a land, house or a room property. 
         */
        [HttpPost]
        public ActionResult RegistrationLandlord(LANDLORDS landlord, string chkProperty)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //initializing the database context 
                    using (JAHomesEntities dbCtx = new JAHomesEntities())
                    {
                        //variable used to detect whether a user exists in the database: if value > 0 user exists
                        int userExistCount = 0;

                        /*
                            * checking if user exists before create one 
                            * if user exists, redirect to a page that will give the user futher instructions
                        */
                        userExistCount = dbCtx.LANDLORDS.Where(e => e.CELL == landlord.CELL).Count();

                        if (userExistCount > 0)
                            throw new Exception("This user already exists.\nIf your already have a registered propert\nplease signin into your account then add your property from the portal");

                        if (landlord.PASSWORD.Equals(landlord.PASSWORD_CONFIRMED, StringComparison.InvariantCultureIgnoreCase))
                        {
                            /*
                             * create roles if they dont exists
                             * landlords role is only for landlords and tennats roles is only for tennants
                             */

                            if (!Roles.RoleExists("Landlords") || !Roles.RoleExists("Tennants"))
                            {
                                Roles.CreateRole("Landlords");
                                Roles.CreateRole("Tennants");
                            }

                            /*
                             * Saving the landlord's object in a session variable to be used once the property
                             * information is fully validated
                             */
                            Session["landlord"] = landlord;
                        }
                        else
                            throw new Exception("Sorry there has been an unexpected error during the registration process \nEnsure that the passwords match");
                    }
                }
                else
                {
                    throw new Exception("Sorry there has been an unexpected error during the registration process \n A field is empty/not selected");
                }
            }
            catch (Exception ex)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                       .Select(x => new { x.Key, x.Value.Errors })
                       .ToArray();


                return Content(ex.Message);
            }

            return Content("");
        }

        [HttpPost]
        public ActionResult RegistrationAccommodation(ACCOMMODATIONS accommodation, HttpPostedFileBase flPropertyPic)
        {
            MembershipCreateStatus status = new MembershipCreateStatus();
            try
            {
                if (ModelState.IsValid)
                {
                    using (JAHomesEntities dbCtx = new JAHomesEntities())
                    {
                        Guid landlord_id = new Guid();
                        LANDLORDS landlord = null;

                        if (Session["landlord"] != null)
                        {
                            //landlord coming from the landlordregistration method that was called first 
                            landlord = (LANDLORDS)Session["landlord"];
                        }
                        /*
                         * checking whether the generated key matches any key that is already in the database
                         * if it does regenerate a key then check again
                         */
                        bool isKeyFound = false;
                        string generated_key = string.Empty;

                        do
                        {
                            generated_key = getRandomKey(7);

                            var keys = dbCtx.ACCOMMODATIONS.Where(a => a.ENROLMENT_KEY == generated_key)
                                       .Select(a => a.ENROLMENT_KEY);

                            foreach (var key in keys)
                            {
                                if (key.ToString() == generated_key)
                                {
                                    isKeyFound = true;
                                }
                            }
                        } while (isKeyFound);
                        /*
                         * getting the name of the picture that was uploaded
                         * if no picture was uploaded then use a default picture
                         */
                        string propertyPicName;

                        if (flPropertyPic != null)
                        {
                            propertyPicName = getPropertyPicName(flPropertyPic);
                        }
                        else
                            propertyPicName = "Nopic.png";

                        if (String.IsNullOrEmpty(HttpContext.User.Identity.Name))
                        {
                            // creating user using the asp membership utility
                            MembershipUser newUser = Membership.CreateUser(landlord.CELL, landlord.PASSWORD, landlord.EMAIL, "null", "null", true, out status);

                            //ensures that the user was created before adding the additional information
                            if (newUser != null)
                            {
                                //inserts landlord into the appropriate table
                                dbCtx.sp_insert_landlord(landlord.FIRST_NAME, landlord.MIDDLE_NAME, landlord.LAST_NAME, landlord.GENDER, landlord.CELL, landlord.EMAIL, "");
                                //retrieving landlord id to support stored procedure
                                landlord_id = dbCtx.LANDLORDS.Where(i => i.CELL == landlord.CELL).Select(i => i.ID).Single();
                                //puts user in landlord role
                                Roles.AddUserToRole(landlord.CELL, "Landlords");

                                //inserts accommodation into the appropriate table
                                dbCtx.sp_insert_accommodation(accommodation.STREET_ADDRESS, accommodation.CITY, accommodation.PARISH, "", "", accommodation.PRICE
                                                             , accommodation.SECURITY_DEPOSIT, accommodation.OCCUPANCY, accommodation.GENDER_PREFERENCE, accommodation.DESCRIPTION,
                                                             accommodation.WATER, accommodation.ELECTRICITY, accommodation.CABLE, accommodation.GAS, accommodation.INTERNET,
                                                             true, generated_key, propertyPicName, "", landlord_id, accommodation.IS_STUDENT_ACC, accommodation.HOUSE_BATHROOM_AMOUNT);

                                Session["isRegistrationComplete"] = true;

                                return RedirectToAction("Home", "Home");
                            }
                            else
                                throw new Exception("Sorry there has been an unexpected error during the registration process \n A field is empty/not selected");
                        }
                        else
                        {
                            Session["isAdditionalProperty"] = true;
                            //retrieving landlord id to support stored procedure
                            landlord_id = dbCtx.LANDLORDS.Where(i => i.CELL == HttpContext.User.Identity.Name).Select(i => i.ID).Single();

                            //inserts accommodation into the appropriate table
                            dbCtx.sp_insert_accommodation(accommodation.STREET_ADDRESS, accommodation.CITY, accommodation.PARISH, "", "", accommodation.PRICE
                                                         , accommodation.SECURITY_DEPOSIT, accommodation.OCCUPANCY, accommodation.GENDER_PREFERENCE, accommodation.DESCRIPTION,
                                                         accommodation.WATER, accommodation.ELECTRICITY, accommodation.CABLE, accommodation.GAS, accommodation.INTERNET,
                                                         true, generated_key, propertyPicName, "", landlord_id, accommodation.IS_STUDENT_ACC, accommodation.HOUSE_BATHROOM_AMOUNT);

                            Session["isRegistrationComplete"] = true;
                        }
                    }
                }
                else
                {
                    throw new Exception("Sorry there has been an unexpected error during the registration process \n A field is empty/not selected");
                }
            }
            catch (Exception ex)
            {
                string errorListCombine = string.Empty;

                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                       .Select(x => new { x.Value.Errors })
                       .ToArray();

                foreach (var error in errors)
                {
                    errorListCombine += errors + "\n";
                }

                Session["registrationErrorMessage"] = ex.Message + " \n" + errorListCombine;
                //gets membership error
                if (status.ToString() != "") 
                   Session["registrationErrorMessage"] += "\n" + GetErrorMessage(status);
            }
            //redirects to the dashboard page instead of homepage
            if (Session["isAdditionalProperty"] != null)
            {
                if ((bool)Session["isAdditionalProperty"])
                    return RedirectToAction("Dashboard", "LandlordManagement");
            }

            return View("Registration");
        }
        /*
         * used to produce a random key for the enrolment key 
         * the size of this key is 7 alphanumerical characters 
         */
        private string getRandomKey(int size)
        {
            string input = "abcdefghijklmnopqrstuvwxyz0123456789";

            Random random = new Random();

            var chars = Enumerable.Range(0, size)
                        .Select(x => input[random.Next(0, input.Length)]);

            return new string(chars.ToArray());
        }

        [HttpPost]
        public ActionResult RegistrationHouse(HOUSE house, HttpPostedFileBase flPropertyPic)
        {
            MembershipCreateStatus status = new MembershipCreateStatus();
            try
            {
                if (ModelState.IsValid)
                {
                    using (JAHomesEntities dbCtx = new JAHomesEntities())
                    {
                        Guid landlord_id = new Guid();
                        LANDLORDS landlord = null;

                        if (Session["landlord"] != null)
                        {
                            //landlord coming from the landlordregistration method that was called first 
                            landlord = (LANDLORDS)Session["landlord"];
                        }

                        string propertyPicName;

                        if (flPropertyPic != null)
                        {
                            propertyPicName = getPropertyPicName(flPropertyPic);
                        }
                        else
                            propertyPicName = "Nopic.jpg";

                        if (String.IsNullOrEmpty(HttpContext.User.Identity.Name))
                        {
                            // creating user using the asp membership utility
                            MembershipUser newUser = Membership.CreateUser(landlord.CELL, landlord.PASSWORD, landlord.EMAIL, "null", "null", true, out status);

                            //ensures that the user was created before adding the additional information
                            if (newUser != null)
                            {
                                //inserts landlord into the appropriate table
                                dbCtx.sp_insert_landlord(landlord.FIRST_NAME, landlord.MIDDLE_NAME, landlord.LAST_NAME, landlord.GENDER, landlord.CELL, landlord.EMAIL, "");
                                //retrieving landlord id to support stored procedure
                                landlord_id = dbCtx.LANDLORDS.Where(i => i.CELL == landlord.CELL).Select(i => i.ID).Single();
                                //puts user in landlord role
                                Roles.AddUserToRole(landlord.CELL, "Landlords");
                                //inserts house into the appropriate table
                                dbCtx.sp_insert_house(house.STREET_ADDRESS, house.CITY, house.PARISH, house.PRICE, house.BED_ROOM_AMOUNT, house.LIVING_ROOM_AMOUNT,
                                    house.BATH_ROOM_AMOUNT, house.PURPOSE, house.ISFURNISHED, house.DESCRIPTION, propertyPicName, landlord_id);

                                Session["isRegistrationComplete"] = true;

                                return RedirectToAction("Home", "Home");
                            }
                            else
                                throw new Exception("Sorry there has been an unexpected error during the registration process \n A field is empty/not selected");
                        }
                        else
                        {
                            Session["isAdditionalProperty"] = true;
                            //retrieving landlord id to support stored procedure
                            landlord_id = dbCtx.LANDLORDS.Where(i => i.CELL == HttpContext.User.Identity.Name).Select(i => i.ID).Single();

                            //inserts house into the appropriate table
                            dbCtx.sp_insert_house(house.STREET_ADDRESS, house.CITY, house.PARISH, house.PRICE, house.BED_ROOM_AMOUNT, house.LIVING_ROOM_AMOUNT,
                                house.BATH_ROOM_AMOUNT, house.PURPOSE, house.ISFURNISHED, house.DESCRIPTION, propertyPicName, landlord_id);

                            Session["isRegistrationComplete"] = true;
                        }
                        
                    }
                }
                else
                {
                    throw new Exception("Sorry there has been an unexpected error during the registration process \n A field is empty/not selected");
                }
            }
            catch (Exception ex)
            {
                string errorListCombine = string.Empty;

                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                       .Select(x => new { x.Value.Errors })
                       .ToArray();

                foreach (var error in errors)
                {
                    errorListCombine += errors + "\n";
                }

                Session["registrationErrorMessage"] = ex.Message + " \n" + errorListCombine;
                //gets membership error
                if (status.ToString() != "")
                    Session["registrationErrorMessage"] += "\n" + GetErrorMessage(status);
            }
            //redirects to the dashboard page instead of homepage
            if (Session["isAdditionalProperty"] != null)
            {
                if ((bool)Session["isAdditionalProperty"])
                    return RedirectToAction("Dashboard", "LandlordManagement");
            }

            return View("Registration");
        }

        [HttpPost]
        public ActionResult RegistrationLand(LAND land, HttpPostedFileBase flPropertyPic)
        {
            MembershipCreateStatus status = new MembershipCreateStatus();
            try
            {
                if (ModelState.IsValid)
                {
                    Guid landlord_id = new Guid();
                    LANDLORDS landlord = null;

                    if (Session["landlord"] != null)
                    {
                        //landlord coming from the landlordregistration method that was called first 
                        landlord = (LANDLORDS)Session["landlord"];
                    }

                    using (JAHomesEntities dbCtx = new JAHomesEntities())
                    {
                        string propertyPicName;

                        if (flPropertyPic != null)
                        {
                            propertyPicName = getPropertyPicName(flPropertyPic);
                        }
                        else
                            propertyPicName = "Nopic.jpg";

                        if (String.IsNullOrEmpty(HttpContext.User.Identity.Name))
                        {
                            // creating user using the asp membership utility
                            MembershipUser newUser = Membership.CreateUser(landlord.CELL, landlord.PASSWORD, landlord.EMAIL, "null", "null", true, out status);
                            
                            //ensures that the user was created before adding the additional information
                            if (newUser != null)
                            {
                                //inserts landlord into the appropriate table
                                dbCtx.sp_insert_landlord(landlord.FIRST_NAME, landlord.MIDDLE_NAME, landlord.LAST_NAME, landlord.GENDER, landlord.CELL, landlord.EMAIL, "");
                                //retrieving landlord id to support stored procedure
                                landlord_id = dbCtx.LANDLORDS.Where(i => i.CELL == landlord.CELL).Select(i => i.ID).Single();
                                //puts user in landlord role
                                Roles.AddUserToRole(landlord.CELL, "Landlords");

                                //inserts land into the appropriate table
                                dbCtx.sp_insert_land(land.STREET_ADDRESS, land.CITY, land.PARISH, land.PURPOSE, land.PRICE, land.AREA.ToString(), land.DESCRIPTION, propertyPicName, landlord_id);

                                Session["isRegistrationComplete"] = true;

                                return RedirectToAction("Home", "Home");
                            }
                            else
                                throw new Exception("Sorry there has been an unexpected error during the registration process \n A field is empty/not selected");
                        }
                        else
                        {
                            Session["isAdditionalProperty"] = true;
                            //retrieving landlord id to support stored procedure
                            landlord_id = dbCtx.LANDLORDS.Where(i => i.CELL == HttpContext.User.Identity.Name).Select(i => i.ID).Single();

                            //inserts land into the appropriate table
                            dbCtx.sp_insert_land(land.STREET_ADDRESS, land.CITY, land.PARISH, land.PURPOSE, land.PRICE, land.AREA.ToString(), land.DESCRIPTION, propertyPicName, landlord_id);

                            Session["isRegistrationComplete"] = true;
                        } 
                    }
                }
                else
                {
                    throw new Exception("Sorry there has been an unexpected error during the registration process \n A field is empty/not selected");
                }
            }
            catch (Exception ex)
            {
                string errorListCombine = string.Empty;

                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                       .Select(x => new { x.Value.Errors })
                       .ToArray();

                foreach (var error in errors)
                {
                    errorListCombine += errors + "\n";
                }

                Session["registrationErrorMessage"] = ex.Message + " \n" + errorListCombine;
                //gets membership error
                if (status.ToString() != "")
                    Session["registrationErrorMessage"] += "\n" + GetErrorMessage(status);
            }
            //redirects to the dashboard page instead of homepage
            if (Session["isAdditionalProperty"] != null)
            {
                if ((bool)Session["isAdditionalProperty"])
                    return RedirectToAction("Dashboard", "LandlordManagement");
            }

            return View("Registration");
        }

        //return name of picture uploaded
        private string getPropertyPicName(HttpPostedFileBase file)
        {
            string fileName = string.Empty;

            try
            {
                var guid = Guid.NewGuid();

                //ensures file is not empty and is a valid image
                if (file.ContentLength > 0 && file.ContentType.Contains("image"))
                {
                    fileName = guid.ToString() + Path.GetExtension(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/Uploads"), fileName);

                    file.SaveAs(path);
                }
                else
                    throw new Exception("Sorry there has been an unexpected error during the registration process");
            }
            catch (Exception ex)
            {
                Session["registrationErrorMessage"] = ex.Message;
            }

            return fileName;
        }
        //used for membership error message
        public string GetErrorMessage(MembershipCreateStatus status)
        {
            switch (status)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

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
        //return _house partial view
        [HttpGet]
        public PartialViewResult getHousePartialView()
        {
            HOUSE house = new HOUSE();

            house.Parishes = new[]
            {
                new SelectListItem {Value = "westmoreland",Text = "Westmoreland"},
                new SelectListItem {Value = "hanover",Text = "Hanover"},
                new SelectListItem {Value = "stjames",Text = "St. James"},
                new SelectListItem {Value = "trelawny",Text = "Trelawny"},
                new SelectListItem {Value = "stann",Text = "St. Ann"},
                new SelectListItem {Value = "stmary",Text = "St. Mary"},
                new SelectListItem {Value = "portland",Text = "Portland"},
                new SelectListItem {Value = "stthomas",Text = "St. Thomas"},
                new SelectListItem {Value = "kingston",Text = "Kingston"},
                new SelectListItem {Value = "standrew",Text = "St. Andrew"},
                new SelectListItem {Value = "stcatherine",Text = "St. Catherine"},
                new SelectListItem {Value = "clarendon",Text = "Clarendon"},
                new SelectListItem {Value = "manchester",Text = "Manchester"},
                new SelectListItem {Value = "stelizabeth",Text = "St. Elizabeth"}
            };

            return PartialView("_HouseInformationRegistration",house);
        }
        //return _room partial view
        [HttpGet]
        public PartialViewResult getRoomPartialView()
        {
            ACCOMMODATIONS accommodations = new ACCOMMODATIONS();

          /*  accommodations.Parishes = new[]
            {
                new SelectListItem {Value = "westmoreland",Text = "Westmoreland"},
                new SelectListItem {Value = "hanover",Text = "Hanover"},
                new SelectListItem {Value = "stjames",Text = "St. James"},
                new SelectListItem {Value = "trelawny",Text = "Trelawny"},
                new SelectListItem {Value = "stann",Text = "St. Ann"},
                new SelectListItem {Value = "stmary",Text = "St. Mary"},
                new SelectListItem {Value = "portland",Text = "Portland"},
                new SelectListItem {Value = "stthomas",Text = "St. Thomas"},
                new SelectListItem {Value = "kingston",Text = "Kingston"},
                new SelectListItem {Value = "standrew",Text = "St. Andrew"},
                new SelectListItem {Value = "stcatherine",Text = "St. Catherine"},
                new SelectListItem {Value = "clarendon",Text = "Clarendon"},
                new SelectListItem {Value = "manchester",Text = "Manchester"},
                new SelectListItem {Value = "stelizabeth",Text = "St. Elizabeth"}
            };*/

            return PartialView("_RoomInformationRegistration",accommodations);
        }
        //return _land partial view
        [HttpGet]
        public PartialViewResult getLandPartialView()
        {
            LAND land = new LAND();

            land.Parishes = new[]
            {
                new SelectListItem {Value = "westmoreland",Text = "Westmoreland"},
                new SelectListItem {Value = "hanover",Text = "Hanover"},
                new SelectListItem {Value = "stjames",Text = "St. James"},
                new SelectListItem {Value = "trelawny",Text = "Trelawny"},
                new SelectListItem {Value = "stann",Text = "St. Ann"},
                new SelectListItem {Value = "stmary",Text = "St. Mary"},
                new SelectListItem {Value = "portland",Text = "Portland"},
                new SelectListItem {Value = "stthomas",Text = "St. Thomas"},
                new SelectListItem {Value = "kingston",Text = "Kingston"},
                new SelectListItem {Value = "standrew",Text = "St. Andrew"},
                new SelectListItem {Value = "stcatherine",Text = "St. Catherine"},
                new SelectListItem {Value = "clarendon",Text = "Clarendon"},
                new SelectListItem {Value = "manchester",Text = "Manchester"},
                new SelectListItem {Value = "stelizabeth",Text = "St. Elizabeth"}
            };

            return PartialView("_LandInformationRegistration",land);
        }
        //return _land partial view
        [HttpGet]
        public PartialViewResult getLandlordPartialView()
        {
            return PartialView("_OwnerInformationRegistration");
        }

    }
}