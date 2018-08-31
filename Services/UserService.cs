using log4net;
using SS.Core;
using SS.Models;
using SS.ViewModels.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Security;

namespace SS.Services
{
    public class UserService
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UserService() { }

        /// <summary>
        /// Creates a membership account for the property owner
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="userType"></param>
        /// <param name="model"></param>
        public void CreateUserAccount(String email, String password)
        {
            MembershipCreateStatus status = new MembershipCreateStatus();

            MembershipUser newUser = Membership.CreateUser(email, password, email, "null", "null", true, out status);

            if (newUser == null)
            {
                throw new Exception(GetMembershipErrorMessage(status));
            }
        }

        /// <summary>
        /// removes a membership account for the property owner
        /// </summary>
        /// <param name="email"></param>
        public bool RemoveUserAccount(UnitOfWork unitOfWork, String email)
        {
            var wasUserRemoved = false;

            User user = unitOfWork.User.GetUserByEmail(email);

            if (user != null)
            {
                unitOfWork.User.Remove(user);
                wasUserRemoved = Membership.DeleteUser(email, true);

                unitOfWork.save();
            }

            return wasUserRemoved;
        }

        /// <summary>
        /// Creates a user object
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="userType"></param>
        /// <param name="subscriptionType"></param>
        /// <param name="email"></param>
        /// <param name="fName"></param>
        /// <param name="lName"></param>
        /// <param name="cellNum"></param>
        /// <returns></returns>
        public User CreateUser(UnitOfWork unitOfWork, String userType, String subscriptionType
            , String email, String fName, String lName, String cellNum, String areaCode, DateTime dob)
        {
            User user = null;
            var userID = Guid.NewGuid();

            if (!dob.Equals(DateTime.MinValue))
            {
                user = new User()
                {
                    ID = userID,
                    FirstName = MiscellaneousHelper.UppercaseFirst(fName),
                    LastName = MiscellaneousHelper.UppercaseFirst(lName),
                    CellNum = areaCode + cellNum,
                    Email = email,
                    DOB = dob,
                    DateTCreated = DateTime.Now
                };
            }
            else
            {
                user = new User()
                {
                    ID = userID,
                    FirstName = MiscellaneousHelper.UppercaseFirst(fName),
                    LastName = MiscellaneousHelper.UppercaseFirst(lName),
                    CellNum = areaCode + cellNum,
                    Email = email,
                    DateTCreated = DateTime.Now
                };

            }

            unitOfWork.User.Add(user);

            AssociateUserWithUserType(unitOfWork, userID, userType);
            AddUserToRespectedRole(email, subscriptionType);

            return user;
        }

        /// <summary>
        /// Associates a user to an user type
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="userID"></param>
        /// <param name="userType"></param>
        public void AssociateUserWithUserType(UnitOfWork unitOfWork, Guid userID, String userType)
        {
            UserTypeAssoc userTypeAssoc = new UserTypeAssoc()
            {
                ID = Guid.NewGuid(),
                UserID = userID,
                UserTypeCode = userType,
                DateTCreated = DateTime.Now
            };

            unitOfWork.UserTypeAssoc.Add(userTypeAssoc);
        }

        /// <summary>
        /// adds user to it's respected role and generate enrolment key if necessary
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subscriptionType"></param>
        public void AddUserToRespectedRole(string email, string subscriptionType)
        {
            CreateRolesIfNotExist();

            if (subscriptionType.Equals(nameof(EFPConstants.PropertySubscriptionType.Landlord)))
            {
                Roles.AddUserToRole(email, EFPConstants.RoleNames.Landlord.ToString());
            }
            else if (subscriptionType.Equals(nameof(EFPConstants.PropertySubscriptionType.Realtor)))
            {
                Roles.AddUserToRole(email, EFPConstants.RoleNames.Realtor.ToString());
            }
            else if (subscriptionType.Equals(nameof(EFPConstants.PropertySubscriptionType.Basic)))
            {
                Roles.AddUserToRole(email, EFPConstants.RoleNames.Basic.ToString());
            }
            /*removed since subscription type is only for paid users
            else if (subscriptionType.Equals(EFPConstants.RoleNames.Tennant.ToString()))
            {
                Roles.AddUserToRole(email, EFPConstants.RoleNames.Tennant.ToString());
            }
            else
                Roles.AddUserToRole(email, EFPConstants.RoleNames.Consumer.ToString());*/
        }

        /// <summary>
        /// creates lanlord,tennant and realtor roles if they dont exist
        /// </summary>
        public void CreateRolesIfNotExist()
        {
            if (!Roles.RoleExists("Basic"))
            {
                Roles.CreateRole(EFPConstants.RoleNames.Basic.ToString());
            }

            if (!Roles.RoleExists("Landlord"))
            {
                Roles.CreateRole(EFPConstants.RoleNames.Landlord.ToString());
            }

            if (!Roles.RoleExists("Realtor"))
            {
                Roles.CreateRole(EFPConstants.RoleNames.Realtor.ToString());
            }

            if (!Roles.RoleExists("Admin"))
            {
                Roles.CreateRole(EFPConstants.RoleNames.Admin.ToString());
            }
            /*
             if (!Roles.RoleExists("Tennant"))
            {
                Roles.CreateRole(EFPConstants.RoleNames.Tennant.ToString());
            }            

            if (!Roles.RoleExists("Consumer"))
            {
                Roles.CreateRole(EFPConstants.RoleNames.Consumer.ToString());
            }*/
        }

        //used for membership error message
        private string GetMembershipErrorMessage(MembershipCreateStatus status)
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

        /// <summary>
        /// Checks the user type
        /// </summary>
        /// <param name="userTypes"></param>
        /// <param name="userType"></param>
        /// <returns></returns>
        public bool IsUserOfType(IEnumerable<String> userTypes, String userType)
        {
            var count = userTypes.Where(x => x.Equals(userType)).Count();

            return count > 0 ? true : false;
        }

        /// <summary>
        /// Checks if a user exists before creating a new one
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        private bool DoesUserExist(UnitOfWork unitOfWork, String email)
        {
            return unitOfWork.User.DoesUserExist(email);
        }

        /// <summary>
        /// Populates and returns the account view model
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isPropOwner"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public ProfileViewModel PopulateProfileViewModel(Guid userId, bool isPropOwner, UnitOfWork unitOfWork)
        {
            var user = unitOfWork.User.Get(userId);

            ProfileViewModel profileVM = new ProfileViewModel()
            {
                ID = user.ID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CellNum = user.CellNum,
                Email = user.Email,
            };

            if (isPropOwner)
            {
                var owner = unitOfWork.Owner.GetOwnerByUserID(userId);
                profileVM.Organization = owner.Organization;
                profileVM.LogoUrl = owner.LogoUrl;
            }

            return profileVM;
        }

        /// <summary>
        ///  Updates a user's profile information
        /// </summary>
        /// <param name="model"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public bool UpdateProfile(ProfileViewModel model, UnitOfWork unitOfWork)
        {
            var currentUser = unitOfWork.User.Get(model.ID);
            var currentOwner = unitOfWork.Owner.GetOwnerByUserID(model.ID);

            currentUser.FirstName = model.FirstName;
            currentUser.LastName = model.LastName;
            currentUser.CellNum = model.CellNum;
            currentUser.Email = model.Email;
            currentOwner.Organization = model.Organization;

            if (model.organizationLogo != null)
            {
                currentOwner.LogoUrl = PropertyHelper.ReplaceUplodedImage(currentOwner.LogoUrl, model.organizationLogo);
            }

            try
            {
                unitOfWork.save();
                return true; // indicate success
            }
            catch (Exception ex)
            {
                if (PropertyHelper.uploadedImageNames != null && PropertyHelper.uploadedImageNames.Count > 0)
                {
                    PropertyHelper.RemoveUploadedImages(PropertyHelper.uploadedImageNames);
                }
                log.Error("Profile update unsuccessful");
                return false;//indicate failure
            }

        }

        /// <summary>
        /// recovers user password by generating a unique code
        /// </summary>
        /// <param name="email"></param>
        /// <param name="errorModel"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public bool RecoverPassword(string email, ErrorModel errorModel, UnitOfWork unitOfWork)
        {
            using (var txscope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                try
                {
                    var userExist = unitOfWork.User.DoesUserExist(email);

                    if (!userExist)
                    {
                        string errMessage = "The email address was not recognised";
                        errorModel.AddErrorMessage(errMessage);

                        return false;
                    }

                    var fName = unitOfWork.User.GetUserByEmail(email).FirstName;
                    var userId = unitOfWork.User.GetUserByEmail(email).ID;
                    var uniqueKey = PropertyHelper.GetRandomKey(5);

                    savePasswordRecoveryRequest(unitOfWork, userId, uniqueKey);
                    var emailSent = sendRecoverPasswordEmail(email, userId, fName, uniqueKey, errorModel);

                    txscope.Complete();

                    return emailSent;
                }
                catch (Exception ex)
                {
                    string errMsg = "An error occurred while recovering password";
                    errorModel.AddErrorMessage(errMsg);
                    log.Error(errMsg, ex);

                    return false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="userId"></param>
        /// <param name="accessCode"></param>
        /// <param name="errorModel"></param>
        /// <returns></returns>
        private bool sendRecoverPasswordEmail(string email, Guid userId, string fName, string accessCode, ErrorModel errorModel)
        {
            try
            {
                string emailTo = email;
                string subject = "JProps - Password Recovery";
                string body = "Your access code to reset your password is <b>" + accessCode + "</b> ";
                body += ". Please click the link below or copy and paste it in a new browser window to reset your password: <br/><br/>";
                body += "http://www." + EFPConstants.Application.Host + "/accounts/resetpassword?userId=" + userId.ToString();
                body += "<br/><br/><small>Your access code will expire 10 minutes after recieving this mail</small>";

                MailHelper mail = new MailHelper(emailTo, subject, body, fName);

                return mail.SendMail();
            }
            catch (Exception ex)
            {
                string errMessage = "An unexpected error occurred while sending the recovery password <br /> Please try again later";
                errorModel.AddErrorMessage(errMessage);
                log.Error(errMessage);

                throw new Exception(errMessage, ex);
            }
        }

        private void savePasswordRecoveryRequest(UnitOfWork unitOfWork, Guid userId, string accessCode)
        {
            PasswordRecoveryRequest pwdRecoveryRequest = new PasswordRecoveryRequest()
            {
                ID = Guid.NewGuid(),
                UserID = userId,
                AccessCode = accessCode,
                Processed = false,
                ExpiryDate = DateTime.Now.AddMinutes(10),
                DateTCreated = DateTime.Now
            };

            unitOfWork.PasswordRecoveryRequest.Add(pwdRecoveryRequest);
            unitOfWork.save();
        }

        /// <summary>
        /// resets a user's password
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="errorModel"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public bool ResetPassword(Guid userId, string password, ErrorModel errorModel, UnitOfWork unitOfWork)
        {
            var user = unitOfWork.User.Get(userId);

            if (user == null)
            {
                string errMessage = "The user does not exist";
                errorModel.AddErrorMessage(errMessage);
                return false;
            }

            var membershipUsr = Membership.GetUser(user.Email);

            if (membershipUsr == null)
            {
                string errMessage = "The user does not exist";
                errorModel.AddErrorMessage(errMessage);
                return false;
            }

            try
            {
                if (membershipUsr.IsLockedOut)
                    membershipUsr.UnlockUser();

                var tempOldPassword = membershipUsr.ResetPassword();
                membershipUsr.ChangePassword(tempOldPassword, password);

                return true;
            }
            catch (MembershipPasswordException e)
            {
                string errMessage = "Unable to reset your password : " + e.Message;
                errorModel.AddErrorMessage(errMessage);
                log.Error(errMessage, e);
                return false;
            }
            catch (Exception e)
            {
                string errMessage = "An unexpected error occurred - please contact system administrator or try again later";
                errorModel.AddErrorMessage(errMessage);
                log.Error(errMessage, e);
                return false;
            }
        }

        /// <summary>
        /// validates the access code used to reset a user's password
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accessCode"></param>
        /// <returns></returns>
        public bool ValidateAccessCode(Guid userId, string accessCode, UnitOfWork unitOfWork)
        {
            return unitOfWork.PasswordRecoveryRequest.DoesAccessCodeExistForUser(userId, accessCode);
        }
    }
}