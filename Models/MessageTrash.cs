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
    
    public partial class MessageTrash
    {
        public System.Guid UserID { get; set; }
        public System.Guid MessageID { get; set; }
        public System.DateTime DateTCreated { get; set; }
    
        public virtual Message Message { get; set; }
        public virtual User User { get; set; }
    }
}
