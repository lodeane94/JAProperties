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
using log4net;
using SS.Services;

namespace SS.Controllers
{
    [Authorize]
    public class LandlordManagementController : Controller
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly PropertyService propertyService;
        private readonly PropertyRequisitionService propertyRequisitionService;
        private readonly MessageService messageService;
        private readonly UserService userService;
        private readonly PaymentService paymentService;
        private readonly SubscriptionService subscriptionService;

        public LandlordManagementController()
        {
            propertyService = new PropertyService();
            propertyRequisitionService = new PropertyRequisitionService();
            messageService = new MessageService();
            userService = new UserService();
            paymentService = new PaymentService();
            subscriptionService = new SubscriptionService();
        }

        //loads the help page
        public ActionResult Help()
        {
            return View();
        }
        //loads dashboard page
        public ActionResult Dashboard()
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["username"] != null && Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];

                    var isUserPropOwner = Session["isUserPropOwner"] != null ? (bool)Session["isUserPropOwner"] : false;

                    ViewBag.userId = Session["userId"];
                    ViewBag.isUserPropOwner = isUserPropOwner;
                    ViewBag.isUserConsumer = Session["isUserConsumer"] != null ? (bool)Session["isUserConsumer"] : false;
                    ViewBag.isUserTennant = Session["isUserTennant"] != null ? (bool)Session["isUserTennant"] : false;

                    if (isUserPropOwner)
                    {
                        ViewBag.propertyImages = propertyService.GetAllPropertyImages(userId, unitOfWork);
                    }
                    else
                    {
                        ViewBag.propertyImages = propertyService.GetAllSavedPropertyImages(userId, unitOfWork);
                    }

                    ViewBag.unseenMsgCount = messageService.GetUnseenMsgsCount(userId, unitOfWork);
                    ViewBag.unseenReqCount = propertyRequisitionService.GetUnseenReqsCount(userId, unitOfWork);

                    return View();
                }
                else
                    return RedirectToAction("signin", "accounts");
            }
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
                    propertyRequisitionService.AcceptPropertyRequisition(unitOfWork, reqID);
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
                    propertyRequisitionService.CancelOrDenyPropertyRequisition(unitOfWork, reqID, isUserPropOwner);
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
        /// returns messages for a specific user
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        //
        public JsonResult GetMessages()
        {
            List<MessageViewModel> messagesViewModel = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];

                    messagesViewModel = messageService.GetMessages(userId, unitOfWork);
                }
            }

            return Json(messagesViewModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// removes the selected message from the system
        /// </summary>
        /// <param name="id"></param>
        [HttpGet]
        public void DeleteMsg(Guid id)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if ((Guid)Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];
                    messageService.DeleteMsg(id, userId, unitOfWork);
                }
            }
        }

        /// <summary>
        /// Get messages in the proper order both user and sender based on the msg selected
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetMsgThread(Guid id)
        {
            IEnumerable<Message> messages = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if ((Guid)Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];

                    messages = messageService.GetMsgThread(unitOfWork, id, userId);
                }

                return Json(messages, JsonRequestBehavior.AllowGet); ;
            }
        }

        /// <summary>
        /// deletes all messages within the message thread both user and sender
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public void DeleteMsgThread(Guid id)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if ((Guid)Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];

                    messageService.DeleteMsgsFromMsgThread(unitOfWork, userId, id);
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
                    var threadId = message.ThreadId;
                    User userTo = null;

                    if (userId.Equals(message.From))
                        userTo = unitOfWork.User.Get(message.To);
                    else
                        userTo = unitOfWork.User.Get(message.From);

                    Message newMsg = new Message()
                    {
                        ID = Guid.NewGuid(),
                        ThreadId = threadId,
                        To = userTo.ID,
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
            IEnumerable<RequisitionViewModel> requisitionInfo = null;

            if (Session["userId"] != null)
            {
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    var userId = (Guid)Session["userId"];
                    requisitionInfo = propertyRequisitionService.GetRequisitions(userId, false, unitOfWork);
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
            var rpUsers = unitOfWork.PropertyRequisition.GetRequestedPropertyUsers(userId);
            var mUsers = unitOfWork.Message.GetMsgUsers(userId);
            var users = rpUsers.Concat(mUsers).Distinct();

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
            var mUsers = unitOfWork.Message.GetMsgUsers(userId);
            var users = userRequestees.Concat(mUsers).Distinct();

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

            foreach (var user in users)
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
                        var threadId = unitOfWork.Message.GetThreadIdForUser(userId, recipientId);

                        Message message = new Message()
                        {
                            ID = Guid.NewGuid(),
                            ThreadId = threadId != Guid.Empty ? threadId : Guid.NewGuid(),
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
            var requisitions = getRequisitions().ToList();
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
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];

                    ViewBag.propertyImages = propertyService.GetAllPropertyImages(userId, unitOfWork);
                }
            }
            return PartialView("_partialPropertiesOwned");
        }

        [HttpGet]
        public ActionResult GetSavedPropertiesView()
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];

                    ViewBag.propertyImages = propertyService.GetAllSavedPropertyImages(userId, unitOfWork);
                }
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
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var isUpdated = propertyService.UpdateProperty(model, unitOfWork);

                TempData["PropertyUpdated"] = isUpdated;
            }

            return RedirectToAction("dashboard");
        }

        /// <summary>
        /// Updates the propert's primary image flag
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public ActionResult UpdatePropertyPrimaryImg(Guid propertyId, Guid imgId)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (propertyService.UpdatePropertyPrimaryImg(propertyId, imgId, unitOfWork))
                    return PartialView("_partialUpdatePropertyImage", propertyService.GetUpdatePropertyVM_PropertyImages(propertyId, unitOfWork));
            }

            return null;
        }

        /// <summary>
        /// Deletes property image
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public ActionResult DeletePropertyImage(Guid propertyId, Guid imageId)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (propertyService.DeletePropertyImage(imageId, unitOfWork))
                    return PartialView("_partialUpdatePropertyImage", propertyService.GetUpdatePropertyVM_PropertyImages(propertyId, unitOfWork));
            }
            return null;
        }

        /// <summary>
        /// Adds property image
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddPropertyImage(HttpPostedFileBase propertyImgUpload, Guid ID)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (!String.IsNullOrEmpty(propertyService.AssociateImageWithProperty(propertyImgUpload, ID, unitOfWork)))
                    return PartialView("_partialUpdatePropertyImage", propertyService.GetUpdatePropertyVM_PropertyImages(ID, unitOfWork));
            }

            return null;
        }

        //loads advertise property view
        public ActionResult AdvertiseProperty()
        {
            if (Session["userId"] != null)
            {
                var userId = (Guid)Session["userId"];
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    if (subscriptionService.IsAdAccessValid(userId, unitOfWork))
                    {
                        TempData["layout"] = "~/Views/Shared/_ManagementLayout.cshtml";
                        TempData["calledByManagement"] = true;

                        return RedirectToAction("advertiseproperty", "accounts");
                    }
                    else TempData["errorMsg"] = subscriptionService.GetAdAccessErrMessage();
                }
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

                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    profileVM = userService.PopulateProfileViewModel(userId, isUserPropOwner, unitOfWork);
                }

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

                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    profileVM = userService.PopulateProfileViewModel(userId, isUserPropOwner, unitOfWork);
                }
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

                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    subscriptionVM = subscriptionService.PopulateSubscriptionViewModel(userId, unitOfWork);
                }
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

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];
                    payments = paymentService.GetPayments(pgTake, pgNo, userId, unitOfWork);

                    ViewBag.pgNo = pgNo;
                    ViewBag.pgTake = pgTake;
                    ViewBag.itemsCount = paymentService.GetPaymentsCount(userId, unitOfWork);
                }
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
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    return Json(userService.UpdateProfile(model, unitOfWork), JsonRequestBehavior.AllowGet);
                }
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
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    return Json(paymentService.MakePayment(model, unitOfWork));
                }
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
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    var userId = (Guid)Session["userId"];
                    result = propertyService.RemoveSavedProperty(userId, propertyID, unitOfWork);
                }
            }

            return Json(result);
        }

        [HttpPut]
        public JsonResult TogglePropertyAvailability(Guid propertyID)
        {
            ErrorModel errorModel = new ErrorModel();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                    errorModel = propertyService.TogglePropertyAvailability(propertyID, unitOfWork);
            }

            return Json(errorModel);
        }

        [HttpDelete]
        public JsonResult RemoveProperty(Guid propertyID)
        {
            bool result = false;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                    result = propertyService.RemoveProperty(propertyID, unitOfWork);
            }

            return Json(result);
        }

        [HttpGet]
        public ActionResult GetModalSubscriptionChange(Guid subscriptionID)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    ViewBag.currentSubType = subscriptionService.GetSubscriptionTypeByUserSubId(subscriptionID, unitOfWork);
                    ViewBag.subscriptionTypes = subscriptionService.GetSubscriptionTypes(unitOfWork);
                }
            }
            return PartialView("_partialModalSubscriptionChange");
        }

        [HttpPost]
        public JsonResult changeSubscription(Guid subscriptionID, String subscriptionType, int? period)
        {
            var result = new RequestModel();

            if (Session["userId"] != null)
            {
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    result = subscriptionService.ChangeSubscription(subscriptionID, subscriptionType, period, unitOfWork);
                }
            }

            return Json(result);
        }

        [HttpPost]
        public JsonResult RenewSubscription(Guid subscriptionID)
        {
            var result = new RequestModel();

            if (Session["userId"] != null)
            {
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                    var userId = (Guid)Session["userId"];

                    result = subscriptionService.RenewSubscription(subscriptionID, userId, unitOfWork);
                }
            }

            return Json(result);
        }

        [HttpPost]
        public JsonResult CancelSubscription(Guid subscriptionID)
        {
            var result = new RequestModel();

            if (Session["userId"] != null)
            {
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                    var userId = (Guid)Session["userId"];

                    result = subscriptionService.CancelSubscription(subscriptionID, userId, unitOfWork);
                }
            }

            return Json(result);
        }

        [HttpGet]
        public ActionResult GetRequisitionHistory()
        {
            List<RequisitionViewModel> requisitions = null;

            if (Session["userId"] != null)
            {
                using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                    var userId = (Guid)Session["userId"];

                    requisitions = propertyRequisitionService.GetRequisitionHistory(userId, unitOfWork).ToList();
                }
            }

            return PartialView("_RequisitionHistory", requisitions);
        }

        [HttpGet]
        public JsonResult GetUnseenMsgsCount()
        {
            var count = 0;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];
                    count = messageService.GetUnseenMsgsCount(userId, unitOfWork);
                }
            }

            return Json(count, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetUnseenReqsCount()
        {
            var count = 0;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (Session["userId"] != null)
                {
                    var userId = (Guid)Session["userId"];
                    count = propertyRequisitionService.GetUnseenReqsCount(userId, unitOfWork);
                }
            }

            return Json(count, JsonRequestBehavior.AllowGet);
        }
    }

}


