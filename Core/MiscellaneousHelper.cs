using SS.Models;
using SS.ViewModels.WebInfo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SS.Core
{
    public static class MiscellaneousHelper
    {
        /// <summary>
        /// Populates the error model to be returned
        /// </summary>
        /// <param name="modelStateErrorMsgs"></param>
        /// <param name="addtlErrorMsgs"></param>
        /// <returns>ErrorModel</returns>
        public static ErrorModel PopulateErrorModel(ModelStateDictionary modelStateErrorMsgs, List<String> addtlErrorMsgs = null)
        {
            ErrorModel errorModel = new ErrorModel();
            
            errorModel.AddErrorMessage("An unexpected error occurred");

            if (modelStateErrorMsgs != null)
            {
                var modelStateErrors = modelStateErrorMsgs.Values.SelectMany(m => m.Errors);
                errorModel.AddErrorMessages(modelStateErrors.Select(x => x.ErrorMessage));
            }

            if(addtlErrorMsgs !=null) errorModel.AddErrorMessages(addtlErrorMsgs.Select(x => x.ToString()));

            return errorModel;
        }

        /// <summary>
        /// In the event the application has been stopped however user
        /// was still signed in, then reload this user, userid into a session variable
        /// </summary>
        public static Guid getLoggedInUser()
        {
            try
            {
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    if (HttpContext.Current.User.Identity.Name != null)
                    {
                        UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                        var userId = dbCtx.User.Where(x => x.Email == HttpContext.Current.User.Identity.Name)
                                                        .Select(x => x.ID).Single();
                        return userId;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to restore user's session");
            }

            return new Guid();
        }
        /// <summary>
        /// Return first character of a string as uppercase
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static String UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        /// <summary>
        /// Sends an email to the admin's email address
        /// </summary>
        /// <param name="model"></param>
        public static void SendMessageToAdmin(ContactFormViewModel model, RequestModel requestModel)
        {
            User adminUser = null;
            String msg = String.Empty;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                adminUser = unitOfWork.User.GetUserByEmail(EFPConstants.Admin.Email);
            }

            if (adminUser == null)
            {
                msg = "Admin user's email could not be found - Submission failed";
                requestModel.AddErrorMessage(msg);
                throw new Exception(msg);
            }
            else
            {
                string subject = "JProps - " + model.Subject;
                string body = model.Message;

                MailHelper mail = new MailHelper(adminUser.Email, subject, body, adminUser.FirstName);

                if (mail.SendMail())
                {
                    msg = "Your message was successfully sent to the system adminstrator";
                    requestModel.AddMessage(msg);
                }
                else
                {
                    msg = "An unexpected error occurred while sending your message - Please try again later";
                    requestModel.AddErrorMessage(msg);
                    throw new Exception(msg);
                }
            }            
        }
    }
}