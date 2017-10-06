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

namespace SS.Controllers
{
    public class LandlordManagementController : Controller
    {
        //loads the help page
        [Authorize]
        public ActionResult Help()
        {
            return View();
        }
        //loads dashboard page
        /*[Authorize]
        public ActionResult Dashboard()
        {
            try
            {
                using (JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities())
                {
                    if (HttpContext.User.Identity.Name != null)
                    {
                        /*
                         * upon entrance of the dashboard, put the id of the current signed in user into a session
                         * so that it may be used if a new property is being added
                         
                        var landlordID = dbCtx.LANDLORDS.Where(l => l.USERNAME == HttpContext.User.Identity.Name)
                                                        .Select(l => l.ID).Single();
                        Session["landlord_id"] = landlordID;
                        //isAdditionalProperty is a session variable that is used to detect whenever an additional property is being added
                        Session["isAdditionalProperty"] = true;
                        /*
                         * setting the viewbags in order to load contents specific to users based on the different properties
                         * they have
                         
                        ViewBag.hasAccommodation = 0;
                        ViewBag.hasHouse = 0;
                        ViewBag.hasLand = 0;

                        ViewBag.hasAccommodation = dbCtx.ACCOMMODATIONS.Where(a => a.OWNER == landlordID).Count();
                        ViewBag.hasHouse = dbCtx.HOUSE.Where(a => a.OWNER == landlordID).Count();
                        ViewBag.hasLand = dbCtx.LAND.Where(a => a.OWNER == landlordID).Count();
                    }
                }
            }
            catch (Exception ex)
            { }

            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult AcceptRequest(Guid reqID, RequisitionInformation requestInfo)
        {
            try
            {
                //retrieving enrolment key that is associated with an accommodation
                using (JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities())
                {
                    /*
                     * used to check whether the property is a room or otherwise
                     * a different email message will be generated depending on the property requested
                     
                    //bool isRoomRequest = dbCtx.ACCOMMODATIONS.All(r => r.ID == propertyID);
                    //email address which acceptance letter should be sent to
                    string emailTo = requestInfo.Email;
                    string subject = "Property Requisition Accepted";
                    //body of the email
                    string body = string.Empty;
                    /*
                    if (isRoomRequest)
                    {
                        //generated key that is used to associate each tennant to their landlord
                        string enrolmentKey = dbCtx.ACCOMMODATIONS.Where(ek => ek.ID == propertyID)
                                    .Select(ek => ek.ENROLMENT_KEY).Single();

                        body = "Congratulations!!, your property request was accepted ." + "Your enrolment key is " + enrolmentKey +
                                    ". " + "If you wish to accommodate this room, please click on the following link and enter your email address and the enrolment key that was provided to you" +
                                  " localhost:5829/enrolment/requisition/?accID=" + propertyID + "&fname=" + requestInfo.FirstName +
                                  "&lname=" + requestInfo.LastName + "&gender=" + requestInfo.Gender + "&email=" + requestInfo.Email + "&cell=" + requestInfo.Cell;
                    }
                    else
                    body = "Congratulations!!, your property request was accepted. The property owner will contact you if there"
                            + " is further negotiations or concerns.";
                    //getting information about the owner of the property to give back to the requestee

                    var accommodationOwnerInformation = dbCtx.REQUISITION_PROPERTY_MAPPINGS.Where(a => a.ID == reqID)
                                            .Select(l => l.ACCOMMODATIONS.LANDLORDS);

                    var houseOwnerInformation = dbCtx.REQUISITION_PROPERTY_MAPPINGS.Where(a => a.ID == reqID)
                                            .Select(l => l.HOUSE.LANDLORDS);

                    var landOwnerInformation = dbCtx.REQUISITION_PROPERTY_MAPPINGS.Where(a => a.ID == reqID)
                                            .Select(l => l.LAND.LANDLORDS);

                    if (accommodationOwnerInformation != null)
                    {
                        foreach (var info in accommodationOwnerInformation)
                        {
                            body += "<br/> Owner Information<br/> First Name:&nbsp;" + info.FIRST_NAME + "<br/>Last Name:&nbsp;" + info.LAST_NAME
                                    + "<br/>Cellphone Number:&nbsp;" + info.CELL + "<br/>Email:&nbsp;" + info.EMAIL;
                        }
                    }
                    else if (houseOwnerInformation != null)
                    {
                        foreach (var info in houseOwnerInformation)
                        {
                            body += "<br/> Owner Information<br/> First Name:&nbsp;" + info.FIRST_NAME + "<br/>Last Name:&nbsp;" + info.LAST_NAME
                                    + "<br/>Cellphone Number:&nbsp;" + info.CELL + "<br/>Email:&nbsp;" + info.EMAIL;
                        }
                    }
                    else
                        foreach (var info in landOwnerInformation)
                        {
                            body += "<br/> Owner Information<br/> First Name:&nbsp;" + info.FIRST_NAME + "<br/>Last Name:&nbsp;" + info.LAST_NAME
                                    + "<br/>Cellphone Number:&nbsp;" + info.CELL + "<br/>Email:&nbsp;" + info.EMAIL;
                        }

                    //if (sendMail(emailTo, body, subject))
                    //{
                    //sets the accepted field of the requisition table to true for the accepted property request
                    REQUISITIONS requisition = dbCtx.REQUISITIONS.Single(x => x.REQUISITION_ID == reqID);
                    requisition.ACCEPTED = true;
                    dbCtx.SaveChanges();
                    //message outputted if request was accepted successfully
                    Session["acceptedRequestCheck"] = "Request has been successfully accepted";
                    //}
                    //else
                    //    throw new Exception("Mail Exception");
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
            List<IEnumerable> pInfo = new List<IEnumerable>();
            try
            {
                using (JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities())
                {
                    //checking if the landlord id was saved in the session
                    if ((Guid)Session["landlord_id"] != null)
                    {
                        var landlordID = (Guid)Session["landlord_id"];
                        /*
                         * using the property counts to detect the particular properties
                         * owned by the current user that is signed in
                         
                        var allAccommodationOwnedCount = dbCtx.LANDLORDS.Where(a => a.ID == landlordID)
                                                .Select(a => a.ACCOMMODATIONS.Select(x => x.ID).Count());

                        var allHouseOwnedCount = dbCtx.LANDLORDS.Where(a => a.ID == landlordID)
                                                        .Select(a => a.HOUSE.Select(x => x.ID).Count());

                        var allLandOwnedCount = dbCtx.LANDLORDS.Where(a => a.ID == landlordID)
                                                        .Select(a => a.LAND.Select(x => x.ID).Count());

                        if (allAccommodationOwnedCount.Select(x => x.ToString()).Single().ToString() != "0")
                        {
                            var allAccommodationOwned = dbCtx.LANDLORDS.Where(a => a.ID == landlordID)
                                                        .Select(a => a.ACCOMMODATIONS.Select(i => new { i.ID, i.IMAGE_URL })).SelectMany(x => x);

                            foreach (var property in allAccommodationOwned)
                            {
                                /*adding properties to dictionary to display image to the user
                                Dictionary<String, String> properties = new Dictionary<string, string>();
                                properties.Add("ID", property.ID.ToString());
                                properties.Add("ImageURL", property.IMAGE_URL);
                                //JsonConvert.SerializeObject(properties)
                                pInfo.Add(properties);
                            }
                        }

                        if (allHouseOwnedCount.Select(x => x.ToString()).Single().ToString() != "0")
                        {
                            var allHouseOwned = dbCtx.LANDLORDS.Where(a => a.ID == landlordID)
                                                        .Select(a => a.HOUSE.Select(i => new { i.ID, i.IMAGE_URL })).SelectMany(x => x);

                            foreach (var property in allHouseOwned)
                            {
                                /*adding properties to dictionary to display image to the user
                                Dictionary<String, String> properties = new Dictionary<string, string>();
                                properties.Add("ID", property.ID.ToString());
                                properties.Add("ImageURL", property.IMAGE_URL);

                                pInfo.Add(properties);
                            }
                        }

                        if (allLandOwnedCount.Select(x => x.ToString()).Single().ToString() != "0")
                        {
                            var allLandOwned = dbCtx.LANDLORDS.Where(a => a.ID == landlordID)
                                                        .Select(a => a.LAND.Select(i => new { i.ID, i.IMAGE_URL })).SelectMany(x => x);

                            foreach (var property in allLandOwned)
                            {
                                /*adding properties to dictionary to display image to the user
                                Dictionary<String, String> properties = new Dictionary<string, string>();
                                properties.Add("ID", property.ID.ToString());
                                properties.Add("ImageURL", property.IMAGE_URL);

                                pInfo.Add(properties);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("There has been an error retrieving  property images" + ex.Message);
            }

            return Json(pInfo, JsonRequestBehavior.AllowGet);
        }
        //returns latest 5 messages for a specific user
        [Authorize]
        public JsonResult getLatestMessages()
        {
            int count = 1;

            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();

            List<Messages> messagesList = new List<Messages>();

            var landlordID = (Guid)Session["landlord_id"];

            var messages = dbCtx.MESSAGES.Where(m => m.ID == landlordID).Select(m => new { m.ID, m.MESSAGE, m.MESSENGER_ID, m.DATE });

            foreach (var message in messages)
            {
                if (count < 6)
                {
                    Messages userMessages = new Messages()
                    {
                        ID = message.ID.ToString(),
                        MessengerID = message.MESSENGER_ID.ToString(),
                        MessengerName = dbCtx.TENNANTS.Where(t => t.ID == message.MESSENGER_ID).Select(t => t.FIRST_NAME).Single(),
                        Date = message.DATE.ToString(),
                        Message = message.MESSAGE
                    };

                    messagesList.Add(userMessages);
                }
                count++;
            }

            return Json(messagesList, JsonRequestBehavior.AllowGet);
        }
        //gets the basic information about the messages 
        [Authorize]
        public JsonResult getMessagesHeaders()
        {
            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();

            List<Messages> messagesList = new List<Messages>();

            var landlordID = (Guid)Session["landlord_id"];

            var messages = dbCtx.MESSAGES.Where(m => m.ID == landlordID).Select(m => new { m.ID, m.MESSAGE, m.MESSENGER_ID, m.DATE });

            foreach (var message in messages)
            {

                Messages userMessages = new Messages()
                {
                    ID = message.ID.ToString(),
                    MessengerID = message.MESSENGER_ID.ToString(),
                    MessengerName = dbCtx.TENNANTS.Where(t => t.ID == message.MESSENGER_ID).Select(t => t.FIRST_NAME).Single(),
                    Date = message.DATE.ToString()
                };

                messagesList.Add(userMessages);

            }

            return Json(messagesList, JsonRequestBehavior.AllowGet);
        }
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
        }
        //returns requition information
        [Authorize]
        public JsonResult getRequisitions()
        {
            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();
            List<RequisitionInformation> rInfo = new List<RequisitionInformation>();

            try
            {
                //retrieving requisitions made by users
                var requisitions = dbCtx.REQUISITION_PROPERTY_MAPPINGS
                    .Where(x => x.HOUSE.LANDLORDS.USERNAME == HttpContext.User.Identity.Name
                    || x.LAND.LANDLORDS.USERNAME == HttpContext.User.Identity.Name
                    || x.ACCOMMODATIONS.LANDLORDS.USERNAME == HttpContext.User.Identity.Name)
                    .Select(x => x.REQUISITIONS);

                foreach (var r in requisitions)
                {
                    if (r != null)
                    {
                        RequisitionInformation requisitionInformation = new RequisitionInformation();

                        requisitionInformation.ID = r.REQUISITION_ID.ToString();
                        requisitionInformation.FirstName = r.FIRST_NAME;
                        requisitionInformation.LastName = r.LAST_NAME;
                        requisitionInformation.Email = r.EMAIL;
                        requisitionInformation.Cell = r.CELL;
                        requisitionInformation.Gender = r.GENDER;
                        requisitionInformation.Date = r.R_DATE.ToShortDateString();
                        requisitionInformation.accepted = r.ACCEPTED;


                        if (r.REQUISITION_PROPERTY_MAPPINGS.ACCOMMODATION_ID.HasValue
                            && PropertiesDAO.getPropertyType(r.REQUISITION_PROPERTY_MAPPINGS.ACCOMMODATION_ID.Value).Equals(accommodation))
                        {
                            requisitionInformation.Image_URL = dbCtx.ACCOMMODATIONS.Where(a => a.ID == r.REQUISITION_PROPERTY_MAPPINGS.ACCOMMODATION_ID).Select(a => a.IMAGE_URL).Single();
                        }
                        if (r.REQUISITION_PROPERTY_MAPPINGS.HOUSE_ID.HasValue
                            && PropertiesDAO.getPropertyType(r.REQUISITION_PROPERTY_MAPPINGS.HOUSE_ID.Value).Equals(house))
                        {
                            requisitionInformation.Image_URL = dbCtx.HOUSE.Where(a => a.ID == r.REQUISITION_PROPERTY_MAPPINGS.HOUSE_ID).Select(a => a.IMAGE_URL).Single();
                        }
                        if (r.REQUISITION_PROPERTY_MAPPINGS.LAND_ID.HasValue
                            && PropertiesDAO.getPropertyType(r.REQUISITION_PROPERTY_MAPPINGS.LAND_ID.Value).Equals(land))
                        {
                            requisitionInformation.Image_URL = dbCtx.LAND.Where(a => a.ID == r.REQUISITION_PROPERTY_MAPPINGS.LAND_ID).Select(a => a.IMAGE_URL).Single();
                        }

                        rInfo.Add(requisitionInformation);
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("Error retrieving requisitions"); }

            return Json(rInfo, JsonRequestBehavior.AllowGet);
        }
        /*
         * gets the information for the property that was selected in order to
         * update information about this property
         
        [Authorize]
        public JsonResult getProperty(Guid property_id)
        {
            JWorldPropertiesEntities dbCtx = new JWorldPropertiesEntities();

            List<AccommodationModel> AInfo = new List<AccommodationModel>();
            List<HouseModel> HInfo = new List<HouseModel>();
            List<LandModel> LInfo = new List<LandModel>();

            //var accommodationCount = dbCtx.ACCOMMODATIONS.Where(a => a.ID == property_id).Count();
            //var houseCount = dbCtx.HOUSE.Where(h => h.ID == property_id).Count();
            // landCount = dbCtx.LAND.Where(l => l.ID == property_id).Count();

            if (PropertiesDAO.getPropertyType(property_id) == PropertyConstants.PropertyType.accommodation)
            {
                var accommodations = dbCtx.ACCOMMODATIONS.Where(a => a.ID == property_id).Select(a => new { a.ID, a.IMAGE_URL, a.WATER, a.TERMS_AGREEMENT, a.AVAILABILITY, a.INTERNET, a.OCCUPANCY, a.PRICE, a.SECURITY_DEPOSIT, a.CABLE, a.DESCRIPTION, a.ELECTRICITY, a.GAS, a.GENDER_PREFERENCE });

                AccommodationModel accommodationModel = new AccommodationModel();

                foreach (var accommodation in accommodations)
                {
                    accommodationModel.ID = accommodation.ID.ToString();
                    accommodationModel.Availability = accommodation.AVAILABILITY == true ? "selected" : "";
                    accommodationModel.Cable = accommodation.CABLE == true ? "selected" : "";
                    accommodationModel.Description = accommodation.DESCRIPTION;
                    accommodationModel.Electricity = accommodation.ELECTRICITY == true ? "selected" : "";
                    accommodationModel.Gas = accommodation.GAS == true ? "selected" : "";
                    accommodationModel.Internet = accommodation.INTERNET == true ? "selected" : "";
                    accommodationModel.Occupancy = accommodation.OCCUPANCY.ToString();
                    accommodationModel.Price = accommodation.PRICE.ToString();
                    accommodationModel.TermsAgreement = accommodation.TERMS_AGREEMENT;
                    accommodationModel.SecurityDeposit = accommodation.SECURITY_DEPOSIT.ToString();
                    accommodationModel.Water = accommodation.WATER == true ? "selected" : "";
                    accommodationModel.ImageURL = accommodation.IMAGE_URL;
                    accommodationModel.GenderPreference = accommodation.GENDER_PREFERENCE;
                }

                AInfo.Add(accommodationModel);

                return Json(AInfo, JsonRequestBehavior.AllowGet);
            }

            if (PropertiesDAO.getPropertyType(property_id) == PropertyConstants.PropertyType.house)
            {
                var houses = dbCtx.HOUSE.Where(h => h.ID == property_id).Select(h => new { h.ID, h.IMAGE_URL, h.ISFURNISHED, h.PRICE, h.PURPOSE, h.BATH_ROOM_AMOUNT, h.BED_ROOM_AMOUNT, h.DESCRIPTION });

                HouseModel houseModel = new HouseModel();

                foreach (var house in houses)
                {
                    houseModel.ID = house.ID.ToString();
                    houseModel.BathroomAmount = house.BATH_ROOM_AMOUNT;
                    houseModel.BedroomAmount = house.BED_ROOM_AMOUNT;
                    houseModel.Price = house.PRICE.ToString();
                    houseModel.isFurnished = house.ISFURNISHED.ToString();
                    houseModel.Purpose = house.PURPOSE;
                    houseModel.Description = house.DESCRIPTION;
                    houseModel.ImageURL = house.IMAGE_URL;
                }

                HInfo.Add(houseModel);

                return Json(HInfo, JsonRequestBehavior.AllowGet);
            }

            if (PropertiesDAO.getPropertyType(property_id) == PropertyConstants.PropertyType.land)
            {
                var lands = dbCtx.LAND.Where(l => l.ID == property_id).Select(l => new { l.ID, l.IMAGE_URL, l.PRICE, l.PURPOSE, l.AREA, l.DESCRIPTION });

                LandModel landModel = new LandModel();

                foreach (var land in lands)
                {
                    landModel.ID = land.ID.ToString();
                    landModel.Area = land.AREA.ToString();
                    landModel.Price = land.PRICE.ToString();
                    landModel.Purpose = land.PURPOSE;
                    landModel.ImageURL = land.IMAGE_URL;
                    landModel.Description = land.DESCRIPTION;
                }

                LInfo.Add(landModel);

                return Json(LInfo, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }
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


