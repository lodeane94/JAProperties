using log4net;
using SS.Core;
using SS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Services
{
    public class MailingService
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MailingService()
        { }

        /// <summary>
        /// Create the email that is send to alert a property owner
        /// that a message / request came in
        /// </summary>
        /// <param name="req"></param>
        /// <param name="isRequisition"></param>
        /// <returns></returns>
        public string CreateRequestEmail(User user, bool isRequisition)
        {
            String body = String.Empty;

            if (isRequisition)
            {
                body = "<p>" + user.FirstName + " " + user.LastName + " is requesting your property.</p> ";
                body += "<p>Click the following link to go to your portal ";
                body += "http://www." + EFPConstants.Application.Host + "/landlordmanagement/dashboard</p>";
            }
            else
            {
                body = "</p>" + user.FirstName + " " + user.LastName + " sent a message to you regarding your property.</p> ";
                body += "Click the following link to go to your portal ";
                body += "http://www." + EFPConstants.Application.Host + "/landlordmanagement/dashboard</p>";
            }

            return body;
        }

        /// <summary>
        /// Creates the email that notifies the user that their requisition was accepted
        /// </summary>
        /// <param name="propertyUser"></param>
        /// <returns></returns>
        public MailHelper CreateRequisitionAcceptedEmail(User propertyUser, User reqUser)
        {
            //email address which acceptance letter should be sent to
            string emailTo = reqUser.Email;
            string subject = "JProps - Property Requisition Accepted";
            string body = string.Empty;

            body = "Congratulations!!, your property request was accepted. Please make the necessary communication with the property owner to acquire any additional information "
                        + "to secure your reservation. Thank you for using JProps.";

            //getting information about the owner of the property to give back to the requestee
            body += "<br/><br/><strong>Property Owner Information</strong><br/><br/> First Name:&nbsp;"
                + propertyUser.FirstName + "<br/><br/>Last Name:&nbsp;" + propertyUser.LastName
                            + "<br/><br/>Cellphone Number:&nbsp;" + propertyUser.CellNum + "<br/><br/>Email:&nbsp;" + propertyUser.Email;

            return new MailHelper(emailTo, subject, body, reqUser.FirstName);
        }

        /// <summary>
        /// Creates the email that notifies the user that their requisition was denied
        /// </summary>
        /// <param name="requisition"></param>
        /// <param name="ownerUser"></param>
        /// <param name="isUserPropOwner"></param>
        /// <returns></returns>
        public MailHelper CreateRequisitionCancelledEmail(PropertyRequisition requisition, User ownerUser, bool isUserPropOwner)
        {
            string body = String.Empty;
            string subject = String.Empty;
            string emailTo = String.Empty;

            if (isUserPropOwner)
            {
                body = "<p> We are sorry to advise you that " +
                        " the owner ( " + ownerUser.FirstName + " " + ownerUser.LastName + " ) of the property have declined your requisition." +
                        " Please feel free to request more properties. <br /> http://www." + EFPConstants.Application.Host + "</p>";

                subject = "JProps - Property Requisition Declined";
                emailTo = requisition.User.Email;
            }
            else
            {
                body = "<p> The property requisition for (" + requisition.User.FirstName + " " + requisition.User.LastName + " ) has been cancelled. </p>";
                subject = "JProps - Property Requisition Cancelled";
                emailTo = requisition.Property.Owner.User.Email;
            }

            return new MailHelper(emailTo, subject, body, requisition.User.FirstName);
        }
    }
}