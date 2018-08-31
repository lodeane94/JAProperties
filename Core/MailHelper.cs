using log4net;
using SS.Code;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using static SS.Core.EFPConstants;

namespace SS.Core
{
    public class MailHelper
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<String> templateParams;

        private String emailTo { get; }
        private String subject { get; }
        private String body { get; }
        private String firstName { get; }

        public MailHelper(String emailTo, String subject, String body, String fName)
        {
            this.emailTo = emailTo;
            this.subject = subject;
            this.body = body;
            this.firstName = fName;

            templateParams = new List<string>();
            templateParams.Add(EmailTmplParams.Title);
            templateParams.Add(EmailTmplParams.Fname);            
            templateParams.Add(EmailTmplParams.Body);
        }

        /// <summary>
        /// sends email from the application server
        /// </summary>
        /// <param name="emailTo"></param>
        /// <param name="body"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public bool SendMail()
        {
            try
            {
                MailModel mailModel = new MailModel()
                {
                    To = emailTo,
                    Subject = subject,
                    From = "JProps@jprops.net",
                    Body = GenerateHTML(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/EmailTemplate.html")).ToString()
                };

                //setting mail requirements
                MailMessage mail = new MailMessage();
                mail.To.Add(mailModel.To);
                mail.From = new MailAddress(mailModel.From);
                mail.Subject = mailModel.Subject;
                mail.Body = mailModel.Body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.jprops.net";
                smtp.Port = 25;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("jprops@jprops.net", "Gihtiwm88*");
                smtp.EnableSsl = false;
                //throw new Exception("trst");
                smtp.Send(mail);

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Error encountered while sending email", ex);

                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Generates the Html file that will be sent out in the 
        /// email
        /// </summary>
        /// <param name="htmlFileNameWithPath"></param>
        /// <returns></returns>
        public StringBuilder GenerateHTML(String htmlTemplatePath)
        {
            StringBuilder htmlContent = new StringBuilder();
            String line;
            try
            {
                using (StreamReader htmlReader = new StreamReader(htmlTemplatePath))
                {
                    while ((line = htmlReader.ReadLine()) != null)
                    {
                        htmlContent.Append(fillTemplateHolders(line));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while generating html for email", ex);
                throw ex;
            }

            return htmlContent;
        }

        /// <summary>
        /// Fills the html template with the actual parameter values
        /// One parameter per line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private String fillTemplateHolders(String line)
        {
            Regex rgx = null;

            foreach (var param in templateParams)
            {
                if (line.Contains(param) && param == EmailTmplParams.Title)
                {
                    rgx = new Regex(param);
                    return rgx.Replace(line, this.subject);
                }
                else if (line.Contains(param) && param == EmailTmplParams.Fname)
                {
                    rgx = new Regex(param);
                    return rgx.Replace(line, this.firstName);
                }
                else if (line.Contains(param) && param == EmailTmplParams.Body)
                {
                    rgx = new Regex(param);
                    return rgx.Replace(line, this.body);
                }
            }

            return line;
        }


    }
}