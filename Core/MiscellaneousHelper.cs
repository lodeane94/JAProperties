using SS.Models;
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
            errorModel.ErrorMessages = new List<string>();

            errorModel.hasErrors = true;
            errorModel.ErrorMessages.Add("An unexpected error occurred");

            if (modelStateErrorMsgs != null)
            {
                var modelStateErrors = modelStateErrorMsgs.Values.SelectMany(m => m.Errors);
                errorModel.ErrorMessages.AddRange(modelStateErrors.Select(x => x.ErrorMessage));
            }

            if(addtlErrorMsgs !=null) errorModel.ErrorMessages.AddRange(addtlErrorMsgs.Select(x => x.ToString()));

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
    }
}