//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SS.Models
{
    using System;
    using System.Collections.Generic;
    using System.Web.Script.Serialization;

    public partial class Message
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Message()
        {
            this.MessageTrash = new HashSet<MessageTrash>();
        }
    
        public System.Guid ID { get; set; }
        public System.Guid To { get; set; }
        public System.Guid From { get; set; }
        public string Msg { get; set; }
        public bool Seen { get; set; }
        public System.DateTime DateTCreated { get; set; }
        public System.Guid ThreadId { get; set; }

        [ScriptIgnore(ApplyToOverrides = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MessageTrash> MessageTrash { get; set; }
    }
}
