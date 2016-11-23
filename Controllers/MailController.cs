using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using SS.Code;

namespace SS.Controllers
{
    public class MailController : Controller
    {
        [HttpPost]
        public ActionResult SendRequisitionMail(string emailTo)
        {
            string body = "Congratulations, you have been accepted into an accommodation." + "Your enrolment key is " +
                            "" + "If you wish to accommodate this room, please click on the following link and enter your email address and the enrolment key that was provided to you" +
                          "jahomes/enrolment/enroll";

            MailModel mailModel = new MailModel()
            {
                To = emailTo,
                Subject = "Accommodation Accepted By Owner",
                From = "info@jahomes.com",
                Body = body
            };

            //setting mail requirements
            MailMessage mail = new MailMessage();
            mail.To.Add(mailModel.To);
            mail.From = new MailAddress(mailModel.From);
            mail.Subject = mailModel.Subject;
            mail.Body = mailModel.Body;
            mail.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential("lodeanekelly@gmail.com", "JmIlUKelly1997");
            smtp.EnableSsl = true;
            smtp.Send(mail);


            return View();
        }
    }
}