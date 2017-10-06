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
    
    public partial class Tennant
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tennant()
        {
            this.Bill = new HashSet<Bill>();
            this.Complaint = new HashSet<Complaint>();
        }
    
        public System.Guid ID { get; set; }
        public System.Guid PropertyID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> DOB { get; set; }
        public string CellNum { get; set; }
        public decimal RentAmt { get; set; }
        public int SettlementPeriod { get; set; }
        public string InstitutionName { get; set; }
        public string ProgrammeName { get; set; }
        public Nullable<System.DateTime> ProgrammeStartDate { get; set; }
        public Nullable<System.DateTime> ProgrammeEndDate { get; set; }
        public string PhotoUrl { get; set; }
        public string ReferencedLetterURL { get; set; }
        public System.DateTime DateTCreated { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Bill> Bill { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Complaint> Complaint { get; set; }
        public virtual Property Property { get; set; }
    }
}