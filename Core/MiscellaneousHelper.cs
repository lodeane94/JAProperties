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
    }
}