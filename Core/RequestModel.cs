using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Core
{
    public class RequestModel
    {
        public bool HasMessage;
        public bool HasErrors;
        public List<String> ReturnedMessages;
        public List<String> ErrorMessages;

        public RequestModel()
        {
            this.ReturnedMessages = new List<string>();
            this.ErrorMessages = new List<string>();
            HasMessage = false;
        }

        public void AddMessage(string message)
        {
            HasMessage = true;
            this.ReturnedMessages.Add(message);
        }

        public void AddMessages(IEnumerable<string> messages)
        {
            HasMessage = true;
            this.ReturnedMessages.AddRange(messages);
        }

        public void AddErrorMessage(string message)
        {
            HasErrors = true;
            this.ErrorMessages.Add(message);
        }

        public void AddErrorMessages(IEnumerable<string> messages)
        {
            HasErrors = true;
            this.ErrorMessages.AddRange(messages);
        }
    }
}