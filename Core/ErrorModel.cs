using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Core
{
    public class ErrorModel
    {
        public bool hasErrors { get; set; } = false;
        public List<String> ErrorMessages;
    }
}