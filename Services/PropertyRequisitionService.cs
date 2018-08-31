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
    public class PropertyRequisitionService
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly UserService userService;

        public PropertyRequisitionService()
        {
            userService = new UserService();
        }

        /// <summary>
        /// Used by property owners to accept property requisitions
        /// It also generates an appropriate email to the property requestor
        /// notifying them that their requisition was successful
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="reqID"></param>
        /// <returns></returns>
        public bool AcceptPropertyRequisition(UnitOfWork unitOfWork, Guid reqID)
        {
            try
            {
                using (var txscope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    MailingService mailingService = new MailingService();
                    var requisition = unitOfWork.PropertyRequisition.Get(reqID);
                    var reqUser = requisition.User;
                    var property = requisition.Property;
                    var propertyUser = property.Owner.User;

                    var mail = mailingService.CreateRequisitionAcceptedEmail(propertyUser, reqUser);

                    //sets the accepted field of the requisition table to true for the accepted property request
                    requisition.IsAccepted = true;
                    requisition.Seen = true;
                    unitOfWork.save();

                    DashboardHub.alertRequisition(propertyUser.Email);
                    DashboardHub.alertRequisition(reqUser.Email);

                    mail.SendMail();

                    txscope.Complete();

                    return true;
                }
            }
            catch (Exception ex)
            {
                var msg = "Mail Exception - Unable to send out mail - Accept property requisition";
                log.Error(msg);
                throw new Exception(msg, ex);
            }
        }

        /// <summary>
        /// cancels any property request by either the property owner
        /// or the property requestor and sends the appropriate 
        /// email to the users.
        /// </summary>
        public bool CancelOrDenyPropertyRequisition(UnitOfWork unitOfWork, Guid reqID, bool isUserPropOwner)
        {
            try
            {
                using (var txscope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    MailingService mailingService = new MailingService();
                    var requisition = unitOfWork.PropertyRequisition.Get(reqID);
                    var ownerUser = requisition.Property.Owner.User;

                    MailHelper mail = mailingService.CreateRequisitionCancelledEmail(requisition, ownerUser, isUserPropOwner);

                    requisition.IsAccepted = null;
                    requisition.Seen = true;
                    unitOfWork.save();

                    DashboardHub.alertRequisition(ownerUser.Email);
                    DashboardHub.alertRequisition(requisition.User.Email);

                    mail.SendMail();

                    txscope.Complete();

                    return true;
                }
            }
            catch (Exception ex)
            {
                var msg = "Mail Exception - Unable to send out mail - CancelOrDenyPropertyRequisition";
                log.Error(msg);
                throw new Exception(msg, ex);
            }
        }

        /// <summary>
        /// returns the requisition history for any user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<RequisitionViewModel> GetRequisitionHistory(Guid userId, UnitOfWork unitOfWork)
        {
            return GetRequisitions(userId, true, unitOfWork);
        }

        /// <summary>
        /// returns the requisitions for a property owner or a property requestor
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shouldGetHist"></param>
        /// <returns></returns>
        public IEnumerable<RequisitionViewModel> GetRequisitions(Guid userId, bool shouldGetHist, UnitOfWork unitOfWork)
        {
            IEnumerable<RequisitionViewModel> requisitionInfo = null;
            IEnumerable<PropertyRequisition> requisitions = null;
            IEnumerable<PropertyRequisition> ownerRequisitions = null;
            IEnumerable<PropertyRequisition> requestorRequisitions = null;

            var userTypes = unitOfWork.UserTypeAssoc.GetUserTypesByUserID(userId);
            bool isUserPropOwner = userService.IsUserOfType(userTypes, EFPConstants.UserType.PropertyOwner);
            bool isUserConsumer = userService.IsUserOfType(userTypes, EFPConstants.UserType.Consumer);

            if (isUserPropOwner && isUserConsumer)
            {
                var owner = unitOfWork.Owner.GetOwnerByUserID(userId);

                if (!shouldGetHist)
                {
                    ownerRequisitions = unitOfWork.PropertyRequisition.GetRequestsByOwnerId(owner.ID);
                    requestorRequisitions = unitOfWork.PropertyRequisition.GetRequestsMadeByUserId(userId);
                }
                else
                {
                    ownerRequisitions = unitOfWork.PropertyRequisition.GetRequestHistoryByOwnerId(owner.ID);
                    requestorRequisitions = unitOfWork.PropertyRequisition.GetRequestsHistoryByUserId(userId);
                }

                var requisitionInfoOwner = PopulateRequisitionVMForOwner(unitOfWork, ownerRequisitions);
                var requisitionInfoRequestor = PopulateRequisitionVMForRequestor(unitOfWork, requestorRequisitions);

                requisitionInfo = requisitionInfoOwner.Concat(requisitionInfoRequestor);
            }
            else if (isUserPropOwner)
            {
                var owner = unitOfWork.Owner.GetOwnerByUserID(userId);

                if (!shouldGetHist)
                    requisitions = unitOfWork.PropertyRequisition.GetRequestsByOwnerId(owner.ID);
                else
                    requisitions = unitOfWork.PropertyRequisition.GetRequestHistoryByOwnerId(owner.ID);

                requisitionInfo = PopulateRequisitionVMForOwner(unitOfWork, requisitions);
            }
            else
            {
                if (!shouldGetHist)
                    requisitions = unitOfWork.PropertyRequisition.GetRequestsMadeByUserId(userId);
                else
                    requisitions = unitOfWork.PropertyRequisition.GetRequestsHistoryByUserId(userId);

                requisitionInfo = PopulateRequisitionVMForRequestor(unitOfWork, requisitions);
            }


            return requisitionInfo;
        }

        /// <summary>
        /// populates the RequisitionViewModel for the property owner
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="requisitions"></param>
        /// <returns></returns>
        public IEnumerable<RequisitionViewModel> PopulateRequisitionVMForOwner(UnitOfWork unitOfWork, IEnumerable<PropertyRequisition> requisitions)
        {
            List<RequisitionViewModel> requisitionInfo = null;

            if (requisitions != null)
            {
                requisitionInfo = new List<RequisitionViewModel>();

                foreach (var req in requisitions)
                {
                    RequisitionViewModel model = new RequisitionViewModel();

                    model.PropertyRequisition.User = new User();

                    model.ImageUrl = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(req.PropertyID);
                    model.PropertyRequisition.ID = req.ID;
                    model.PropertyRequisition.PropertyID = req.PropertyID;
                    model.PropertyRequisition.User.FirstName = req.User.FirstName;
                    model.PropertyRequisition.User.LastName = req.User.LastName;
                    model.PropertyRequisition.User.Email = req.User.Email;
                    model.PropertyRequisition.User.CellNum = req.User.CellNum;
                    model.PropertyRequisition.Msg = req.Msg;
                    model.PropertyRequisition.ExpiryDate = req.ExpiryDate;
                    model.PropertyRequisition.IsAccepted = req.IsAccepted;
                    model.PropertyRequisition.DateTCreated = req.DateTCreated;
                    model.isUserPropOwner = true;

                    requisitionInfo.Add(model);
                }
            }

            return requisitionInfo;
        }

        /// <summary>
        /// Populates the RequisitionViewModel for the property requestor
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="requisitions"></param>
        /// <returns></returns>
        public IEnumerable<RequisitionViewModel> PopulateRequisitionVMForRequestor(UnitOfWork unitOfWork, IEnumerable<PropertyRequisition> requisitions)
        {
            List<RequisitionViewModel> requisitionInfo = null;

            if (requisitions != null)
            {
                requisitionInfo = new List<RequisitionViewModel>();

                foreach (var req in requisitions)
                {
                    RequisitionViewModel model = new RequisitionViewModel();

                    model.PropertyRequisition.User = new User();

                    model.ImageUrl = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(req.PropertyID);
                    model.PropertyRequisition.ID = req.ID;
                    model.PropertyRequisition.PropertyID = req.PropertyID;
                    model.PropertyRequisition.User.FirstName = req.Property.Owner.User.FirstName;
                    model.PropertyRequisition.User.LastName = req.Property.Owner.User.LastName;
                    model.PropertyRequisition.User.CellNum = req.Property.Owner.User.CellNum;
                    model.PropertyRequisition.Msg = req.Msg;
                    model.PropertyRequisition.ExpiryDate = req.ExpiryDate;
                    model.PropertyRequisition.IsAccepted = req.IsAccepted;
                    model.PropertyRequisition.DateTCreated = req.DateTCreated;
                    model.isUserPropOwner = false;

                    requisitionInfo.Add(model);
                }
            }

            return requisitionInfo;
        }

        /// <summary>
        /// Gets the total unseen requisitions
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetUnseenReqsCount(Guid userId, UnitOfWork unitOfWork)
        {
            try
            {
                return unitOfWork.PropertyRequisition.GetTotUnseenForUser(userId);
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while retrieving the total unseen requisition for user + " + userId, ex);
                return 0;
            }
        }
    }
}