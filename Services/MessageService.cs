using log4net;
using SS.Core;
using SS.Models;
using SS.SignalR;
using SS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;

namespace SS.Services
{
    public class MessageService
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MessageService()
        { }

        /// <summary>
        /// Retrieves messages per user Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<MessageViewModel> GetMessages(Guid userId, UnitOfWork unitOfWork)
        {
            List<MessageViewModel> messagesViewModel = null;

            var messages = unitOfWork.Message.GetMsgsForUserID(userId);
            messagesViewModel = new List<MessageViewModel>();

            foreach (var msg in messages)
            {
                User user = null;

                if (userId.Equals(msg.From))
                    user = unitOfWork.User.Get(msg.To);
                else
                    user = unitOfWork.User.Get(msg.From);

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

            return messagesViewModel;
        }

        /// <summary>
        /// Get messages in the proper order both user and sender based on the msg selected
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="msgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<Message> GetMsgThread(UnitOfWork unitOfWork, Guid msgId, Guid userId)
        {
            updateMsgSeen(unitOfWork, msgId, userId);

            return unitOfWork.Message.GetMsgThreadByMsgID(msgId, userId);
        }

        /// <summary>
        /// deletes all messages from a message thread by retrieving all messages 
        /// sent to another user and and received from this same user then
        /// adding this list into the message trash. The messages list is refreshed after
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="userId"></param>
        /// <param name="messageId"></param>
        public void DeleteMsgsFromMsgThread(UnitOfWork unitOfWork, Guid userId, Guid messageId)
        {
            IEnumerable<Message> messagesToBeDeleted = null;

            messagesToBeDeleted = unitOfWork.Message.GetMsgThreadByMsgID(messageId, userId);

            if (messagesToBeDeleted != null)
            {
                foreach (var msg in messagesToBeDeleted)
                {
                    MessageTrash messageTrash = new MessageTrash()
                    {
                        UserID = userId,
                        MessageID = msg.ID,
                        DateTCreated = DateTime.Now
                    };

                    unitOfWork.MessageTrash.Add(messageTrash);
                }

                unitOfWork.save();

                //broadcast the new messages to the user
                var userTo = unitOfWork.User.Get(userId).Email;

                DashboardHub.BroadcastUserMessages(userTo);
            }
        }

        /// <summary>
        /// Removes the selected message from the system
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="userId"></param>
        /// <param name="unitOfWork"></param>
        public void DeleteMsg(Guid msgId, Guid userId, UnitOfWork unitOfWork)
        {
            var userTo = unitOfWork.User.Get(userId);
            var message = unitOfWork.Message.Get(msgId);

            if (message != null)
            {
                MessageTrash messageTrash = new MessageTrash()
                {
                    UserID = userId,
                    MessageID = msgId,
                    DateTCreated = DateTime.Now
                };

                unitOfWork.MessageTrash.Add(messageTrash);
                unitOfWork.save();

                //broadcast the new messages to the recipient 
                DashboardHub.BroadcastUserMessages(userTo.Email);
            }
        }

        /// <summary>
        /// Updates the seen column on the selected message
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="id"></param>
        private void updateMsgSeen(UnitOfWork unitOfWork, Guid msgId, Guid userId)
        {
            var msgThread = unitOfWork.Message.GetMsgThreadByMsgID(msgId, userId);
            var wasUpdated = false;

            foreach (var msg in msgThread)
            {
                var from = msg.From;

                if (!userId.Equals(from) && !msg.Seen)
                {
                    msg.Seen = true;
                    unitOfWork.save();
                    wasUpdated = true;
                }
            }

            if (wasUpdated)
            {
                var user = unitOfWork.User.Get(userId);
                DashboardHub.BroadcastUserMessages(user.Email);
            }
        }

        /// <summary>
        /// Sends message 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userId"></param>
        /// <param name="errorModel"></param>
        /// <param name="unitOfWork"></param>
        public void SendAnonymousMessage(PropertyRequisition request, Guid userId, ErrorModel errorModel, UnitOfWork unitOfWork)
        {
            using (var txscope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                MailingService mailingService = new MailingService();
                String errMsg = String.Empty;
                User ownerUser = unitOfWork.Property.GetPropertyOwnerByPropID(request.PropertyID).User;
                User user = unitOfWork.User.Get(userId);

                var threadId = unitOfWork.Message.GetThreadIdForUser(userId, ownerUser.ID);

                Message message = new Message()
                {
                    ID = Guid.NewGuid(),
                    ThreadId = threadId != Guid.Empty ? threadId : Guid.NewGuid(),
                    To = ownerUser.ID,
                    From = userId,
                    Msg = request.Msg,
                    Seen = false,
                    DateTCreated = DateTime.Now
                };

                unitOfWork.Message.Add(message);

                MailHelper mail = new MailHelper(ownerUser.Email, "JProps - Your have a new message",
                    mailingService.CreateRequestEmail(user, false), ownerUser.FirstName);

                unitOfWork.save();
                mail.SendMail();
                DashboardHub.BroadcastUserMessages(ownerUser.Email);

                txscope.Complete();
            }
        }

        /// <summary>
        /// Gets the total unseen messages
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public int GetUnseenMsgsCount(Guid userId, UnitOfWork unitOfWork)
        {
            try
            {
                return unitOfWork.Message.GetTotUnseenForUser(userId);
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while retrieving the total unseen messages for user + " + userId, ex);
                return 0;
            }
        }
    }
}