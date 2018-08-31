using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.ViewModels
{
    public class EnrolmentViewModel
    {
       /* public DateTime programmeStartDate { get; set; }
        public DateTime programmeEndDate { get; set; }
        public DateTime dOB { get; set; }
        */
        public Guid ReqID { get; set; }
        public Guid PropertyID { get; set; }
        public String EnrolmentKey { get; set; }
        public int SettlementPeriod { get; set; }
        public string InstitutionName { get; set; }
        public string ProgrammeName { get; set; }
        public String ProgrammeStartDate { get; set; }
        /*{
            get { return this.ProgrammeStartDate; }
            set
            {
                programmeStartDate = DateTime.Parse(value);
            }
        }*/
        public String ProgrammeEndDate { get; set; }
        /* {
             get { return this.ProgrammeEndDate; }
             set
             {
                 programmeEndDate = DateTime.Parse(value);
             }
         }*/
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CellNum { get; set; }
        public string AreaCode { get; set; }
        public string Email { get; set; }
        public String DOB { get; set; }
        /*  {
              get { return this.DOB; }
              set
              {
                  dOB = DateTime.Parse(value);
              }
          }*/
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string PhotoUrl { get; set; }
        public string ReferencedLetterURL { get; set; }
    }
}