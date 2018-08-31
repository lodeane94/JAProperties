using log4net;
using SS.Core;
using SS.Models;
using SS.ViewModels.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static SS.Core.EFPConstants;

namespace SS.Services
{
    public class SubscriptionService
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly PropertyService propertyService;
        private String adAccessErrMessage;

        public SubscriptionService()
        {
            propertyService = new PropertyService();
            adAccessErrMessage = String.Empty;
        }

        /// <summary>
        /// Starts subscription if it has not been started as yet upon
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="payment"></param>
        /// <param name="userName"></param>
        public Subscription StartSubscription(UnitOfWork unitOfWork, Payment payment, string userName)
        {
            var subscription = unitOfWork.Subscription.Get(payment.SubscriptionID);

            if (!subscription.StartDate.HasValue)
            {
                subscription.StartDate = DateTime.Now;
                subscription.ExpiryDate = DateTime.Now.AddMonths(subscription.Period).AddDays(-1);
                subscription.IsActive = true;
                subscription.DateTModified = DateTime.Now;
                subscription.ModifiedBy = userName;
            }

            return subscription;
        }

        /// <summary>
        /// Extends the subscription period for a property owner
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="payment"></param>
        /// <param name="userName"></param>
        public void ExtendSubscription(UnitOfWork unitOfWork, Payment payment, string userName)
        {
            var hasSubscriptionExtension = unitOfWork.SubscriptionExtension.HasSubscriptionExtensionByPaymentID(payment.ID);

            if (hasSubscriptionExtension)
            {
                var subscriptionExt = unitOfWork.SubscriptionExtension.GetSubscriptionExtByPaymentID(payment.ID);
                var subscription = unitOfWork.Subscription.Get(payment.SubscriptionID);

                if (subscription.IsActive && subscription.ExpiryDate.HasValue)
                {
                    if (subscription.ExpiryDate.Value < DateTime.Now)
                    {
                        subscription.ExpiryDate = DateTime.Now.AddMonths(subscription.Period).AddDays(-1);
                        subscription.Period = subscriptionExt.Period;
                    }
                    else
                    {
                        subscription.ExpiryDate = subscription.ExpiryDate.Value.AddMonths(subscription.Period).AddDays(-1);
                        subscription.Period = subscription.Period + subscriptionExt.Period;
                    }

                    subscription.DateTModified = DateTime.Now;
                    subscription.ModifiedBy = userName;
                }
                else
                    throw new Exception("Cannot extend a non-active subscription");
            }
        }

        /// <summary>
        /// returns the subscription types
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public List<SubscriptionType> GetSubscriptionTypes(UnitOfWork unitOfWork)
        {
            return unitOfWork.SubscriptionType.GetAll().ToList();
        }

        /// <summary>
        /// Changes the user's subscription 
        /// Compare subsciption prices to determine if a payment will be required 
        /// for subscription change i.e. if it is an upgrade
        /// </summary>
        /// <param name="subscriptionID"></param>
        /// <param name="subscriptionTypeName"></param>
        /// <param name="period"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public RequestModel ChangeSubscription(Guid subscriptionID, String subscriptionTypeName, int? period, UnitOfWork unitOfWork)
        {
            var requestModel = new RequestModel();
            var userName = String.Empty;

            try
            {
                var subscription = unitOfWork.Subscription.Get(subscriptionID);
                userName = subscription.Owner.User.Email;
                subscription.IsActive = false;

                var newSubscriptionType = unitOfWork.SubscriptionType
                .GetSubscriptionTypeByID(PropertyHelper.MapPropertySubscriptionTypeToCode(subscriptionTypeName));

                Subscription newSubscription = new Subscription()
                {
                    ID = Guid.NewGuid(),
                    OwnerID = subscription.OwnerID,
                    TypeCode = PropertyHelper.MapPropertySubscriptionTypeToCode(subscriptionTypeName),
                    DateTCreated = DateTime.Now
                };

                //ensure that users who have not subscribed can change their subscription

                if (newSubscriptionType.MonthlyCost < subscription.SubscriptionType.MonthlyCost)
                {
                    var msg = "Subscription was changed successfully. You are now on the <b>" + newSubscriptionType.Name + "</b> subscription";

                    newSubscription.Period = PropertyHelper.DateDiff(Intervals.Months, DateTime.Now, subscription.ExpiryDate.Value);
                    newSubscription.StartDate = DateTime.Now;
                    newSubscription.ExpiryDate = subscription.ExpiryDate.Value;
                    newSubscription.IsActive = true;

                    requestModel.AddMessage(msg);

                    propertyService.MakePropertiesAvailableForOwner(unitOfWork, newSubscription, Guid.Empty, userName);
                }
                else
                {
                    if (period.HasValue)
                    {
                        var msg = "A payment of " + (newSubscriptionType.MonthlyCost * period.Value) + " is required to activate your subscription. <br />";
                        msg += "Your properties will not be displayed until payment is confirmed";

                        newSubscription.Period = period.Value;
                        newSubscription.IsActive = false;

                        requestModel.AddMessage(msg);
                    }
                    else
                    {
                        String errMessage = "Period must have a value for successful subscription change from ";
                        errMessage += subscription.SubscriptionType.Name + " to " + newSubscriptionType.Name;

                        requestModel.AddErrorMessage(errMessage);

                        throw new Exception(errMessage);
                    }
                }

                unitOfWork.Subscription.Add(newSubscription);
                unitOfWork.save();
            }
            catch (Exception ex)
            {
                var msg = "An error occurred while changing the subscription type. Please contact system administrator";
                requestModel.AddErrorMessage(msg);
                log.Error(msg, ex);
                return requestModel;
            }

            return requestModel;
        }

        /// <summary>
        /// Gets the subscription type of a user
        /// </summary>
        /// <param name="subscriptionID"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public SubscriptionType GetSubscriptionTypeByUserSubId(Guid subscriptionID, UnitOfWork unitOfWork)
        {
            return unitOfWork.Subscription.Get(subscriptionID).SubscriptionType;
        }

        /// <summary>
        ///  Renews the basic subscription for a user
        /// </summary>
        /// <param name="subscriptionID"></param>
        /// <param name="userId"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public RequestModel RenewSubscription(Guid subscriptionID, Guid userId, UnitOfWork unitOfWork)
        {
            var requestModel = new RequestModel();
            var msg = String.Empty;

            try
            {
                var userName = unitOfWork.User.Get(userId).Email;
                var subscription = unitOfWork.Subscription.Get(subscriptionID);
                var propertyOwnerID = subscription.OwnerID;

                subscription.ExpiryDate = DateTime.Now.AddMonths(1).AddDays(-1);
                subscription.Period = 1;
                subscription.DateTModified = DateTime.Now;
                subscription.ModifiedBy = userName;

                propertyService.MakePropertiesAvailableForOwner(unitOfWork, subscription, propertyOwnerID, userName);
                unitOfWork.save();

                msg = "<p>Your subscription was successfully renewed</p><p>Your property will now be visible to the public</p>";
                requestModel.AddMessage(msg);

                return requestModel;
            }
            catch (Exception ex)
            {
                msg = "An error occurred while renewing your subscription. Please contact system administrator";
                requestModel.AddErrorMessage(msg);
                log.Error(msg, ex);

                return requestModel;
            }

        }

        /// <summary>
        /// checks for an active subscription that is associated with 
        /// that email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public RequestModel SubscriptionCheck(string email, UnitOfWork unitOfWork)
        {
            RequestModel requestModel = new RequestModel();

            try
            {
                var doesUserExist = unitOfWork.User.DoesUserExist(email);

                if (!doesUserExist)
                {
                    requestModel.AddBool(false);
                }
                else
                {
                    var user = unitOfWork.User.GetUserByEmail(email);
                    var owner = unitOfWork.Owner.GetOwnerByUserID(user.ID);

                    if (owner != null)
                    {
                        var subscription = unitOfWork.Subscription.GetSubscriptionByOwnerID(owner.ID);
                        requestModel.AddBool(subscription != null ? true : false);
                    }
                    else
                        requestModel.AddBool(false);
                }

                return requestModel;
            }
            catch (Exception ex)
            {
                var msg = "An error occurred while checking subscription - Contact system administrator";
                requestModel.AddErrorMessage(msg);
                log.Error(msg, ex);

                return requestModel;
            }
        }

        /// <summary>
        /// validates the number of properties that can be published 
        /// based on the subscription type
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsAdAccessValid(Guid userId, UnitOfWork unitOfWork)
        {
            try
            {
                var ownerId = unitOfWork.Owner.GetOwnerByUserID(userId).ID;
                var propertiesCount = unitOfWork.Property.GetCount(ownerId);
                var subscription = unitOfWork.Subscription.GetSubscriptionByOwnerID(ownerId);

                switch (subscription.SubscriptionType.ID)
                {
                    case PropertySubscriptionType.Basic:
                        if ((propertiesCount + 1) > 3)
                        {
                            adAccessErrMessage = "Basic subscription is only limited to 3 properties "
                                + "Please upgrade your subscription to add more than 3 properties";
                            return false;
                        }
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                adAccessErrMessage = "An error occurred while validating ad access - Contact system administrator";
                log.Error(adAccessErrMessage, ex);

                return false;
            }
        }

        /// <summary>
        /// returns the ad access error message to the user
        /// </summary>
        /// <returns></returns>
        public String GetAdAccessErrMessage()
        {
            return adAccessErrMessage;
        }

        /// <summary>
        /// Cancels a user's subscription and removes the associated properties
        /// </summary>
        /// <param name="subscriptionID"></param>
        /// <param name="userId"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public RequestModel CancelSubscription(Guid subscriptionID, Guid userId, UnitOfWork unitOfWork)
        {
            var requestModel = new RequestModel();
            var msg = String.Empty;

            try
            {
                var userName = unitOfWork.User.Get(userId).Email;
                var subscription = unitOfWork.Subscription.Get(subscriptionID);
                var propertyOwnerID = subscription.OwnerID;

                subscription.ExpiryDate = DateTime.Now;
                subscription.Period = 0;
                subscription.IsActive = false;
                subscription.DateTModified = DateTime.Now;
                subscription.ModifiedBy = userName;

                propertyService.RemovePropertiesForOwner(unitOfWork, propertyOwnerID);
                unitOfWork.save();

                msg = "<p>Your subscription was successfully cancelled</p><p>All properties have been removed from your account</p>";
                requestModel.AddMessage(msg);

                return requestModel;
            }
            catch (Exception ex)
            {
                msg = "An error occurred while cancelling your subscription. Please contact system administrator";
                requestModel.AddErrorMessage(msg);
                log.Error(msg, ex);

                return requestModel;
            }

        }

        /// <summary>
        /// Populates the subscription view model based on the user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public SubscriptionViewModel PopulateSubscriptionViewModel(Guid userId, UnitOfWork unitOfWork)
        {
            var user = unitOfWork.User.Get(userId);
            var owner = unitOfWork.Owner.GetOwnerByUserID(userId);
            var subscription = unitOfWork.Subscription.GetSubscriptionByOwnerID(owner.ID);
            var subscriptionType = unitOfWork.SubscriptionType.GetSubscriptionTypeByID(subscription.TypeCode);

            SubscriptionViewModel model = new SubscriptionViewModel();
            model.ID = subscription.ID;
            model.Name = subscriptionType.Name;
            model.MonthlyCost = subscriptionType.MonthlyCost;
            model.Period = subscription.Period;
            model.Description = subscriptionType.Description;
            model.StartDate = subscription.StartDate.HasValue ? subscription.StartDate.Value : DateTime.MinValue;
            model.ExpiryDate = subscription.ExpiryDate.HasValue ? subscription.ExpiryDate.Value : DateTime.MinValue;

            return model;
        }
    }
}