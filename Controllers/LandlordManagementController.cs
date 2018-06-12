using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SS.Models;
using SS.Core;
using SS.ViewModels;
using SS.SignalR;
using SS.ViewModels.Management;

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
            if (Session["username"] != null && Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];//unitOfWork.User.GetUserByEmail(HttpContext.User.Identity.Name).ID;
                                                     // var userTypes = unitOfWork.UserTypeAssoc.GetUserTypesByUserID(userId);
                                                     // bool isUserPropOwner = PropertyHelper.isUserOfType(userTypes, EFPConstants.UserType.PropertyOwner);
                var isUserPropOwner = Session["isUserPropOwner"] != null ? (bool)Session["isUserPropOwner"] : false;

                ViewBag.userId = Session["userId"];
                ViewBag.isUserPropOwner = isUserPropOwner;
                ViewBag.isUserConsumer = Session["isUserConsumer"] != null ? (bool)Session["isUserConsumer"] : false;
                ViewBag.isUserTennant = Session["isUserTennant"] != null ? (bool)Session["isUserTennant"] : false;

                if (isUserPropOwner)
                {
                    ViewBag.propertyImages = PropertyHelper.GetAllPropertyImages(userId);
                }
                else
                {
                    ViewBag.propertyImages = PropertyHelper.GetAllSavedPropertyImages(userId);
                }

                return View();
            }
            else
                return RedirectToAction("signin", "accounts");
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
                    PropertyHelper.AcceptPropertyRequisition(unitOfWork, reqID);
                }
            }
            catch (Exception ex)
            {
                //message outputted if request acceptance has failed
                Session["acceptedRequestCheck"] = "An error has occurred while accepting the request. Please contact site administrator";

                return Content("RequestFailed");
            }

            //message outputted if request was accepted successfully
            Session["acceptedRequestCheck"] = "Request has been successfully accepted";

            return Content("RequestSuccess");
        }

        //denies user's requisition
        [HttpPost]
        public ActionResult cancelRequest(Guid reqID, bool isUserPropOwner)
        {
            try
            {
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                    PropertyHelper.CancelOrDenyPropertyRequisition(unitOfWork, reqID, isUserPropOwner);
                }
            }
            catch (Exception ex)
            {
                Session["cancelRequestCheck"] = "An error has occurred while cancelling the request. Please contact site administrator";
            }

            //message outputted if request was accepted successfully
            Session["acceptedRequestCheck"] = "Request has been successfully accepted";

            return Content("Request Cancelled");
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
        /// deletes all messages within the message thread both user and sender
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public void deleteMsgThread(Guid id)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if ((Guid)Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];

                    PropertyHelper.deleteMsgsFromMsgThread(unitOfWork, userId, id);
                }
            }
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

        /// <summary>
        /// returns requition information for the user
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RequisitionViewModel> getRequisitions()
        {
            List<RequisitionViewModel> requisitionInfo = null;
            IEnumerable<PropertyRequisition> requisitions = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];

                    var userTypes = unitOfWork.UserTypeAssoc.GetUserTypesByUserID(userId);
                    bool isUserPropOwner = PropertyHelper.isUserOfType(userTypes, EFPConstants.UserType.PropertyOwner);

                    if (isUserPropOwner)
                    {
                        var owner = unitOfWork.Owner.GetOwnerByUserID(userId);
                        requisitions = unitOfWork.PropertyRequisition.GetRequestsByOwnerId(owner.ID);
                        requisitionInfo = PropertyHelper.populateRequisitionVMForOwner(unitOfWork, requisitions);
                    }
                    else
                    {
                        requisitions = unitOfWork.PropertyRequisition.GetRequestsMadeByUserId(userId);
                        requisitionInfo = PropertyHelper.populateRequisitionVMForRequestor(unitOfWork, requisitions);
                    }
                }
            }

            return requisitionInfo;
        }

        /// <summary>
        /// returns the users which the user can set meetings for and 
        /// send messages to
        /// </summary>
        /// <returns></returns>
        public JsonResult getInvitees()
        {
            List<InviteeViewModel> invitees = new List<InviteeViewModel>();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];

                    bool isUserPropOwner = Session["isUserPropOwner"] != null ? (bool)Session["isUserPropOwner"] : false;
                    bool isUserTennant = Session["isUserTennant"] != null ? (bool)Session["isUserTennant"] : false;

                    if (isUserPropOwner)
                    {
                        setInviteeVMForPO(unitOfWork, userId, invitees);
                    }
                    else if (isUserTennant)
                    {
                        setInviteeVMForTennant(unitOfWork, userId, invitees);
                    }
                    else
                    {
                        setInviteeVMForPropertyRequestors(unitOfWork, userId, invitees);
                    }
                }
            }

            return Json(invitees, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Sets the invitee view model for tennants
        /// property owner and other tennants belonging to the same property
        /// should be sent back as invitees
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="userId"></param>
        /// <param name="invitees"></param>
        private void setInviteeVMForTennant(UnitOfWork unitOfWork, Guid userId, List<InviteeViewModel> invitees)
        {
            var tennant = unitOfWork.Tennant.GetTennantByUserId(userId);
            var poUser = tennant.Property.Owner.User;

            //getting all tennants that are in the same property
            var currentPropertyId = tennant.PropertyID;
            var tennants = unitOfWork.Tennant.GetTennantsByPropertyId(currentPropertyId);

            InviteeViewModel inviteeViewModel = new InviteeViewModel()
            {
                UserID = poUser.ID,
                FullName = poUser.FirstName + " " + poUser.LastName,
                ImageUrl = "",
                inviteeType = "O"
            };

            invitees.Add(inviteeViewModel);

            foreach (var t in tennants)
            {
                //exclude current signed in tennant from being populated
                if (!t.ID.Equals(tennant.ID))
                {
                    inviteeViewModel = new InviteeViewModel()
                    {
                        UserID = tennant.User.ID,
                        FullName = tennant.User.FirstName + " " + tennant.User.LastName,
                        ImageUrl = tennant.PhotoUrl,
                        inviteeType = EFPConstants.UserType.Tennant
                    };

                    invitees.Add(inviteeViewModel);
                }
            }
        }

        /// <summary>
        /// Sets the invitee view model for users who currently requested propeties
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="userId"></param>
        /// <param name="invitees"></param>
        private void setInviteeVMForPropertyRequestors(UnitOfWork unitOfWork, Guid userId, List<InviteeViewModel> invitees)
        {
            var users = unitOfWork.PropertyRequisition.GetRequestedPropertyUsers(userId);

            foreach (var user in users)
            {
                InviteeViewModel inviteeViewModel = new InviteeViewModel()
                {
                    UserID = user.ID,
                    FullName = user.FirstName + " " + user.LastName,
                    ImageUrl = "",
                    inviteeType = "R"
                };

                invitees.Add(inviteeViewModel);
            }
        }

        /// <summary>
        /// Sets the invitee view model for property owners
        /// Property owner's tennants and requisitions not yet declined should be
        /// sent back as invitees
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="userId"></param>
        /// <param name="invitees"></param>
        private void setInviteeVMForPO(UnitOfWork unitOfWork, Guid userId, List<InviteeViewModel> invitees)
        {
            var owner = unitOfWork.Owner.GetOwnerByUserID(userId);
            var tennants = unitOfWork.Tennant.GetTennantsByOwnerId(owner.ID);
            var userRequestees = unitOfWork.PropertyRequisition.GetRequestedPropertyUsersByOwnerId(owner.ID);

            //populate invitee model with each ienumerable items
            foreach (var tennant in tennants)
            {
                InviteeViewModel inviteeViewModel = new InviteeViewModel()
                {
                    UserID = tennant.User.ID,
                    FullName = tennant.User.FirstName + " " + tennant.User.LastName,
                    ImageUrl = tennant.PhotoUrl,
                    inviteeType = EFPConstants.UserType.Tennant
                };

                invitees.Add(inviteeViewModel);
            }

            foreach (var user in userRequestees)
            {
                InviteeViewModel inviteeViewModel = new InviteeViewModel()
                {
                    UserID = user.ID,
                    FullName = user.FirstName + " " + user.LastName,
                    ImageUrl = "",
                    inviteeType = "R"
                };

                if (!invitees.Contains(inviteeViewModel))
                    invitees.Add(inviteeViewModel);
            }
        }

        /// <summary>
        /// Returns message recipients
        /// getinvitees wrapper
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult getMsgRecipients()
        {
            return getInvitees();
        }

        /// <summary>
        /// Sends new message to a user
        /// getinvitees wrapper
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public void sendMessage(List<Guid> msgRecipients, String msg)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];

                    foreach (var recipientId in msgRecipients)
                    {
                        Message message = new Message()
                        {
                            ID = Guid.NewGuid(),
                            To = recipientId,
                            From = userId,
                            Msg = msg,
                            Seen = false,
                            DateTCreated = DateTime.Now
                        };

                        unitOfWork.Message.Add(message);
                        unitOfWork.save();

                        var userTo = unitOfWork.User.Get(recipientId);

                        DashboardHub.BroadcastUserMessages(userTo.Email);
                    }
                }
            }
        }

        /// <summary>
        /// returns the meeting for the given Id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult getMeeting(Guid Id)
        {
            MeetingViewModel model = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var meeting = unitOfWork.Meeting.Get(Id);

                model = new MeetingViewModel()
                {
                    ID = meeting.ID,
                    MeetingTitle = meeting.MeetingTitle,
                    MeetingDate = meeting.MeetingDate,
                    MeetingHour = meeting.MeetingHour,
                    MeetingMinute = meeting.MeetingMinute,
                    MeetingPeriod = meeting.MeetingPeriod,
                    Location = meeting.Location,
                    Purpose = meeting.Purpose,
                    MeetingMemberUserIDs = new List<Guid>(meeting.MeetingMembers.Select(x => x.InviteesUserID).ToList())    //gets all meeting member user ids
                };
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// returns the meeting for the given Id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult getMeetingsForUser()
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];

                    var meetings = unitOfWork.Meeting.GetMeetingsByUserId(userId);

                    return Json(meetings, JsonRequestBehavior.AllowGet);
                }

            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Insert meeting records
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public void scheduleMeeting(MeetingViewModel model, Boolean isEdit = false)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    Meeting meeting = null;
                    var userId = (Guid)Session["userId"];

                    IEnumerable<MeetingMembers> orMeetingMembers = null;
                    if (isEdit)
                    {
                        meeting = unitOfWork.Meeting.Get(model.ID);

                        meeting.MeetingTitle = model.MeetingTitle;
                        meeting.MeetingHour = model.MeetingHour;
                        meeting.MeetingMinute = model.MeetingMinute;
                        meeting.MeetingPeriod = model.MeetingPeriod;
                        meeting.Location = model.Location;
                        meeting.Purpose = model.Purpose;
                        meeting.DateTCreated = DateTime.Now;

                        //remove previous meeting members and replace with new
                        orMeetingMembers = unitOfWork.Meeting.Get(model.ID).MeetingMembers;
                        unitOfWork.MeetingMembers.RemoveRange(orMeetingMembers);
                    }
                    else
                    {
                        meeting = new Meeting()
                        {
                            ID = Guid.NewGuid(),
                            InviterUserID = userId,
                            MeetingTitle = model.MeetingTitle,
                            MeetingDate = model.MeetingDate,
                            MeetingHour = model.MeetingHour,
                            MeetingMinute = model.MeetingMinute,
                            MeetingPeriod = model.MeetingPeriod,
                            Location = model.Location,
                            Purpose = model.Purpose,
                            DateTCreated = DateTime.Now
                        };

                        unitOfWork.Meeting.Add(meeting);//if it is not an edit then add new meeting
                    }


                    foreach (var id in model.MeetingMemberUserIDs)
                    {
                        MeetingMembers meetingMembers = new MeetingMembers()
                        {
                            MeetingID = meeting.ID,
                            InviteesUserID = id,
                            DateTCreated = DateTime.Now
                        };

                        unitOfWork.MeetingMembers.Add(meetingMembers);
                    }

                    unitOfWork.save();
                    //alert active users of their updated scheduled meetings
                    foreach (var id in model.MeetingMemberUserIDs)
                    {
                        DashboardHub.broadcastMeeting(id.ToString());
                    }
                }
            }
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

        /// <summary>
        /// returns the requisition partial view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMeetingRequestView()
        {
            return PartialView("_MeetingRequest");
        }

        /// <summary>
        /// returns the Meetings view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMeetingsView()
        {
            return PartialView("_Meetings");
        }

        /// <summary>
        /// returns the tennants view with date for the current logged in user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetTennantsView()
        {
            IEnumerable<Tennant> tennants = null;

            //using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            //{
            EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities();
            UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];

                var ownerId = unitOfWork.Owner.GetOwnerByUserID(userId).ID;

                tennants = unitOfWork.Tennant.GetTennantsByOwnerId(ownerId);
            }
            //}

            return PartialView("_Tennants", tennants);
        }

        [HttpGet]
        public ActionResult GetManagementPropertiesView()
        {
            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];

                ViewBag.propertyImages = PropertyHelper.GetAllPropertyImages(userId);
            }

            return PartialView("_partialPropertiesOwned");
        }

        [HttpGet]
        public ActionResult GetSavedPropertiesView()
        {
            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];

                ViewBag.propertyImages = PropertyHelper.GetAllSavedPropertyImages(userId);
            }

            return PartialView("_partialPropertiesSaved");
        }
        /// <summary>
        /// Updates the rent amount for the selected tennant
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public void updateRent(Guid id, decimal newRentAmt)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var tennant = unitOfWork.Tennant.Get(id);

                tennant.RentAmt = newRentAmt;

                unitOfWork.save();
            }
        }

        /// <summary>
        /// removes the tennant's record from the system
        /// as well as remove the tennant role from the user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public void unenrollTennant(Guid id)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var tennant = unitOfWork.Tennant.Get(id);

                var userTypeAssoc = unitOfWork.UserTypeAssoc.GetTennantUserTypeAssocByUserID(tennant.UserID);

                unitOfWork.UserTypeAssoc.Remove(userTypeAssoc);
                unitOfWork.Tennant.Remove(tennant);

                unitOfWork.save();
            }
        }

        /// <summary>
        /// Updates the property
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateProperty(UpdatePropertyViewModel model)
        {
            var isUpdated = PropertyHelper.UpdateProperty(model);
            TempData["PropertyUpdated"] = isUpdated;

            return RedirectToAction("dashboard");
        }

        /// <summary>
        /// Updates the propert's primary image flag
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public ActionResult UpdatePropertyPrimaryImg(Guid propertyId, Guid imgId)
        {
            if (PropertyHelper.UpdatePropertyPrimaryImg(propertyId, imgId))
                return PartialView("_partialUpdatePropertyImage", PropertyHelper.GetUpdatePropertyVM_PropertyImages(propertyId));

            return null;
        }

        /// <summary>
        /// Deletes property image
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public ActionResult DeletePropertyImage(Guid propertyId, Guid imageId)
        {
            if (PropertyHelper.DeletePropertyImage(imageId))
                return PartialView("_partialUpdatePropertyImage", PropertyHelper.GetUpdatePropertyVM_PropertyImages(propertyId));

            return null;
        }

        /// <summary>
        /// Adds property image
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddPropertyImage(HttpPostedFileBase propertyImgUpload, Guid ID)
        {
            if (!String.IsNullOrEmpty(PropertyHelper.AssociateImageWithProperty(propertyImgUpload, ID)))
                return PartialView("_partialUpdatePropertyImage", PropertyHelper.GetUpdatePropertyVM_PropertyImages(ID));

            return null;
        }

        //loads advertise property view
        public ActionResult AdvertiseProperty()
        {
            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];

                if (PropertyHelper.IsAdAccessValid(userId))
                {
                    TempData["layout"] = "~/Views/Shared/_ManagementLayout.cshtml";
                    TempData["calledByManagement"] = true;

                    return RedirectToAction("advertiseproperty", "accounts");
                }
                else TempData["errorMsg"] = PropertyHelper.GetAdAccessErrMessage();
            }
            else
                TempData["errorMsg"] = "User session has ended. Please log out then log in";

            return RedirectToAction("dashboard", "landlordmanagement");
        }

        /// <summary>
        /// Forwards the 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetProperty(Guid id)
        {
            return RedirectToAction("getproperty", "properties", new { id = id });
        }

        /// <summary>
        /// returns the account view with it's data model
        /// </summary>
        /// <returns></returns>
        public ActionResult Account()
        {
            ProfileViewModel profileVM = null;

            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];
                var isUserPropOwner = Session["isUserPropOwner"] != null ? (bool)Session["isUserPropOwner"] : false;

                profileVM = PropertyHelper.PopulateProfileViewModel(userId, isUserPropOwner);

                ViewBag.userId = userId;
                return View(profileVM);
            }
            else
                return RedirectToAction("signin", "accounts");
        }

        /// <summary>
        /// return profile view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetProfileView()
        {
            ProfileViewModel profileVM = null;

            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];
                var isUserPropOwner = Session["isUserPropOwner"] != null ? (bool)Session["isUserPropOwner"] : false;

                profileVM = PropertyHelper.PopulateProfileViewModel(userId, isUserPropOwner);
            }

            return PartialView("_partialProfile", profileVM);
        }

        /// <summary>
        /// return profile view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetSubscriptionView()
        {
            SubscriptionViewModel subscriptionVM = null;

            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];
                subscriptionVM = PropertyHelper.PopulateSubscriptionViewModel(userId);
            }

            return PartialView("_partialSubscription", subscriptionVM);
        }

        /// <summary>
        /// return profile view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetPaymentsView(int pgTake = 16, int pgNo = 0)
        {
            IEnumerable<PaymentViewModel> payments = null;

            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];
                payments = PropertyHelper.GetPayments(pgTake, pgNo, userId);

                ViewBag.pgNo = pgNo;
                ViewBag.pgTake = pgTake;
                ViewBag.itemsCount = PropertyHelper.GetPaymentsCount(userId);
            }

            return PartialView("_partialPayments", payments);
        }

        /// <summary>
        /// Updates user profile
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateProfile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                return Json(PropertyHelper.UpdateProfile(model), JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the _partialModalPayment view to make payments towards a subscription
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetModalMakePayment(PaymentViewModel model)
        {
            return PartialView("_partialModalPayment", model);
        }

        [HttpPost]
        public JsonResult MakePayment(PaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                return Json(PropertyHelper.MakePayment(model));
            }
            else
            {
                return Json(false);
            }
        }

        [HttpDelete]
        public JsonResult RemoveSavedProperty(Guid propertyID)
        {
            bool result = false;

            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];
                result = PropertyHelper.RemoveSavedProperty(userId, propertyID);
            }

            return Json(result);
        }

        [HttpPut]
        public JsonResult TogglePropertyAvailability(Guid propertyID)
        {
            ErrorModel errorModel = new ErrorModel();

            if (Session["userId"] != null)
                errorModel = PropertyHelper.TogglePropertyAvailability(propertyID);

            return Json(errorModel);
        }

        [HttpDelete]
        public JsonResult RemoveProperty(Guid propertyID)
        {
            bool result = false;

            if (Session["userId"] != null)
                result = PropertyHelper.RemoveProperty(propertyID);

            return Json(result);
        }

        [HttpGet]
        public ActionResult GetModalSubscriptionChange(Guid subscriptionID)
        {
            if (Session["userId"] != null)
            {
                ViewBag.currentSubType = PropertyHelper.GetSubscriptionTypeByUserSubId(subscriptionID);
                ViewBag.subscriptionTypes = PropertyHelper.GetSubscriptionTypes();
            }
            return PartialView("_partialModalSubscriptionChange");
        }

        [HttpPost]
        public JsonResult changeSubscription(Guid subscriptionID, String subscriptionType, int? period)
        {
            var result = new RequestModel();

            if (Session["userId"] != null)
            {
                result = PropertyHelper.ChangeSubscription(subscriptionID, subscriptionType, period);
            }

            return Json(result);
        }

        [HttpPost]
        public JsonResult RenewSubscription(Guid subscriptionID)
        {
            var result = new RequestModel();

            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];
                result = PropertyHelper.RenewSubscription(subscriptionID, userId);
            }

            return Json(result);
        }

        [HttpPost]
        public JsonResult CancelSubscription(Guid subscriptionID)
        {
            var result = new RequestModel();

            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];
                result = PropertyHelper.CancelSubscription(subscriptionID, userId);
            }

            return Json(result);
        }
    }

}


