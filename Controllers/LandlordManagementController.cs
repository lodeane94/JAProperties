using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SS.Models;
using SS.Code;
using System.Net.Mail;
using System.IO;
using System.Web.Script.Serialization;
using System.Collections;
using Newtonsoft.Json;
using static SS.Code.PropertyConstants.PropertyType;
using SS.Core;
using SS.ViewModels;
using SS.SignalR;

namespace SS.Controllers
{
    [Authorize]
    public class LandlordManagementController : Controller
    {
        //loads the help page
        public ActionResult Help()
        {
            return View();
        }
        //loads dashboard page
        public ActionResult Dashboard()
        {
            try
            {
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    if (HttpContext.User.Identity.Name != null)
                    {
                        UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                        /*
                         * upon entrance of the dashboard, put the id of the current signed in user into a session
                         * so that it may be used if a new property is being added*/

                        var userId = dbCtx.User.Where(x => x.Email == HttpContext.User.Identity.Name)
                                                        .Select(x => x.ID).Single();
                        Session["userId"] = userId;
                        ViewBag.userId = userId;
                    }
                }
            }
            catch (Exception ex)
            { }

            return View();
        }

        [HttpPost]
        public ActionResult AcceptRequest(Guid reqID)
        {
            try
            {
                //retrieving enrolment key that is associated with an accommodation
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    var requisition = unitOfWork.PropertyRequisition.Get(reqID);
                    var reqUser = requisition.User;

                    var property = requisition.Property;
                    var propertyUser = property.Owner.User;
                    var propCategoryCode = property.CategoryCode;
                    var adTypeCode = property.AdTypeCode;

                    //email address which acceptance letter should be sent to
                    string emailTo = reqUser.Email;
                    string subject = "EasyFindProperties - Property Requisition Accepted";
                    //body of the email
                    string body = string.Empty;
                    bool isRealEstateLeasedOrRent = false;

                    if (propCategoryCode.Equals(EFPConstants.PropertyCategory.RealEstate)
                        &&
                        (adTypeCode.Equals(EFPConstants.PropertyAdType.Rent)
                        || adTypeCode.Equals(EFPConstants.PropertyAdType.Lease)))
                    {
                        //generated key that is used to associate each tennant to their landlord
                        string enrolmentKey = property.EnrolmentKey;

                        body = "Congratulations!!, your property request was accepted ." + "Your enrolment key is " + enrolmentKey +
                                    ". " + "If you wish to accommodate this room, please click on the following link and enter your email address and the enrolment key that was provided to you" +
                                  " localhost:5829/enrolment/requisition/?propId=" + property.ID + "&requestId=" + reqID;

                        isRealEstateLeasedOrRent = true;
                    }
                    else
                        body = "Congratulations!!, your property request was accepted. The property owner will contact you if there"
                                + " is further negotiations or concerns.";

                    //getting information about the owner of the property to give back to the requestee
                    body += "<br/> Owner Information<br/> First Name:&nbsp;" + propertyUser.FirstName + "<br/>Last Name:&nbsp;" + propertyUser.LastName
                                    + "<br/>Cellphone Number:&nbsp;" + propertyUser.CellNum + "<br/>Email:&nbsp;" + propertyUser.Email;

                    if (sendMail(emailTo, body, subject))
                    {
                        //sets the accepted field of the requisition table to true for the accepted property request
                        requisition.IsAccepted = true;
                        unitOfWork.save();
                        //message outputted if request was accepted successfully
                        Session["acceptedRequestCheck"] = "Request has been successfully accepted";
                    }
                    else
                        throw new Exception("Mail Exception");
                }
            }
            catch (Exception ex)
            {
                //message outputted if request acceptance has failed
                Session["acceptedRequestCheck"] = "An error has occurred while accepting the request. Please contact site administrator";

                return Content("RequestFailed");
            }

            return Content("RequestSuccess");
        }

        /*denies user's requisition
        [HttpPost]
        [Authorize]
        public ActionResult cancelRequest(Guid reqID, string cell, string email)
        {
            REQUISITIONS requisition;
            REQUISITION_PROPERTY_MAPPINGS requisitionMapping;

            string body = "The owner of the property have declined your requisition";
            string subject = "Property Requisition Declined";

            try
            {
                using (JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities())
                {
                    //if (sendMail(email, body, subject))
                    //{
                    //removes requisition from the database
                    requisition = dbCtx.REQUISITIONS.Single(x => x.REQUISITION_ID == reqID);
                    requisitionMapping = dbCtx.REQUISITION_PROPERTY_MAPPINGS.Single(x => x.ID == reqID);

                    dbCtx.REQUISITIONS.Remove(requisition);
                    dbCtx.REQUISITION_PROPERTY_MAPPINGS.Remove(requisitionMapping);

                    dbCtx.SaveChanges();
                    Session["cancelRequestCheck"] = "The request has been cancelled";
                    // }
                    // else
                    //   throw new Exception("Mail Exception");
                }
            }
            catch (Exception ex)
            { Session["cancelRequestCheck"] = "An error has occurred while accepting the request. Please contact site administrator"; }

            return Content("Request Cancelled");
        }

        [HttpPost]
        [Authorize]
        public ActionResult removeProperty(Guid id)
        {
            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();
            ACCOMMODATIONS accommodation;
            HOUSE house;
            LAND land;

            try
            {
                switch (PropertiesDAO.getPropertyType(id))
                {
                    case PropertyConstants.PropertyType.accommodation:
                        accommodation = dbCtx.ACCOMMODATIONS.Single(x => x.ID == id);
                        dbCtx.ACCOMMODATIONS.Remove(accommodation);
                        break;
                    case PropertyConstants.PropertyType.house:
                        house = dbCtx.HOUSE.Single(x => x.ID == id);
                        dbCtx.HOUSE.Remove(house);
                        break;
                    case PropertyConstants.PropertyType.land:
                        land = dbCtx.LAND.Single(x => x.ID == id);
                        dbCtx.LAND.Remove(land);
                        break;
                }

                dbCtx.SaveChanges();
                Session["propertyRemovedMesssage"] = "Property removed successfully";
            }
            catch (Exception ex)
            {
                Session["propertyRemovedMesssage"] = "There was an error removing your property";
            }

            return Content("");
        }
        */
        public bool sendMail(string emailTo, string body, string subject)
        {
            MailModel mailModel = new MailModel()
            {
                To = emailTo,
                Subject = subject,
                From = "jamprops@hotmail.com",
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
            smtp.Host = "smtp-mail.outlook.com";
            smtp.Port = 587;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential("jamprops@hotmail.com", "Daveyot88*");
            smtp.EnableSsl = true;
            smtp.Send(mail);

            return true;
        }

        //returns all properties owned by the current user that is signed in in json format
        [Authorize]
        public JsonResult getAllPropertyImages()
        {
            ErrorModel errorModel = new ErrorModel();

            IEnumerable<PropertyImage> propertyImage;
            List<IEnumerable> pImageInfo = new List<IEnumerable>();
            try
            {
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                    //checking if the landlord id was saved in the session
                    if (Session["userId"] != null)
                    {
                        var userId = (Guid)Session["userId"];
                        var owner = unitOfWork.Owner.GetOwnerByUserID(userId);
                        propertyImage = unitOfWork.PropertyImage.GetAllPrimaryPropertyImageByOwnerId(owner.ID);

                        foreach (var image in propertyImage)
                        {
                            //adding properties to dictionary to display image to the user
                            Dictionary<String, String> imageInfo = new Dictionary<string, string>();
                            imageInfo.Add("propertyID", image.PropertyID.ToString());
                            imageInfo.Add("imageURL", image.ImageURL);
                            pImageInfo.Add(imageInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorModel = MiscellaneousHelper.PopulateErrorModel(null);

                return Json(errorModel, JsonRequestBehavior.AllowGet);
            }

            return Json(pImageInfo, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// returns latest 5 messages for a specific user
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        //
        public JsonResult getMessages()
        {
            List<MessageViewModel> messagesViewModel = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];

                    var messages = unitOfWork.Message.GetMsgsForUserID(userId);
                    messagesViewModel = new List<MessageViewModel>();

                    foreach (var msg in messages)
                    {
                        var user = unitOfWork.User.Get(msg.From);

                        MessageViewModel messageViewModel = new MessageViewModel()
                        {
                            ID = msg.ID,
                            From = user.FirstName + " " + user.LastName,
                            CellNum = user.CellNum,
                            Email = user.Email,
                            Msg = msg.Msg,
                            Seen = msg.Seen,
                            DateTCreated = msg.DateTCreated.ToShortDateString()
                        };

                        messagesViewModel.Add(messageViewModel);
                    }
                }
            }

            return Json(messagesViewModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the seen column on the selected message
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="id"></param>
        public void updateMsgSeen(UnitOfWork unitOfWork, Guid id)
        {
            var message = unitOfWork.Message.Get(id);

            if (!message.Seen)
            {
                message.Seen = true;
                unitOfWork.save();
            }
        }

        /// <summary>
        /// removes the selected message from the system
        /// </summary>
        /// <param name="id"></param>
        [HttpGet]
        public void deleteMsg(Guid id)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if ((Guid)Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];
                    var userTo = unitOfWork.User.Get(userId);

                    var message = unitOfWork.Message.Get(id);

                    if (message != null)
                    {
                        MessageTrash messageTrash = new MessageTrash()
                        {
                            UserID = userId,
                            MessageID = id,
                            DateTCreated = DateTime.Now
                        };

                        unitOfWork.MessageTrash.Add(messageTrash);
                        unitOfWork.save();

                        //broadcast the new messages to the recipient 
                        DashboardHub.BroadcastUserMessages(userTo.Email);
                    }
                }
            }
        }

        /// <summary>
        /// Get messages in the proper order both user and sender based on the msg selected
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult getMsgThread(Guid id)
        {
            //using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            // {
            EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities();
            UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
            IEnumerable<Message> messages = null;

            if ((Guid)Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];

                updateMsgSeen(unitOfWork, id);
                messages = unitOfWork.Message.GetMsgThreadByMsgID(id, userId);
            }

            return Json(messages, JsonRequestBehavior.AllowGet); ;
            //  }
        }

        /// <summary>
        /// replies to the message
        /// </summary>
        /// <param name="id"></param>
        [HttpGet]
        public int sendMsg(Guid id, String msg)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var message = unitOfWork.Message.Get(id);
                Guid userId;

                if ((Guid)Session["userId"] != null && message != null)
                {
                    userId = (Guid)Session["userId"];
                    var userTo = unitOfWork.User.Get(message.From);

                    Message newMsg = new Message()
                    {
                        ID = Guid.NewGuid(),
                        To = message.From,
                        From = userId,
                        Msg = msg,
                        Seen = false,
                        DateTCreated = DateTime.Now
                    };

                    unitOfWork.Message.Add(newMsg);
                    unitOfWork.save();

                    //broadcast the new messages to the recipient 
                    DashboardHub.BroadcastUserMessages(userTo.Email);
                    return 0;
                }
            }

            return 1;
        }
        /*
        /*
        //loads all bills for the current user
        [Authorize]
        public JsonResult getAllBills()
        {
            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();

            List<BillsModel> billsList = new List<BillsModel>();

            var landlordID = (Guid)Session["landlord_id"];

            var bills = dbCtx.BILLS.Where(b => b.ACCOMMODATIONS.OWNER == landlordID)
                            .Select(b => new { b.B_AMOUNT, b.B_TYPE, b.BILL_URL, b.DATE_DUE, b.DATE_ISSUED, b.DESCRIPTION, b.ID });

            foreach (var bill in bills)
            {
                BillsModel billsModel = new BillsModel();

                billsModel.BAmount = bill.B_AMOUNT.ToString();
                billsModel.BType = bill.B_TYPE;
                billsModel.DateDue = bill.DATE_DUE.ToString();
                billsModel.DateIssued = bill.DATE_ISSUED.ToString();
                billsModel.Description = bill.DESCRIPTION;
                billsModel.ID = bill.ID.ToString();

                billsList.Add(billsModel);
            }

            return Json(billsList, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult BillSubmission(string bill_type, DateTime date_issued, DateTime date_due, decimal bill_amount,
                                            string tennats_message, HttpPostedFileBase bill_image, Guid[] room_selection)
        {
            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();

            for (int count = 0; count < room_selection.Length; count++)
            {
                dbCtx.sp_insert_bill(bill_type, bill_amount, tennats_message, date_issued, date_due, room_selection[count]);

                uploadBillPicture(bill_image, room_selection[count]);
            }

            ViewBag.BillRegistered = true;

            return RedirectToAction("Dashboard");
        }
        //updates bill picture url
        [HttpPost]
        [Authorize]
        public void uploadBillPicture(HttpPostedFileBase file, Guid room_selection)
        {
            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();

            string fileName = string.Empty;
            //ensures file is not empty and is a valid image
            if (file.ContentLength > 0 && file.ContentType.Contains("image"))
            {
                fileName = Path.GetFileName(file.FileName);

                string path = Path.Combine(Server.MapPath("~/Uploads/UtilityBills"), fileName);

                dbCtx.sp_update_bill_pic_url(room_selection, fileName);

                file.SaveAs(path);
            }
        }*/
        /// <summary>
        /// returns requition information for the owner
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RequisitionViewModel> getRequisitions()
        {
            List<RequisitionViewModel> requisitionInfo = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];

                    var owner = unitOfWork.Owner.GetOwnerByUserID(userId);
                    var requisitions = unitOfWork.PropertyRequisition.GetRequestsByOwnerId(owner.ID);

                    if (requisitions != null)
                    {
                        requisitionInfo = new List<RequisitionViewModel>();

                        foreach (var req in requisitions)
                        {
                            RequisitionViewModel model = new RequisitionViewModel();

                            model.PropertyRequisition.User = new User();

                            model.ImageUrl = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(req.PropertyID);
                            model.PropertyRequisition.PropertyID = req.PropertyID;
                            model.PropertyRequisition.User.FirstName = req.User.FirstName;
                            model.PropertyRequisition.User.LastName = req.User.LastName;
                            model.PropertyRequisition.User.Email = req.User.Email;
                            model.PropertyRequisition.User.CellNum = req.User.CellNum;
                            model.PropertyRequisition.Msg = req.Msg;
                            model.PropertyRequisition.IsAccepted = req.IsAccepted;
                            model.PropertyRequisition.DateTCreated = req.DateTCreated;

                            requisitionInfo.Add(model);
                        }
                    }
                }
            }

            return requisitionInfo;
        }

        /// <summary>
        /// returns the messages partial view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMessagesView()
        {
            return PartialView("_Messages");
        }

        /// <summary>
        /// returns the requisition partial view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetRequisitionsView()
        {
            var requisitions = getRequisitions();
            return PartialView("_Requisitions", requisitions);
        }

        /*
         * gets the information for the property that was selected in order to
         * update information about this property
         
       
        [Authorize]
        public ActionResult updateAccommodation(Guid id, bool availability, bool cable, bool electricity, bool gas, bool internet, bool water, decimal security_deposit,
                                            string terms_agreement, decimal price, short occupancy, string gender_preference, string description)
        {
            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();

            List<ACCOMMODATIONS> accommodation = (from x in dbCtx.ACCOMMODATIONS
                                                  where x.ID == id
                                                  select x).ToList();

            foreach (ACCOMMODATIONS prop in accommodation)
            {
                prop.PRICE = price;
                prop.SECURITY_DEPOSIT = security_deposit;
                prop.OCCUPANCY = occupancy;
                prop.GENDER_PREFERENCE = gender_preference;
                prop.DESCRIPTION = description;
                prop.WATER = water;
                prop.ELECTRICITY = electricity;
                prop.CABLE = cable;
                prop.GAS = gas;
                prop.INTERNET = internet;
                prop.AVAILABILITY = availability;
                prop.TERMS_AGREEMENT = terms_agreement;
            }
            //dbCtx.sp_update_accommodations(id, price, security_deposit, occupancy, gender_preference, description, water, electricity, cable, gas, internet, availability, terms_agreement);
            try
            {
                dbCtx.SaveChanges();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                Session["propertyUpdateFailure"] = "Property was not updated successfully";
                return RedirectToAction("Dashboard");
            }

            Session["propertyUpdateSuccess"] = "Property was updated successfully";

            return RedirectToAction("Dashboard");
        }
        [Authorize]
        public ActionResult updateHouse(Guid id, decimal price, string description, string purpose, bool isFurnished)
        {
            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();

            List<HOUSE> house = (from x in dbCtx.HOUSE
                                 where x.ID == id
                                 select x).ToList();

            foreach (HOUSE prop in house)
            {
                prop.PURPOSE = purpose;
                prop.PRICE = price;
                prop.DESCRIPTION = description;
                prop.ISFURNISHED = isFurnished;
            }

            //dbCtx.sp_update_house(id, purpose, price, isFurnished, description);
            try
            {
                dbCtx.SaveChanges();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                Session["propertyUpdateFailure"] = "Property was not updated successfully";
                return RedirectToAction("Dashboard");
            }

            Session["propertyUpdateSuccess"] = "Property was updated successfully";

            return RedirectToAction("Dashboard");

        }
        [Authorize]
        public ActionResult updateLand(Guid id, decimal price, string purpose, string area, string description)
        {
            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();

            List<LAND> land = (from x in dbCtx.LAND
                               where x.ID == id
                               select x).ToList();

            foreach (LAND prop in land)
            {
                prop.PURPOSE = purpose;
                prop.PRICE = price;
                prop.DESCRIPTION = description;
                prop.AREA = Decimal.Parse(area);
            }
            //dbCtx.sp_update_land(id, purpose, price, area, description);
            try
            {
                dbCtx.SaveChanges();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                Session["propertyUpdateFailure"] = "Property was not updated successfully";
                return RedirectToAction("Dashboard");
            }

            Session["propertyUpdateSuccess"] = "Property was updated successfully";

            return RedirectToAction("Dashboard");

        }*/

    }

}


