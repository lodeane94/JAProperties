using SS.Code;
using SS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SS.Controllers
{
    public class EnrolmentController : Controller
    {
        [HttpPost]
        public ActionResult Enroll(TENNANTS tennants, string password, string confirmedPassword, string enrolmentKey,bool university_student)
        {/*
            RequisitionInformation rinfo = (RequisitionInformation) Session["requesteeInfo"];
            //checking if the permission was given by the landlord for the user to enroll into room
            JAHomesEntities dbCtx = new JAHomesEntities();

            string eK = dbCtx.ACCOMMODATIONS.Where(ek => ek.ID.ToString() == rinfo.ID)
                                    .Select(ek => ek.ENROLMENT_KEY).Single();
            if (eK.Equals(enrolmentKey))
            {
                bool isAccepted = dbCtx.REQUISITIONS.Where(a => a.ACCOMMODATION_ID.ToString() == rinfo.ID && a.CELL == rinfo.Cell)
                            .Select(a => a.ACCEPTED).Single();
                if (isAccepted)
                {//checking password equality
                    if (password.Equals(confirmedPassword, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //creating new user as a tennant
                        MembershipCreateStatus isCreated;
                        MembershipUser newUser = Membership.CreateUser(rinfo.Email, password, rinfo.Email, "null", "null", true, out isCreated);

                        if (newUser != null)
                        {
                            Roles.AddUserToRole(rinfo.Email, "Tennants");
                            //if user is not a student set the related fields to null
                            if (!university_student)
                            {
                                tennants.UNIVERSITY = "";
                                tennants.PROGRAMME_NAME = "";
                                tennants.PROGRAMME_START_DATE = DateTime.MinValue;
                                tennants.PROGRAMME_END_DATE = DateTime.MinValue;
                            }
                        //inserting information in tennants table
                        dbCtx.sp_insert_tennants(rinfo.ID, rinfo.FirstName, rinfo.LastName, rinfo.Gender,
                                Convert.ToDateTime(tennants.DOB), tennants.STREET_ADDRESS, tennants.CITY, tennants.PARISH,
                                rinfo.Cell, rinfo.Email, tennants.UNIVERSITY, tennants.PROGRAMME_NAME, tennants.PROGRAMME_START_DATE,
                                tennants.PROGRAMME_END_DATE, tennants.SETTLEMENT_PERIOD, tennants.DESCRIPTION);
                        }
                    }
                }
            }*/
            return RedirectToAction("Home","Home");
        }

        [HttpGet]
        public ActionResult Requisition(string accID, string fname, string lname, string gender, string email, string cell)
        {
            RequisitionInformation rinfo = new RequisitionInformation()
            {
                ID = accID,
                FirstName = fname,
                LastName = lname,
                Gender = gender,
                Email = email,
                Cell = cell
            };

            Session["requesteeInfo"] = rinfo;

            return View();
        }
    }
}