using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using SS.Code;
using SS.Models;
using SS.SignalR;
using SS.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Security;

namespace SS.Core
{
    public static class PropertyHelper
    {
        /// <summary>
        /// Function maps property category name to property code
        /// </summary>
        /// <param name="propertyCategoryName"></param>
        public static String mapPropertyCategoryNameToCode(String propertyCategoryName)
        {
            if (!String.IsNullOrEmpty(propertyCategoryName))
            {
                if (propertyCategoryName.Equals(nameof(EFPConstants.PropertyCategory.RealEstate)))
                {
                    return EFPConstants.PropertyCategory.RealEstate;
                }
                else if (propertyCategoryName.Equals(nameof(EFPConstants.PropertyCategory.Lot)))
                {
                    return EFPConstants.PropertyCategory.Lot;
                }

                return EFPConstants.PropertyCategory.Machinery;
            }

            return null;
        }

        /// <summary>
        /// Function maps property category code to property category name
        /// </summary>
        /// <param name="code"></param>
        public static String mapPropertyCategoryCodeToName(String code)
        {
            if (!String.IsNullOrEmpty(code))
            {
                if (code.Equals(EFPConstants.PropertyCategory.RealEstate))
                {
                    return nameof(EFPConstants.PropertyCategory.RealEstate);
                }
                else if (code.Equals(EFPConstants.PropertyCategory.Lot))
                {
                    return nameof(EFPConstants.PropertyCategory.Lot);
                }

                return nameof(EFPConstants.PropertyCategory.Machinery);
            }
            return null;
        }

        /// <summary>
        /// narrows the search result to those properties that are
        /// within the distance radius
        /// </summary>
        /// <param name="model"></param>
        /// <param name="distanceRadius"></param>
        /// <returns></returns>
        public static List<NearbyPropertySearchModel> NarrowSearchResultsToDistanceRadius(NearbyPropertySearchViewModel model, double distanceRadius)
        {
            List<NearbyPropertySearchModel> revisedModel = new List<NearbyPropertySearchModel>();
              
            foreach (var x in model.DestinationInformation)
            {
                double distance = (double)x.distance;

                if (distance <= distanceRadius)
                {
                    var nearbyPropertySearchModel = new NearbyPropertySearchModel()
                    {
                        StreetAddress = x.streetAddress,
                        Distance = x.distance,
                        Duration = x.duration
                    };

                    revisedModel.Add(nearbyPropertySearchModel);
                }
            }

            return revisedModel;
        }

        public static String mapPropertyPurposeNameToCode(String propertyPurposeName)
        {
            if (!String.IsNullOrEmpty(propertyPurposeName))
            {
                if (propertyPurposeName.Equals(nameof(EFPConstants.PropertyPurpose.Residential)))
                {
                    return EFPConstants.PropertyPurpose.Residential;
                }
                else if (propertyPurposeName.Equals(nameof(EFPConstants.PropertyPurpose.Commercial)))
                {
                    return EFPConstants.PropertyPurpose.Commercial;
                }

                return EFPConstants.PropertyPurpose.Industrial;
            }

            return null;
        }

        public static String mapPropertyAdTypeNameToCode(String propertyAdTypeName)
        {
            if (!String.IsNullOrEmpty(propertyAdTypeName))
            {
                if (propertyAdTypeName.Equals(nameof(EFPConstants.PropertyAdType.Rent)))
                {
                    return EFPConstants.PropertyAdType.Rent;
                }
                else if (propertyAdTypeName.Equals(nameof(EFPConstants.PropertyAdType.Sale)))
                {
                    return EFPConstants.PropertyAdType.Sale;
                }

                return EFPConstants.PropertyAdType.Lease;
            }

            return null;
        }

        public static String mapPropertyAdpriorityNameToCode(String adpriorityName)
        {
            if (!String.IsNullOrEmpty(adpriorityName))
            {
                if (adpriorityName.Equals(nameof(EFPConstants.PropertyAdPriority.AdPremium)))
                {
                    return EFPConstants.PropertyAdPriority.AdPremium;
                }
                else if (adpriorityName.Equals(nameof(EFPConstants.PropertyAdPriority.AdPro)))
                {
                    return EFPConstants.PropertyAdPriority.AdPro;
                }

                return EFPConstants.PropertyAdPriority.Regular;
            }

            return null;
        }

        public static String mapPropertySubscriptionTypeToCode(String subscriptionTypeName)
        {
            if (!String.IsNullOrEmpty(subscriptionTypeName))
            {
                if (subscriptionTypeName.Equals(nameof(EFPConstants.PropertySubscriptionType.Basic)))
                {
                    return EFPConstants.PropertySubscriptionType.Basic;
                }
                else if (subscriptionTypeName.Equals(nameof(EFPConstants.PropertySubscriptionType.Landlord)))
                {
                    return EFPConstants.PropertySubscriptionType.Landlord;
                }

                return EFPConstants.PropertySubscriptionType.Realtor;
            }

            return null;
        }

        /// <summary>
        /// removes images that were uploaded to the
        /// </summary>
        /// <param name="fileNames"></param>
        public static void removeUploadedImages(List<String> fileNames)
        {
            foreach (var fileName in fileNames)
            {

                string path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads"), fileName);
                System.IO.File.Delete(path);
            }
        }
        /// <summary>
        /// Resize uploaded image to 350 by 350
        /// </summary>
        /// <param name="fileName"></param>
        public static void resizeFile(String fileName)
        {
            Size size = new Size(0, 350);
            string path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads"), fileName);

            ISupportedImageFormat format = new JpegFormat { Quality = 70 };

            using (MemoryStream inStream = new MemoryStream(File.ReadAllBytes(path)))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                    {
                        imageFactory.Load(inStream)
                        .Resize(size)
                        .Format(format)
                        .Save(outStream);

                        //remove saved image
                        List<String> imageNameList = new List<string>();
                        imageNameList.Add(fileName);

                        removeUploadedImages(imageNameList);

                        using (FileStream writeStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                        {
                            int streamLength = (int)outStream.Length;
                            Byte[] buffer = new Byte[streamLength];

                            int bytesRead = outStream.Read(buffer, 0, streamLength);

                            while (bytesRead > 0)
                            {
                                writeStream.Write(buffer, 0, bytesRead);
                                bytesRead = outStream.Read(buffer, 0, streamLength);
                            }
                        }
                    }
                }
            }
        }

        public static List<FeaturedPropertiesSlideViewModel> PopulatePropertiesViewModel(PropertySearchViewModel model)
        {
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = new List<FeaturedPropertiesSlideViewModel>();

            IEnumerable<Property> filteredProperties = null;
            IEnumerable<Property> searchTermProperties = null;
            IEnumerable<Property> properties = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                List<Core.Filter> filters = createFilterList(model, unitOfWork);
                var deleg = ExpressionBuilder.GetExpression<Property>(filters);

                filteredProperties = unitOfWork.Property.FindProperties(deleg, model.take, model.PgNo);
                searchTermProperties = unitOfWork.Property.FindPropertiesBySearchTerm(model.SearchTerm, model.take, model.PgNo);
                properties = filteredProperties.Concat(searchTermProperties).Distinct();

                //TODO optimize by removing extra calls to the database
                //this could be done via a single query
                foreach (var property in properties)
                {
                    IEnumerable<int> avgPropRatings = unitOfWork.PropertyRating.GetPropertyRatingsCountByPropertyId(property.ID);

                    featuredPropertiesSlideViewModelList.Add(new FeaturedPropertiesSlideViewModel()
                    {
                        property = property,
                        propertyImageURLs = null,
                        propertyPrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(property.ID),
                        averageRating = avgPropRatings.Count() > 0 ? (int)avgPropRatings.Average() : 0
                    });
                }
            }

            return featuredPropertiesSlideViewModelList;
        }

        public static List<FeaturedPropertiesSlideViewModel> PopulatePropertiesViewModel(List<NearbyPropertySearchModel> revisedModel)
        {
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = new List<FeaturedPropertiesSlideViewModel>();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var properties = unitOfWork.Property.FindPropertiesByStreetAddress(revisedModel);

                //TODO optimize by removing extra calls to the database
                //this could be done via a single query
                foreach (var property in properties)
                {
                    IEnumerable<int> avgPropRatings = unitOfWork.PropertyRating.GetPropertyRatingsCountByPropertyId(property.ID);

                    var model = new FeaturedPropertiesSlideViewModel();

                    model.property = property;
                    model.propertyImageURLs = null;
                    model.propertyPrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(property.ID);
                    model.averageRating = avgPropRatings.Count() > 0 ? (int)avgPropRatings.Average() : 0;
                    
                    //matching the distance and durations to the property
                    int matchCount = revisedModel.Where(x => x.StreetAddress.Equals(property.StreetAddress)).Count();

                    if (matchCount > 0)
                    {
                        model.Distance = revisedModel.Where(x => x.StreetAddress.Equals(property.StreetAddress))
                            .Select(x => x.Distance).SingleOrDefault();

                        model.Duration = revisedModel.Where(x => x.StreetAddress.Equals(property.StreetAddress))
                            .Select(x => x.Duration).SingleOrDefault();
                    }

                    featuredPropertiesSlideViewModelList.Add(model);
                }
            }

            return featuredPropertiesSlideViewModelList;
        }

        public static HomePageViewModel PopulateHomePageViewModel(int take)
        {
            HomePageViewModel ViewModel = new HomePageViewModel();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                FeaturedProperty Temporary = new FeaturedProperty();
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                IEnumerable<int> avgPropRatings;

                foreach (var property in unitOfWork.Property.GetFeaturedProperties(take))
                {
                    avgPropRatings = unitOfWork.PropertyRating.GetPropertyRatingsCountByPropertyId(property.ID);

                    Temporary = new FeaturedProperty
                    {
                        ID = property.ID,
                        PrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(property.ID),
                        Price = property.Price,
                        StreetAddress = property.StreetAddress,
                        Division = property.Division,
                        Country = property.Country,
                        Description = property.Description,
                        AverageRating = avgPropRatings.Count() > 0 ? (int)avgPropRatings.Average() : 0
                    };

                    if (property.AdTypeCode.Equals(EFPConstants.PropertyAdType.Rent))
                    {
                        Temporary.ShowRating = true;
                        ViewModel.FeaturedRental.Add(Temporary);
                    }

                    if (property.AdTypeCode.Equals(EFPConstants.PropertyAdType.Sale))
                    {
                        ViewModel.FeaturedSale.Add(Temporary);
                    }

                    if (property.AdTypeCode.Equals(EFPConstants.PropertyAdType.Lease))
                    {
                        ViewModel.FeaturedLease.Add(Temporary);
                    }
                }
            };

            return ViewModel;
        }

        public static String MakeHttpRequest(String Url)
        {
            StreamReader sre = null;
            try
            {
                HttpWebRequest r = (HttpWebRequest)WebRequest.Create(Url);
                r.Method = "Get";
                HttpWebResponse res = (HttpWebResponse)r.GetResponse();
                Stream sr = res.GetResponseStream();
                sre = new StreamReader(sr);
                return sre.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public static bool isUserOfType(IEnumerable<String> userTypes, String userType)
        {
            var count = userTypes.Where(x => x.Equals(userType)).Count();

            return count > 0 ? true : false;
        }

        /// <summary>
        /// Create a filter list to be used for property searching purposes
        /// </summary>
        /// <param name="model"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        private static List<Core.Filter> createFilterList(PropertySearchViewModel model, UnitOfWork unitOfWork)
        {
            List<Core.Filter> filters = new List<Core.Filter>();

            //country
            if (!string.IsNullOrEmpty(model.Country))
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "Country",
                    Operation = Op.Equals,
                    Value = model.Country
                };

                filters.Add(filter);
            }

            //division
            if (!string.IsNullOrEmpty(model.Division))
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "Division",
                    Operation = Op.Equals,
                    Value = model.Division
                };

                filters.Add(filter);
            }

            //property category
            if (!string.IsNullOrEmpty(model.PropertyCategory))
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "CategoryCode",
                    Operation = Op.Equals,
                    Value = model.PropertyCategory
                };

                filters.Add(filter);
            }

            //property type
            if (!string.IsNullOrEmpty(model.PropertyType))
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "TypeID",
                    Operation = Op.Equals,
                    Value = unitOfWork.PropertyType.GetPropertyTypeIDByName(model.PropertyType)
                };

                filters.Add(filter);
            }

            //property purpose
            if (!string.IsNullOrEmpty(model.PropertyPurpose))
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "PurposeCode",
                    Operation = Op.Equals,
                    Value = PropertyHelper.mapPropertyPurposeNameToCode(model.PropertyPurpose)
                };

                filters.Add(filter);
            }

            //price range
            if (model.MinPrice >= 0 && model.MaxPrice > 0)
            {
                Core.Filter filter1 = new Core.Filter()
                {
                    PropertyName = "Price",
                    Operation = Op.GreaterThanOrEqual,
                    Value = model.MinPrice
                };

                Core.Filter filter2 = new Core.Filter()
                {
                    PropertyName = "price",
                    Operation = Op.LessThanOrEqual,
                    Value = model.MaxPrice
                };
                filters.Add(filter1);
                filters.Add(filter2);
            }
            ///////////////TODO implement Or conditions for these///////////////////////
            //ad type sale
            if (model.ChkBuyProperty)
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "AdTypeCode",
                    Operation = Op.Equals,
                    Value = EFPConstants.PropertyAdType.Sale
                };

                filters.Add(filter);
            }

            //ad type rent
            if (model.ChkRentProperty)
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "AdTypeCode",
                    Operation = Op.Equals,
                    Value = EFPConstants.PropertyAdType.Rent
                };

                filters.Add(filter);
            }

            //ad type lease
            if (model.ChkLeasedProperty)
            {
                Core.Filter filter = new Core.Filter()
                {
                    PropertyName = "AdTypeCode",
                    Operation = Op.Equals,
                    Value = EFPConstants.PropertyAdType.Lease
                };

                filters.Add(filter);
            }

            return filters;
        }

        //TODO every one should get an account only realtor and landlord can add unlimited amount of properties without paying the extra cost
        /// <summary>
        /// Creates a membership account for the property owner
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="userType"></param>
        /// <param name="model"></param>
        public static void createUserAccount(UnitOfWork unitOfWork, String email, String password)
        {
            createRolesIfNotExist();

            MembershipCreateStatus status = new MembershipCreateStatus();

            MembershipUser newUser = Membership.CreateUser(email, password, email, "null", "null", true, out status);

            if (newUser == null)
            {
                throw new Exception(GetMembershipErrorMessage(status));
            }
        }

        /// <summary>
        /// Creates a user object
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="userType"></param>
        /// <param name="subscriptionType"></param>
        /// <param name="email"></param>
        /// <param name="fName"></param>
        /// <param name="lName"></param>
        /// <param name="cellNum"></param>
        /// <returns></returns>
        public static User createUser(UnitOfWork unitOfWork, String userType, String subscriptionType
            , String email, String fName, String lName, String cellNum, DateTime dob)
        {
            User user = null;
            var userID = Guid.NewGuid();

            if (!dob.Equals(DateTime.MinValue))
            {
                user = new User()
                {
                    ID = userID,
                    FirstName = fName,
                    LastName = lName,
                    CellNum = cellNum,
                    Email = email,
                    DOB = dob,
                    DateTCreated = DateTime.Now
                };
            }
            else
            {
                user = new User()
                {
                    ID = userID,
                    FirstName = fName,
                    LastName = lName,
                    CellNum = cellNum,
                    Email = email,
                    DateTCreated = DateTime.Now
                };

            }

            unitOfWork.User.Add(user);

            associateUserWithUserType(unitOfWork, userID, userType);
            addUserToRespectedRole(email, subscriptionType);

            return user;
        }

        public static void associateUserWithUserType(UnitOfWork unitOfWork, Guid userID, String userType)
        {
            UserTypeAssoc userTypeAssoc = new UserTypeAssoc()
            {
                ID = Guid.NewGuid(),
                UserID = userID,
                UserTypeCode = userType,
                DateTCreated = DateTime.Now
            };

            unitOfWork.UserTypeAssoc.Add(userTypeAssoc);
        }

        /// <summary>
        /// adds user to it's respected role and generate enrolment key if necessary
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subscriptionType"></param>
        public static void addUserToRespectedRole(string email, string subscriptionType)
        {
            if (subscriptionType.Equals(nameof(EFPConstants.PropertySubscriptionType.Landlord)))
            {
                Roles.AddUserToRole(email, EFPConstants.RoleNames.Landlord.ToString());
            }
            else if (subscriptionType.Equals(nameof(EFPConstants.PropertySubscriptionType.Realtor)))
            {
                Roles.AddUserToRole(email, EFPConstants.RoleNames.Realtor.ToString());
            }
            else if (subscriptionType.Equals(nameof(EFPConstants.PropertySubscriptionType.Basic)))
            {
                Roles.AddUserToRole(email, EFPConstants.RoleNames.Basic.ToString());
            }
            /*removed since subscription type is only for paid users
            else if (subscriptionType.Equals(EFPConstants.RoleNames.Tennant.ToString()))
            {
                Roles.AddUserToRole(email, EFPConstants.RoleNames.Tennant.ToString());
            }
            else
                Roles.AddUserToRole(email, EFPConstants.RoleNames.Consumer.ToString());*/
        }

        /// <summary>
        /// creates lanlord,tennant and realtor roles if they dont exist
        /// </summary>
        public static void createRolesIfNotExist()
        {
            if (!Roles.RoleExists("Basic"))
            {
                Roles.CreateRole(EFPConstants.RoleNames.Basic.ToString());
            }

            if (!Roles.RoleExists("Landlord"))
            {
                Roles.CreateRole(EFPConstants.RoleNames.Landlord.ToString());
            }

            if (!Roles.RoleExists("Realtor"))
            {
                Roles.CreateRole(EFPConstants.RoleNames.Realtor.ToString());
            }

            /*
             if (!Roles.RoleExists("Tennant"))
            {
                Roles.CreateRole(EFPConstants.RoleNames.Tennant.ToString());
            }            

            if (!Roles.RoleExists("Consumer"))
            {
                Roles.CreateRole(EFPConstants.RoleNames.Consumer.ToString());
            }*/
        }

        /// <summary>
        /// deletes all messages from a message thread by retrieving all messages 
        /// sent to another user and and received from this same user then
        /// adding this list into the message trash. The messages list is refreshed after
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="userId"></param>
        /// <param name="messageId"></param>
        public static void deleteMsgsFromMsgThread(UnitOfWork unitOfWork, Guid userId, Guid messageId)
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
        /// populates the RequisitionViewModel for the property owner
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="requisitions"></param>
        /// <returns></returns>
        public static List<RequisitionViewModel> populateRequisitionVMForOwner(UnitOfWork unitOfWork, IEnumerable<PropertyRequisition> requisitions)
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
        public static List<RequisitionViewModel> populateRequisitionVMForRequestor(UnitOfWork unitOfWork, IEnumerable<PropertyRequisition> requisitions)
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
                    model.PropertyRequisition.IsAccepted = req.IsAccepted;
                    model.PropertyRequisition.DateTCreated = req.DateTCreated;
                    model.isUserPropOwner = false;

                    requisitionInfo.Add(model);
                }
            }

            return requisitionInfo;
        }

        /// <summary>
        /// Used by property owners to accept property requisitions
        /// It also generates an appropriate email to the property requestor
        /// notifying them that their requisition was successful
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="reqID"></param>
        /// <returns></returns>
        public static bool AcceptPropertyRequisition(UnitOfWork unitOfWork, Guid reqID)
        {
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

            if (propCategoryCode.Equals(EFPConstants.PropertyCategory.RealEstate)
                &&
                (adTypeCode.Equals(EFPConstants.PropertyAdType.Rent)
                || adTypeCode.Equals(EFPConstants.PropertyAdType.Lease)))
            {
                //generated key that is used to associate each tennant to their landlord
                string enrolmentKey = property.EnrolmentKey;

                body = "Congratulations!!, your property request was accepted ." + "Your enrolment key is " + enrolmentKey +
                            ". " + "If you wish to accommodate this room, please click on the following link and enter your email address and the enrolment key that was provided to you" +
                          " localhost:5829/accounts/requisition/?propId=" + property.ID + "&requestId=" + reqID;

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

                return true;
            }
            else
                throw new Exception("Mail Exception");
        }

        /// <summary>
        /// cancels any property request by either the property owner
        /// or the property requestor and sends the appropriate 
        /// email to the users.
        /// </summary>
        public static bool CancelOrDenyPropertyRequisition(UnitOfWork unitOfWork, Guid reqID, bool isUserPropOwner)
        {
            string body = String.Empty;
            string subject = String.Empty;
            string emailTo = String.Empty;

            var requisition = unitOfWork.PropertyRequisition.Get(reqID);

            if (isUserPropOwner)
            {
                body = "The owner of the property have declined your requisition";
                subject = "EasyFindProperties - Property Requisition Declined";
                emailTo = requisition.User.Email;
            }
            else
            {
                body = "The property requisition has been cancelled";
                subject = "EasyFindProperties - Property Requisition Cancelled";
                emailTo = requisition.Property.Owner.User.Email;
            }

            if (sendMail(emailTo, body, subject))
            {
                requisition.IsAccepted = null;
                unitOfWork.save();

                return true;
            }
            else
                throw new Exception("Mail Exception");
        }

        /// <summary>
        /// sends email from the application server
        /// </summary>
        /// <param name="emailTo"></param>
        /// <param name="body"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static bool sendMail(string emailTo, string body, string subject)
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

        //used for membership error message
        private static string GetMembershipErrorMessage(MembershipCreateStatus status)
        {
            switch (status)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Email address already exists. Please sign into your portal to add more properties";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.\nThe passwords should be 4 characters long";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        /// <summary>
        /// Retrieves the coordinates of properties based on conditions
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Array PopulateModelForPropertyCoordinates(PropertySearchViewModel model)
        {
            Array propertyCoordinates = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                List<Core.Filter> filters = createFilterList(model, unitOfWork);
                var deleg = ExpressionBuilder.GetExpression<Property>(filters);

                propertyCoordinates = unitOfWork.Property.FindPropertiesCoordinates(deleg);
            }

            return propertyCoordinates;
        }

        public static SelectedPropertyViewModel GetProperty(Guid id)
        {
            SelectedPropertyViewModel ViewModel = new SelectedPropertyViewModel();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                ViewModel.property = unitOfWork.Property.Get(id);
                ViewModel.AdType = ViewModel.property.AdType.Name;
                ViewModel.PropertyCondition = ViewModel.property.PropertyCondition.Name;
                ViewModel.owner = unitOfWork.Owner.Get(ViewModel.property.OwnerID);
                ViewModel.OwnerFirstName = ViewModel.owner.User.FirstName;
                ViewModel.OwnerLastName = ViewModel.owner.User.LastName;
                ViewModel.OwnerCellNumber = ViewModel.owner.User.CellNum;
                ViewModel.tags = unitOfWork.Tags.GetTagNamesByPropertyId(id);
                ViewModel.propertyImageURLs = unitOfWork.PropertyImage.GetImageURLsByPropertyId(id, 0);
                ViewModel.propertyPrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(id);

                ViewModel.propRatings = unitOfWork.PropertyRating.GetPropertyRatingsByPropertyId(id);
                ViewModel.averageRating = ViewModel.propRatings.Count() > 0 ? (int)ViewModel.propRatings.Select(x => x.Ratings).Average() : 0;

                return ViewModel;
            }
        }
    }
}

