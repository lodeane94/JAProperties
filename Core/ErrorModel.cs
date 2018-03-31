using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Core
{
    public class ErrorModel
    {
        private bool hasErrors;
        private List<String> ErrorMessages;

        public ErrorModel()
        {
            this.ErrorMessages = new List<string>();
            hasErrors = false;
        }

        public void AddErrorMessage(string message)
        {
            hasErrors = true;
            this.ErrorMessages.Add(message);
        }

        public void AddErrorMessages(IEnumerable<string> messages)
        {
            hasErrors = true;
            this.ErrorMessages.AddRange(messages);
        }

        public List<String> GetErrorMessages()
        {
            return this.ErrorMessages;
        }

        public bool GetHasErrors()
        {
            return this.hasErrors;
        }
    }
}