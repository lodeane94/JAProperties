using log4net;
using SS.Core;
using SS.Models;
using SS.ViewModels.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using static SS.Core.EFPConstants;

namespace SS.Services
{
    public class PaymentService
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly PropertyService propertyService;
        private readonly SubscriptionService subscriptionService;

        public PaymentService()
        {
            propertyService = new PropertyService();
            subscriptionService = new SubscriptionService();
        }

        /// <summary>
        /// Retrieves payment methods
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public List<PaymentMethod> GetPaymentMethods(UnitOfWork unitOfWork)
        {
            return unitOfWork.PaymentMethod.GetAll().ToList();
        }

        /// <summary>
        /// Retrieves the payment methods names
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public List<string> GetPaymentMethodNames(UnitOfWork unitOfWork)
        {
            return unitOfWork.PaymentMethod.GetAllPaymentMethodNames();
        }

        /// <summary>
        /// Gets a list of payments that were made by the property owner
        /// </summary>
        /// <param name="pgTake"></param>
        /// <param name="pgNo"></param>
        /// <param name="userId"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public IEnumerable<PaymentViewModel> GetPayments(int pgTake, int pgNo, Guid? userId, UnitOfWork unitOfWork)
        {
            IEnumerable<Payment> payments = null;
            List<PaymentViewModel> paymentsViewModel = new List<PaymentViewModel>();

            if (userId.HasValue)
            {
                var user = unitOfWork.User.Get(userId.Value);
                var owner = unitOfWork.Owner.GetOwnerByUserID(userId.Value);
                payments = unitOfWork.Payment.GetPaymentsByOwnerID(owner.ID, pgTake, pgNo);
            }
            else
            {
                payments = unitOfWork.Payment.GetAllPaymentsOrdered(pgTake, pgNo);
            }

            foreach (var payment in payments)
            {
                var paymentVM = new PaymentViewModel()
                {
                    ID = payment.ID,
                    Email = !userId.HasValue ? unitOfWork.Payment.GetEmailForPayment(payment.ID) : null,
                    PaymentMethod = payment.PaymentMethod.Name,
                    Amount = payment.Amount,
                    VoucherNumber = payment.VoucherNumber,
                    IsVerified = payment.IsVerified,
                    DateTCreated = payment.DateTCreated,
                    DateTModified = payment.DateTModified.HasValue ? payment.DateTModified.Value : DateTime.MinValue
                };
                paymentsViewModel.Add(paymentVM);
            }

            return paymentsViewModel;
        }

        /// <summary>
        /// Gets the total payments count made for a property owner
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public int GetPaymentsCount(Guid? userId, UnitOfWork unitOfWork)
        {
            if (userId.HasValue)
            {
                var user = unitOfWork.User.Get(userId.Value);
                var owner = unitOfWork.Owner.GetOwnerByUserID(userId.Value);

                return unitOfWork.Payment.GetPaymentsByOwnerIDCount(owner.ID);
            }
            else
                return unitOfWork.Payment.GetAllPaymentsCount();
        }
        /// <summary>
        /// Allow property owner to make a payment with the usage of their mobile credit
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool MakePayment(PaymentViewModel model, UnitOfWork unitOfWork)
        {
            try
            {
                using (var txscope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    Payment payment = new Payment()
                    {
                        ID = Guid.NewGuid(),
                        SubscriptionID = model.SubscriptionID,
                        PaymentMethodID = model.PaymentMethodID,
                        Amount = model.Amount,
                        VoucherNumber = model.VoucherNumber,
                        IsVerified = false,
                        DateTCreated = DateTime.Now
                    };

                    unitOfWork.Payment.Add(payment);

                    if (model.IsExtension)
                    {
                        var subscriptionExtension = new SubscriptionExtension()
                        {
                            ID = Guid.NewGuid(),
                            PaymentID = payment.ID,
                            Period = model.Period,
                            DateTCreated = DateTime.Now
                        };

                        unitOfWork.SubscriptionExtension.Add(subscriptionExtension);
                    }

                    var user = unitOfWork.Subscription.Get(model.SubscriptionID).Owner.User;
                    var adminUser = unitOfWork.User.GetUserByEmail(Admin.Email);

                    unitOfWork.save();

                    if (!sendPaymentReviewEmail(user))
                        throw new Exception("Payment review email was not sent");

                    if (!sendPaymentMadeEmail(adminUser, user, payment))
                        throw new Exception("Payment made email was not sent");

                    txscope.Complete();

                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Payment unsuccessful", ex);
                return false; //indicating faliure
            }
        }

        /// <summary>
        /// sends email to the admin regarding new payments that are made
        /// </summary>
        /// <param name="adminUser"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool sendPaymentMadeEmail(User adminUser, User user, Payment payment)
        {
            string subject = "JProps - New Payment Recieved ";
            string body = "<p>A new payment of " + payment.Amount + " was made by " + user.Email + " . </p> " +
                "Please verify payment by going to the following link and log in using the admin account : " +
                "<br/> Go to JProps - http://www." + EFPConstants.Application.Host + "/landlordmanagement/dashboard ";

            MailHelper mail = new MailHelper(adminUser.Email, subject, body, adminUser.FirstName);

            if (mail.SendMail())
                return true;

            return false;
        }

        /// <summary>
        /// Sends an email to the property owner, indicating that their payment is
        /// currently being reviewed
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        private bool sendPaymentReviewEmail(User user)
        {
            string subject = "JProps - Your payment is being reviewed";
            string body = "<p>Thank you for advertising your property on <b>JProps</b></p>" +
                "<p>Your property will be displayed as soon as your payment has been verified.</p>" +
                "<p>You will be notified after payment verification</p>" +
                "<p>To make payments, action the following instructions: <ol><li><b> Sign in</b> " +
                "to your account using your recently created credentials</li> " +
                "<li>Select the <b>Account</b> link at the top the screen</li> " +
                "<li>Select the <b>Subscription link</b> </li> " +
                "<li>Click the <b>Make Payment link</b></li></ol></p> " +
                "<br/> Go to JProps - http://www." + EFPConstants.Application.Host + "/landlordmanagement/dashboard";

            MailHelper mail = new MailHelper(user.Email, subject, body, user.FirstName);

            if (mail.SendMail())
                return true;

            return false;
        }

        /// <summary>
        /// Sends an email to the property owner, indicating that their payment is
        /// is verified
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        private bool sendPaymentVerifiedEmail(User user)
        {
            string subject = "JProps - Your payment was successful";
            string body = "<p>Thank you for advertising your property on <b>JProps</b></p>" +
                "<p>Your payment was successfully verified. Your properties are now visible to the public.</p>" +
                "<p>Thank you</p>";

            MailHelper mail = new MailHelper(user.Email, subject, body, user.FirstName);

            if (mail.SendMail())
                return true;

            return false;
        }

        /// <summary>
        /// Gives admin an feedback medium to tell the validity of payment
        /// </summary>
        /// <param name="paymentID"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ErrorModel VerifyPayment(Guid paymentID, Guid userId, UnitOfWork unitOfWork)
        {
            ErrorModel errorModel = new ErrorModel();

            try
            {
                using (var txscope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    var userName = unitOfWork.User.Get(userId).Email;
                    var payment = unitOfWork.Payment.Get(paymentID);
                    var propertyOwnerID = payment.Subscription.Owner.ID;

                    payment.IsVerified = true;
                    payment.DateTModified = DateTime.Now;

                    var subscription = subscriptionService.StartSubscription(unitOfWork, payment, userName);
                    subscriptionService.ExtendSubscription(unitOfWork, payment, userName);//extending subscription date if necessary
                    propertyService.MakePropertiesAvailableForOwner(unitOfWork, subscription, propertyOwnerID, userName); //make properties available after payment

                    unitOfWork.save();

                    if (!sendPaymentVerifiedEmail(subscription.Owner.User))
                    {
                        String errString = "Unable to send verification email";
                        errorModel.AddErrorMessage(errString);
                    }
                    else
                        txscope.Complete();

                    return errorModel;
                }
            }
            catch (Exception ex)
            {
                errorModel.AddErrorMessage(ex.Message);
                log.Error("Payment verification error", ex);
                return errorModel;
            }

        }
    }
}