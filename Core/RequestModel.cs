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
        public bool HasBool;
        public bool ReturnedBool;
        public List<String> ReturnedMessages;
        public List<String> ErrorMessages;

        public RequestModel()
        {
            this.ReturnedMessages = new List<string>();
            this.ErrorMessages = new List<string>();
            this.HasMessage = false;
            this.HasErrors = false;
            this.HasBool = false;
        }

        public void AddBool(bool returnedBool)
        {
            this.HasBool = true;
            this.ReturnedBool = returnedBool;
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