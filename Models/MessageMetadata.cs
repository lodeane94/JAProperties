using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace SS.Models
{
    public class MessageMetadata
    {
        [ScriptIgnore(ApplyToOverrides = true)]
        public virtual ICollection<MessageTrash> MessageTrash { get; set; }
    }
}