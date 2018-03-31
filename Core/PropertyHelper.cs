using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using SS.Code;
using SS.Models;
using SS.SignalR;
using SS.ViewModels;
using SS.ViewModels.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
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
        public static List<String> uploadedImageNames = new List<string>();//used to store names of uploaded images. Needed in the case of removing uploaded images during rollback
        private static List<String> searchResultPropertyTags = null;
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
        /// removes images that were uploaded
        /// </summary>
        /// <param name="fileNames"></param>
        private static void removeUploadedImages(List<String> fileNames)
        {
            foreach (var fileName in fileNames)
            {

                string path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads"), fileName);
                System.IO.File.Delete(path);
            }
        }
        /// <summary>
        /// removes an uploaded image
        /// </summary>
        /// <param name="fileName"></param>
        private static void removeUploadedImage(String fileName)
        {
            string path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads"), fileName);
            System.IO.File.Delete(path);
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

        public static List<FeaturedPropertiesSlideViewModel> PopulatePropertiesViewModel(PropertySearchViewModel model, Guid userId)
        {
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = new List<FeaturedPropertiesSlideViewModel>();

            IEnumerable<Property> filteredProperties = null;
            IEnumerable<Property> searchTermProperties = null;
            IEnumerable<Property> properties = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                List<Core.Filter> filters = createFilterList(model);
                var deleg = ExpressionBuilder.GetExpression<Property>(filters);

                filteredProperties = unitOfWork.Property.FindProperties(deleg, model.take, model.PgNo);
                searchTermProperties = unitOfWork.Property.FindPropertiesBySearchTerm(model.SearchTerm, model.PropertyCategory, model.take, model.PgNo);
                var combinedProperties = filteredProperties.Concat(searchTermProperties).Distinct();

                properties = getTaggedFilterPropsIfApplicable(combinedProperties, model, unitOfWork);

                //TODO optimize by removing extra calls to the database
                //this could be done via a single query
                foreach (var property in properties)
                {
                    IEnumerable<int> avgPropRatings = unitOfWork.PropertyRating.GetPropertyRatingsCountByPropertyId(property.ID);

                    var propertyModel = new FeaturedPropertiesSlideViewModel();

                    propertyModel.ID = property.ID.ToString();
                    propertyModel.StreetNumber = property.StreetNumber;
                    propertyModel.StreetAddress = property.StreetAddress;
                    propertyModel.Division = property.Division;
                    propertyModel.Country = property.Country;
                    propertyModel.PropertyType = property.PropertyType.Name;
                    propertyModel.Price = property.Price;
                    propertyModel.Description = property.Description;
                    propertyModel.TotalBedrooms = property.TotRooms.HasValue ? property.TotRooms.Value : 0;
                    propertyModel.TotalBathrooms = property.TotAvailableBathroom.HasValue ? property.TotAvailableBathroom.Value : 0;
                    propertyModel.User = property.Owner.User;
                    propertyModel.PropertyPrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(property.ID);
                    propertyModel.PropertyAverageRatings = avgPropRatings.Count() > 0 ? (int)avgPropRatings.Average() : 0;
                    propertyModel.IsPropertySaved = !userId.Equals(Guid.Empty) ? unitOfWork.SavedProperties.IsPropertySavedForUser(userId, property.ID) : false;

                    if (property.CategoryCode.Equals(EFPConstants.PropertyCategory.RealEstate))
                    {
                        propertyModel.isFurnished = isPropertyFurnished(property, unitOfWork);
                    }

                    if (property.CategoryCode.Equals(EFPConstants.PropertyCategory.Lot))
                    {
                        propertyModel.Area = property.Area.HasValue ? property.Area.Value : 0;
                        propertyModel.PropertyPurpose = property.PropertyPurpose.Name;
                    }

                    featuredPropertiesSlideViewModelList.Add(propertyModel);
                }
            }

            return featuredPropertiesSlideViewModelList;
        }

        public static List<FeaturedPropertiesSlideViewModel> PopulatePropertiesViewModel(List<NearbyPropertySearchModel> revisedModel, PropertySearchViewModel svModel, Guid userId)
        {
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = new List<FeaturedPropertiesSlideViewModel>();
            IEnumerable<Property> properties = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var saProperties = unitOfWork.Property.FindPropertiesByStreetAddress(revisedModel, svModel.take, svModel.PgNo);
                properties = getTaggedFilterPropsIfApplicable(saProperties, svModel, unitOfWork);

                //TODO optimize by removing extra calls to the database
                //this could be done via a single query
                foreach (var property in properties)
                {
                    IEnumerable<int> avgPropRatings = unitOfWork.PropertyRating.GetPropertyRatingsCountByPropertyId(property.ID);

                    var model = new FeaturedPropertiesSlideViewModel();

                    model.ID = property.ID.ToString();
                    model.StreetNumber = property.StreetNumber;
                    model.StreetAddress = property.StreetAddress;
                    model.Division = property.Division;
                    model.Country = property.Country;
                    model.PropertyType = property.PropertyType.Name;
                    model.Price = property.Price;
                    model.Description = property.Description;
                    model.TotalBedrooms = property.TotRooms.HasValue ? property.TotRooms.Value : 0;
                    model.TotalBathrooms = property.TotAvailableBathroom.HasValue ? property.TotAvailableBathroom.Value : 0;
                    model.User = property.Owner.User;
                    model.PropertyPrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(property.ID);
                    model.PropertyAverageRatings = avgPropRatings.Count() > 0 ? (int)avgPropRatings.Average() : 0;
                    model.IsPropertySaved = !userId.Equals(Guid.Empty) ? unitOfWork.SavedProperties.IsPropertySavedForUser(userId, property.ID) : false;

                    if (property.CategoryCode.Equals(EFPConstants.PropertyCategory.RealEstate))
                    {
                        model.isFurnished = isPropertyFurnished(property, unitOfWork);
                    }

                    if (property.CategoryCode.Equals(EFPConstants.PropertyCategory.Lot))
                    {
                        model.Area = property.Area.HasValue ? property.Area.Value : 0;
                        model.PropertyPurpose = property.PropertyPurpose.Name;
                    }

                    //matching the distance and durations to the property
                    int matchCount = revisedModel.Where(x => x.StreetAddress.Equals(property.StreetAddress)).Count();

                    if (matchCount > 0)
                    {
                        model.DistanceFromSearchedAddress = revisedModel.Where(x => x.StreetAddress.Equals(property.StreetAddress))
                            .Select(x => x.Distance).SingleOrDefault();

                        model.DuratiionFromSearchedAddress = revisedModel.Where(x => x.StreetAddress.Equals(property.StreetAddress))
                            .Select(x => x.Duration).SingleOrDefault();
                    }

                    featuredPropertiesSlideViewModelList.Add(model);
                }
            }

            return featuredPropertiesSlideViewModelList;
        }

        private static IEnumerable<Property> getTaggedFilterPropsIfApplicable(IEnumerable<Property> properties, PropertySearchViewModel model, UnitOfWork unitOfWork)
        {
            IEnumerable<Property> propertiesResult = null;

            if (model.Tags != null && model.Tags.Count() > 0)
            {
                propertiesResult = unitOfWork.Property.FilterPropertiesByTagNames(properties, model.Tags.Select(x => x.Key).ToList());
            }
            else
                propertiesResult = properties;

            searchResultPropertyTags = unitOfWork.Tags.GetTagNamesByProperties(properties);

            return propertiesResult;
        }

        private static bool isPropertyFurnished(Property property, UnitOfWork unitOfWork)
        {
            string furnishedTagVal = unitOfWork.Tags.GetTagNamesByPropertyId(property.ID)
                    .Where(x => x.ToString().Contains("Furnished")).SingleOrDefault();

            return furnishedTagVal != null ? true : false;
        }

        /// <summary>
        /// returns the property tags that were generated from the properties 
        /// search results
        /// </summary>
        /// <returns></returns>
        public static List<String> GetSearchResultPropertyTags()
        {
            return searchResultPropertyTags;
        }

        public static HomePageViewModel PopulateHomePageViewModel(int take, Guid userId)
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
                        PropertyType = property.PropertyType,
                        OwnerContactNum = property.Owner.User.CellNum,
                        AverageRating = avgPropRatings.Count() > 0 ? (int)avgPropRatings.Average() : 0,
                        IsPropertySaved = !userId.Equals(Guid.Empty) ? unitOfWork.SavedProperties.IsPropertySavedForUser(userId, property.ID) : false
                    };

                    if (property.CategoryCode.Equals(EFPConstants.PropertyCategory.RealEstate))
                    {
                        Temporary.TotRooms = property.TotRooms.Value;
                        Temporary.TotAvailableBathroom = property.TotAvailableBathroom.Value;

                        string furnishedTagVal = unitOfWork.Tags.GetTagNamesByPropertyId(property.ID)
                            .Where(x => x.ToString().Contains("Furnished")).SingleOrDefault();

                        Temporary.isFurnished = furnishedTagVal != null ? true : false;
                    }

                    if (property.CategoryCode.Equals(EFPConstants.PropertyCategory.Lot))
                    {
                        Temporary.Area = property.Area;
                        Temporary.PropertyPurpose = property.PropertyPurpose;
                    }

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
        public static List<Core.Filter> createFilterList(PropertySearchViewModel model)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

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
                if ((!string.IsNullOrEmpty(model.PropertyCategory) && String.IsNullOrEmpty(model.SearchTerm))
                    || (!String.IsNullOrEmpty(model.SearchTerm)
                        && model.SearchType.Equals("nearByPlaces")
                        && !string.IsNullOrEmpty(model.PropertyCategory)))
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

                //ad type
                if (!String.IsNullOrEmpty(model.RdoAdType))
                {
                    Core.Filter filter = new Core.Filter()
                    {
                        PropertyName = "AdTypeCode",
                        Operation = Op.Equals,
                        Value = mapPropertyAdTypeNameToCode(model.RdoAdType)
                    };

                    filters.Add(filter);
                }

                return filters;
            }
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

            if (!Roles.RoleExists("Admin"))
            {
                Roles.CreateRole(EFPConstants.RoleNames.Admin.ToString());
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
                To = "jamprops@hotmail.com",//emailTo,
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
            smtp.Credentials = new System.Net.NetworkCredential("jamprops@hotmail.com", "iGiPmT88*");
            smtp.EnableSsl = true;

            try
            {
                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                //log exception
                return false;
            }
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

                List<Core.Filter> filters = createFilterList(model);
                var deleg = ExpressionBuilder.GetExpression<Property>(filters);

                propertyCoordinates = unitOfWork.Property.FindPropertiesCoordinates(deleg);
            }

            return propertyCoordinates;
        }

        /// <summary>
        /// Populates the SelectedPropertyViewModel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SelectedPropertyViewModel GetProperty(Guid id)
        {
            SelectedPropertyViewModel ViewModel = new SelectedPropertyViewModel();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var property = unitOfWork.Property.Get(id);
                var user = property.Owner.User;

                ViewModel.ID = property.ID.ToString();
                ViewModel.StreetNumber = property.StreetNumber;
                ViewModel.StreetAddress = property.StreetAddress;
                ViewModel.Community = property.Community;
                ViewModel.Division = property.Division;
                ViewModel.Country = property.Country;
                ViewModel.PropertyType = property.PropertyType.Name;
                ViewModel.AdType = property.AdType.Name;
                ViewModel.TotalBedrooms = property.TotRooms.HasValue ? property.TotRooms.Value : 0;
                ViewModel.TotalBathrooms = property.TotAvailableBathroom.HasValue ? property.TotAvailableBathroom.Value : 0;
                ViewModel.PropertyCategoryCode = property.CategoryCode;
                ViewModel.PropertyCondition = property.PropertyCondition.Name;
                ViewModel.Occupancy = property.Occupancy.ToString();
                ViewModel.Price = property.Price;
                ViewModel.Description = property.Description;
                ViewModel.OwnerFirstName = user.FirstName;
                ViewModel.OwnerLastName = user.LastName;
                ViewModel.OwnerCellNumber = user.CellNum;
                ViewModel.PropertyPrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(property.ID);
                ViewModel.PropertyImageURLs = unitOfWork.PropertyImage.GetImageURLsByPropertyId(id, 0);
                ViewModel.PropRatings = unitOfWork.PropertyRating.GetPropertyRatingsByPropertyId(id);
                ViewModel.PropertyAverageRatings = ViewModel.PropRatings.Count() > 0 ? (int)ViewModel.PropRatings.Select(x => x.Ratings).Average() : 0;
                ViewModel.Tags = unitOfWork.Tags.GetTagNamesByPropertyId(id);

                return ViewModel;
            }
        }


        /// <summary>
        /// updates the property table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool UpdateProperty(UpdatePropertyViewModel model)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                Property property = unitOfWork.Property.Get(model.ID);

                property.Title = model.Title;
                property.Price = model.Price;
                property.SecurityDeposit = model.SecurityDeposit;
                property.TotRooms = model.TotRooms;
                property.TotAvailableBathroom = model.TotAvailableBathroom;
                property.Area = model.Area;
                property.Occupancy = model.Occupancy;
                property.GenderPreferenceCode = model.GenderPreferenceCode;
                property.IsReviewable = model.IsReviewable;
                property.Description = model.Description;
                property.DateTModified = DateTime.Now;

                try
                {
                    unitOfWork.save();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                    //log exception
                }
            }
        }

        public static bool UpdatePropertyPrimaryImg(Guid propertyId, Guid imgId)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var currentPrimaryImg = unitOfWork.PropertyImage.GetPrimaryImageByPropertyId(propertyId);
                PropertyImage newPrimaryImg = unitOfWork.PropertyImage.Get(imgId);

                currentPrimaryImg.IsPrimaryDisplay = false;
                newPrimaryImg.IsPrimaryDisplay = true;

                try
                {
                    unitOfWork.save();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                    //log exception
                }
            }
        }

        /// <summary>
        /// Removes the selected property image 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static object DeletePropertyImage(Guid ID)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                PropertyImage img = unitOfWork.PropertyImage.Get(ID);

                try
                {
                    unitOfWork.PropertyImage.Remove(img);
                    unitOfWork.save();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                    //log exception
                }
            }
        }

        /// <summary>
        /// associates the image with the property that was uploaded
        /// </summary>
        /// <param name="file"></param>
        public static void AssociateImagesWithProperty(UnitOfWork unitOfWork, HttpPostedFileBase[] files, Guid propertyID)
        {
            bool isPrimaryDisplay = true;//the first uploaded will be selected as the primary display

            foreach (var file in files)
            {
                string fileName = string.Empty;

                Guid guid = Guid.NewGuid();

                fileName = guid.ToString() + Path.GetExtension(file.FileName);

                PropertyImage propertyImage = new PropertyImage()
                {
                    ID = guid,
                    PropertyID = propertyID,
                    ImageURL = fileName,
                    IsPrimaryDisplay = isPrimaryDisplay,
                    DateTCreated = DateTime.Now
                };

                if (isPrimaryDisplay == true)
                    isPrimaryDisplay = false;

                unitOfWork.PropertyImage.Add(propertyImage);

                UploadPropertyImages(file, fileName);
            }
        }

        /// <summary>
        /// associates the image with the property that was uploaded
        /// </summary>
        /// <param name="file"></param>
        public static String AssociateImageWithProperty(HttpPostedFileBase file, Guid propertyID)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                string fileName = string.Empty;
                Guid guid = Guid.NewGuid();

                fileName = guid.ToString() + Path.GetExtension(file.FileName);

                PropertyImage propertyImage = new PropertyImage()
                {
                    ID = guid,
                    PropertyID = propertyID,
                    ImageURL = fileName,
                    IsPrimaryDisplay = false,
                    DateTCreated = DateTime.Now
                };

                try
                {
                    unitOfWork.PropertyImage.Add(propertyImage);
                    UploadPropertyImages(file, fileName);

                    unitOfWork.save();

                    return fileName;
                }
                catch (Exception ex)
                {
                    return null;
                    //log exception
                }
            }
        }

        /// <summary>
        /// uploads the property image to the server 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        public static void UploadPropertyImages(HttpPostedFileBase file, String fileName)
        {
            //ensures file is not empty and is a valid image
            if (file.ContentLength > 0 && file.ContentType.Contains("image"))
            {
                string path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads"), fileName);
                file.SaveAs(path);

                resizeFile(fileName);

                uploadedImageNames.Add(fileName);
            }
            else
                throw new Exception("Upload an image file");
        }

        /// <summary>
        /// Returns the update property view model that will be used when updating information
        /// on a property
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static UpdatePropertyViewModel GetUpdatePropertyVM(Guid ID)
        {
            Property property = null;
            UpdatePropertyViewModel newModel = null;
            UnitOfWork unitOfWork = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                unitOfWork = new UnitOfWork(dbCtx);

                property = unitOfWork.Property.Get(ID);

                newModel = new UpdatePropertyViewModel()
                {
                    ID = property.ID,
                    Price = property.Price,
                    SecurityDeposit = property.SecurityDeposit.HasValue ? property.SecurityDeposit.Value : 0,
                    TermsAgreement = property.TermsAgreement,
                    TotRooms = property.TotRooms.HasValue ? property.TotRooms.Value : 0,
                    TotAvailableBathroom = property.TotAvailableBathroom.HasValue ? property.TotAvailableBathroom.Value : 0,
                    Area = property.Area.HasValue ? property.Area.Value : 0,
                    IsReviewable = property.IsReviewable.HasValue ? property.IsReviewable.Value : false,
                    Occupancy = property.Occupancy.HasValue ? property.Occupancy.Value : 0,
                    Description = property.Description,
                    PropertyCategory = property.PropertyCategory.Name,
                    Title = property.Title,
                    GenderPreferenceCode = property.GenderPreferenceCode,
                    PrimaryImageUrl = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(ID),
                    PropertyImages = unitOfWork.PropertyImage.GetAllImagesByPropertyId(ID, 0)
                };

                return newModel;
            }
        }

        /// <summary>
        /// Returns the update property view model that will be used when updating information
        /// on a property
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static UpdatePropertyViewModel GetUpdatePropertyVM_PropertyImages(Guid ID)
        {
            Property property = null;
            UpdatePropertyViewModel newModel = null;
            UnitOfWork unitOfWork = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                unitOfWork = new UnitOfWork(dbCtx);

                property = unitOfWork.Property.Get(ID);

                newModel = new UpdatePropertyViewModel()
                {
                    PrimaryImageUrl = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(ID),
                    PropertyImages = unitOfWork.PropertyImage.GetAllImagesByPropertyId(ID, 0)
                };

                return newModel;
            }
        }

        /// <summary>
        /// creates user account as well as Advertise property
        /// and uploads the images associated with the property
        /// </summary>
        /// <param name="model"></param>
        /// <param name="errorModel"></param>
        public static void AdvertiseProperty(AdvertisePropertyViewModel model, Guid userId, ErrorModel errorModel)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                var doesUserExist = false;
                User user = null;

                using (var dbCtxTran = dbCtx.Database.BeginTransaction())
                {
                    try
                    {
                        if (model.Password != null && !model.Password.Equals(model.ConfirmPassword))
                            throw new Exception("The fields Password and Confirm Password are not equal");

                        PropertyHelper.createRolesIfNotExist();

                        var unitOfWork = new UnitOfWork(dbCtx);

                        if (!userId.Equals(Guid.Empty))
                        {
                            user = unitOfWork.User.Get(userId);
                        }
                        else
                        {
                            doesUserExist = unitOfWork.User.DoesUserExist(model.Email);
                            user = doesUserExist ? unitOfWork.User.GetUserByEmail(model.Email) : null;
                        }

                        //if user already exists and they are not a property owner, then associate user with that user type as well
                        //TODO: user's role will have to be manipulated as well
                        if (user != null)
                        {
                            var userTypes = unitOfWork.UserTypeAssoc.GetUserTypesByUserID(user.ID);
                            bool isUserPropOwner = PropertyHelper.isUserOfType(userTypes, EFPConstants.UserType.PropertyOwner);

                            if (!isUserPropOwner)
                                PropertyHelper.associateUserWithUserType(unitOfWork, user.ID, EFPConstants.UserType.PropertyOwner);
                        }
                        else
                        {
                            user = PropertyHelper.createUser(unitOfWork, EFPConstants.UserType.PropertyOwner, model.SubscriptionType, model.Email, model.FirstName,
                            model.LastName, model.CellNum, DateTime.MinValue);

                            PropertyHelper.createUserAccount(unitOfWork, model.Email, model.Password);
                        }

                        insertProperty(model, unitOfWork, user);

                        unitOfWork.save();
                        dbCtxTran.Commit();
                    }
                    catch (Exception ex)
                    {
                        dbCtxTran.Rollback();

                        if (PropertyHelper.uploadedImageNames != null && PropertyHelper.uploadedImageNames.Count > 0)
                        {
                            PropertyHelper.removeUploadedImages(PropertyHelper.uploadedImageNames);
                        }

                        errorModel.AddErrorMessage(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Inserts the property along with it's owner, subscription period and also the property images
        /// </summary>
        /// <param name="model"></param>
        private static void insertProperty(AdvertisePropertyViewModel model, UnitOfWork unitOfWork, User user)
        {
            bool doesOwnerExist = user != null && user.Owner.Select(x => x.ID).Count() > 0 ? true : false;
            Guid ownerID = doesOwnerExist ? user.Owner.Select(x => x.ID).Single() : Guid.NewGuid();

            Guid propertyID = Guid.NewGuid();
            String lat = String.Empty;
            String lng = String.Empty;

            //generate enrolment key for users with Landlord subscription
            if (!String.IsNullOrEmpty(model.SubscriptionType) && model.SubscriptionType.Equals(nameof(EFPConstants.PropertySubscriptionType.Landlord)))
            {
                model.EnrolmentKey = getRandomKey(6);
            }

            //Coordinate priority : streetaddress then community
            if (!String.IsNullOrEmpty(model.saCoordinateLat) && !String.IsNullOrEmpty(model.saCoordinateLng))
            {
                lat = model.saCoordinateLat;
                lng = model.saCoordinateLng;
            }
            else if (!String.IsNullOrEmpty(model.cCoordinateLat) && !String.IsNullOrEmpty(model.cCoordinateLng))
            {
                //check if lat and lng is already set; if they are then dont using community
                if (string.IsNullOrEmpty(lat) && string.IsNullOrEmpty(lng))
                {
                    lat = model.saCoordinateLat;
                    lng = model.saCoordinateLng;
                }
            }

            Property property = new Property()
            {
                ID = propertyID,
                Title = model.Title,
                OwnerID = ownerID,
                PurposeCode = PropertyHelper.mapPropertyPurposeNameToCode(model.PropertyPurpose),
                TypeID = unitOfWork.PropertyType.GetPropertyTypeIDByName(model.PropertyType),
                AdTypeCode = PropertyHelper.mapPropertyAdTypeNameToCode(model.AdvertismentType),
                AdPriorityCode = EFPConstants.PropertyAdPriority.Regular,//PropertyHelper.mapPropertyAdpriorityNameToCode(model.AdvertismentPriority),
                ConditionCode = EFPConstants.PropertyCondition.NotSurveyed,
                CategoryCode = unitOfWork.PropertyType.GetPopertyTypeCategoryCodeByName(model.PropertyType),
                StreetAddress = model.StreetAddress,
                Division = model.Division,
                Community = model.Community,
                NearByEstablishment = model.NearBy,
                Country = model.Country,
                Latitude = lat,
                Longitude = lng,
                NearByEstablishmentLat = model.nearByCoordinateLat,
                NearByEstablishmentLng = model.nearByCoordinateLng,
                Price = model.Price,
                SecurityDeposit = model.SecurityDeposit,
                Occupancy = model.Occupancy,
                GenderPreferenceCode = model.GenderPreferenceCode,
                Description = model.Description,
                Availability = true,
                EnrolmentKey = model.EnrolmentKey,
                TermsAgreement = model.TermsAgreement,
                TotAvailableBathroom = model.TotAvailableBathroom,
                TotRooms = model.TotRooms,
                Area = model.Area,
                IsReviewable = model.IsReviewable,
                DateTCreated = DateTime.Now
            };

            if (!String.IsNullOrEmpty(model.SubscriptionType))
            {
                Subscription subscription = new Subscription()
                {
                    ID = Guid.NewGuid(),
                    OwnerID = ownerID,
                    TypeCode = mapPropertySubscriptionTypeToCode(model.SubscriptionType),
                    Period = model.SubscriptionPeriod,
                    DateTCreated = DateTime.Now
                };

                unitOfWork.Subscription.Add(subscription);
            }

            if (!doesOwnerExist)
            {
                Guid guid = Guid.NewGuid();
                String fileName = String.Empty;

                if (model.organizationLogo != null)
                {
                    fileName = guid.ToString() + Path.GetExtension(model.organizationLogo.FileName);
                    UploadPropertyImages(model.organizationLogo, fileName);
                }

                Owner owner = new Owner()
                {
                    ID = ownerID,
                    UserID = user.ID,
                    Organization = model.Organization,
                    LogoUrl = fileName,
                    DateTCreated = DateTime.Now
                };

                unitOfWork.Owner.Add(owner);
            }

            unitOfWork.Property.Add(property);

            associateTagsWithProperty(unitOfWork, propertyID, model.selectedTags);
            PropertyHelper.AssociateImagesWithProperty(unitOfWork, model.flPropertyPics, propertyID);
        }

        /// <summary>
        /// Associates a property with the selected tags
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="propertyID"></param>
        /// <param name="selectedTags"></param>
        private static void associateTagsWithProperty(UnitOfWork unitOfWork, Guid propertyID, string[] selectedTags)
        {
            if (selectedTags != null)
            {
                foreach (var tag in selectedTags)
                {
                    Tags tags = new Tags
                    {
                        ID = Guid.NewGuid(),
                        PropertyID = propertyID,
                        TypeID = unitOfWork.TagType.GetTagTypeIDByTagName(tag),
                        DateTCreated = DateTime.Now
                    };

                    unitOfWork.Tags.Add(tags);
                }
            }
        }

        /// <summary>
        ///  checks if a user exists before creating a new one
        /// </summary>
        /// <returns></returns>
        private static bool doesUserExist(UnitOfWork unitOfWork, String email)
        {
            return unitOfWork.User.DoesUserExist(email);
        }

        /// <summary>
        /// used to produce a random key for the enrolment key 
        /// </summary>
        private static string getRandomKey(int size)
        {
            string input = "abcdefghijklmnopqrstuvwxyz0123456789";

            Random random = new Random();

            var chars = Enumerable.Range(0, size)
                        .Select(x => input[random.Next(0, input.Length)]);

            return new string(chars.ToArray());
        }

        //returns all properties owned by the current user that is signed in in json format
        public static List<Dictionary<String, String>> GetAllPropertyImages(Guid userId)
        {
            IEnumerable<PropertyImage> propertyImages;
            List<Dictionary<String, String>> imageList = new List<Dictionary<string, string>>();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var owner = unitOfWork.Owner.GetOwnerByUserID(userId);
                propertyImages = unitOfWork.PropertyImage.GetAllPrimaryPropertyImageByOwnerId(owner.ID);

                foreach (var image in propertyImages)
                {
                    Dictionary<String, String> imageInfo = new Dictionary<string, string>();

                    imageInfo.Add("propertyTitle", unitOfWork.Property.Get(image.PropertyID).Title);
                    imageInfo.Add("propertyID", image.PropertyID.ToString());
                    imageInfo.Add("imageURL", image.ImageURL);

                    imageList.Add(imageInfo);
                }
                return imageList;
            }
        }

        //returns all properties owned by the current user that is signed in in json format
        public static List<Dictionary<String, String>> GetAllSavedPropertyImages(Guid userId)
        {
            List<Dictionary<String, String>> imageList = new List<Dictionary<string, string>>();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var propertyImages = unitOfWork.SavedProperties.GetSavedPropertiesImagesByUserId(userId);

                foreach (var image in propertyImages)
                {
                    Dictionary<String, String> imageInfo = new Dictionary<string, string>();

                    imageInfo.Add("propertyTitle", unitOfWork.Property.Get(image.PropertyID).Title);
                    imageInfo.Add("propertyID", image.PropertyID.ToString());
                    imageInfo.Add("imageURL", image.ImageURL);

                    imageList.Add(imageInfo);
                }
                return imageList;
            }
        }

        /// <summary>
        /// Save liked property for users to review on their dashboards
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="propertyId"></param>
        public static bool SaveLikedProperty(Guid userId, Guid propertyId)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                SavedProperties savedProperties = new SavedProperties()
                {
                    ID = Guid.NewGuid(),
                    UserID = userId,
                    PropertyID = propertyId,
                    DateTCreated = DateTime.Now
                };

                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                try
                {
                    unitOfWork.SavedProperties.Add(savedProperties);
                    unitOfWork.save();
                    return true; //indicate success
                }
                catch (Exception ex)
                {
                    return false;//indicate failure
                }
            }
        }

        /// <summary>
        /// Populates and returns the account view model
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static ProfileViewModel PopulateProfileViewModel(Guid userId, bool isPropOwner)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                var user = unitOfWork.User.Get(userId);

                ProfileViewModel profileVM = new ProfileViewModel()
                {
                    ID = user.ID,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CellNum = user.CellNum,
                    Email = user.Email,
                };

                if (isPropOwner)
                {
                    var owner = unitOfWork.Owner.GetOwnerByUserID(userId);
                    profileVM.Organization = owner.Organization;
                    profileVM.LogoUrl = owner.LogoUrl;
                }

                return profileVM;
            }
        }

        public static SubscriptionViewModel PopulateSubscriptionViewModel(Guid userId)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
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

        /// <summary>
        /// Updates a user's profile information
        /// </summary>
        public static bool UpdateProfile(ProfileViewModel model)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var currentUser = unitOfWork.User.Get(model.ID);
                var currentOwner = unitOfWork.Owner.GetOwnerByUserID(model.ID);

                currentUser.FirstName = model.FirstName;
                currentUser.LastName = model.LastName;
                currentUser.CellNum = model.CellNum;
                currentUser.Email = model.Email;
                currentOwner.Organization = model.Organization;

                if (model.organizationLogo != null)
                {
                    currentOwner.LogoUrl = replaceUplodedImage(currentOwner.LogoUrl, model.organizationLogo);
                }

                try
                {
                    unitOfWork.save();
                    return true; // indicate success
                }
                catch (Exception ex)
                {
                    if (PropertyHelper.uploadedImageNames != null && PropertyHelper.uploadedImageNames.Count > 0)
                    {
                        PropertyHelper.removeUploadedImages(PropertyHelper.uploadedImageNames);
                    }
                    return false;//indicate failure
                }
            }
        }

        private static String replaceUplodedImage(string fileNameToRemove, HttpPostedFileBase fileToAdd)
        {
            if (!String.IsNullOrEmpty(fileNameToRemove))
                removeUploadedImage(fileNameToRemove);

            Guid guid = Guid.NewGuid();
            String fileName = String.Empty;

            if (fileToAdd != null)
            {
                fileName = guid.ToString() + Path.GetExtension(fileToAdd.FileName);
                UploadPropertyImages(fileToAdd, fileName);
            }

            return fileName;
        }
        /// <summary>
        /// Retrieves payment methods
        /// </summary>
        /// <returns></returns>
        public static List<PaymentMethod> GetPaymentMethods()
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                return unitOfWork.PaymentMethod.GetAll().ToList();
            }
        }
        /// <summary>
        /// Retrieves the payment methods names
        /// </summary>
        /// <returns></returns>
        public static List<string> GetPaymentMethodNames()
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                return unitOfWork.PaymentMethod.GetAllPaymentMethodNames();
            }
        }

        /// <summary>
        /// Gets a list of payments that were made by the property owner
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pgTake"></param>
        /// <param name="pgNo"></param>
        /// <returns></returns>
        public static IEnumerable<PaymentViewModel> GetPayments(int pgTake, int pgNo, Guid? userId)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
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
        }

        /// <summary>
        /// Gets the total payments made for a property owner
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static int GetPaymentsCount(Guid? userId)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                if (userId.HasValue)
                {
                    var user = unitOfWork.User.Get(userId.Value);
                    var owner = unitOfWork.Owner.GetOwnerByUserID(userId.Value);

                    return unitOfWork.Payment.GetPaymentsByOwnerIDCount(owner.ID);
                }
                else
                    return unitOfWork.Payment.GetAllPaymentsCount();
            }
        }
        /// <summary>
        /// Allow property owner to make a payment with the usage of their mobile credit
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool MakePayment(PaymentViewModel model)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

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

                try
                {
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

                    string userEmail = unitOfWork.Subscription.Get(model.SubscriptionID).Owner.User.Email;

                    if (sendPaymentReviewEmail(userEmail))
                    {
                        unitOfWork.save();
                        return true; //indicating success
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    return false; //indicating faliure
                }
            }
        }
        /// <summary>
        /// Sends an email to the property owner, indicating that their payment is
        /// currently being reviewed
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        private static bool sendPaymentReviewEmail(string userEmail)
        {
            string subject = "EasyFindProperties - Your payment is being reviewed";
            string body = "<p>Thank you for advertising your property on <b>EasyFindProperties</b></p>" +
                "<p>Your property will be displayed as soon as your payment has been verified.</p>" +
                "<p>You will be notified after payment verification</p>";

            return sendMail(userEmail, body, subject);
        }

        /// <summary>
        /// Gives admin an feedback medium to tell the validity of payment
        /// </summary>
        /// <param name="paymentID"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool VerifyPayment(Guid paymentID, Guid userId)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var userName = unitOfWork.User.Get(userId).Email;
                var payment = unitOfWork.Payment.Get(paymentID);
                var propertyOwnerID = payment.Subscription.Owner.ID;

                payment.IsVerified = true;
                payment.DateTModified = DateTime.Now;

                startSubscription(unitOfWork, payment, userName);
                extendSubscription(unitOfWork, payment, userName);//extending subscription date if necessary
                makePropertiesAvailableForOwner(unitOfWork, propertyOwnerID); //make properties available after payment

                try
                {
                    unitOfWork.save();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;//indicating failure
                }
            }
        }

        private static void makePropertiesAvailableForOwner(UnitOfWork unitOfWork, Guid propertyOwnerID)
        {
            var properties = unitOfWork.Property.GetPropertiesByOwnerId(propertyOwnerID);

            foreach (var property in properties)
            {
                property.Availability = true;
            }
        }
        /// <summary>
        /// Extends the subscription period for a property owner
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="payment"></param>
        /// <param name="userName"></param>
        private static void extendSubscription(UnitOfWork unitOfWork, Payment payment, string userName)
        {
            var hasSubscriptionExtension = unitOfWork.SubscriptionExtension.HasSubscriptionExtensionByPaymentID(payment.ID);

            if (hasSubscriptionExtension)
            {
                var subscriptionExt = unitOfWork.SubscriptionExtension.GetSubscriptionExtByPaymentID(payment.ID);
                var subscription = unitOfWork.Subscription.Get(payment.SubscriptionID);

                subscription.Period = subscription.Period + subscriptionExt.Period;
                subscription.ExpiryDate = subscription.ExpiryDate.Value.AddMonths(subscriptionExt.Period).AddDays(-1);
                subscription.DateTModified = DateTime.Now;
                subscription.ModifiedBy = userName;
            }
        }
        /// <summary>
        /// Starts subscription if it has not been started as yet upon
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="payment"></param>
        /// <param name="userName"></param>
        private static void startSubscription(UnitOfWork unitOfWork, Payment payment, string userName)
        {
            var subscription = unitOfWork.Subscription.Get(payment.SubscriptionID);

            if (!subscription.StartDate.HasValue)
            {
                subscription.StartDate = DateTime.Now;
                subscription.ExpiryDate = DateTime.Now.AddMonths(subscription.Period).AddDays(-1);
                subscription.DateTModified = DateTime.Now;
                subscription.ModifiedBy = userName;
            }
        }
        /// <summary>
        /// returns the names of the divisions
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetDivisionNames()
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                return unitOfWork.Division.GetAllDivisionNames();
            }
        }
        /// <summary>
        /// Remove properties that have been saved by users
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="propertyID"></param>
        /// <returns></returns>
        public static bool RemoveSavedProperty(Guid userId, Guid propertyID)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                try
                {
                    var savedProperty = unitOfWork.SavedProperties.GetSavedProperty(userId, propertyID);
                    unitOfWork.SavedProperties.Remove(savedProperty);

                    unitOfWork.save();
                    return true;
                }
                catch (Exception ex)
                {
                    //log exceptiion
                    return false;
                }
            }
        }

        /// <summary>
        /// recovers user password by generating a unique code
        /// and sending a email to the user
        /// </summary>
        /// <param name="email"></param>
        public static bool RecoverPassword(string email, ErrorModel errorModel)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var userExist = unitOfWork.User.DoesUserExist(email);

                if (!userExist)
                {
                    string errMessage = "The email address is not recognised";
                    errorModel.AddErrorMessage(errMessage);

                    return false;
                }

                var userId = unitOfWork.User.GetUserByEmail(email).ID;
                var uniqueKey = getRandomKey(5);

                savePasswordRecoveryRequest(unitOfWork, userId, uniqueKey);

                return sendRecoverPasswordEmail(email, userId, uniqueKey, errorModel);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="userId"></param>
        /// <param name="accessCode"></param>
        /// <param name="errorModel"></param>
        /// <returns></returns>
        private static bool sendRecoverPasswordEmail(string email, Guid userId, string accessCode, ErrorModel errorModel)
        {
            string emailTo = email;
            string subject = "EasyFindProperties - Password Recovery";
            string body = "Your access code to reset your password is <b>" + accessCode + "</b> ";
            body += "Please click the link below to reset your password: <br/>";
            body += EFPConstants.Application.Host + "/accounts/resetPassword?" + userId.ToString();
            body += "<br/><small>Your access code will expire 30 minutes after recieving this mail</small>";

            if (1==1)//sendMail(emailTo, body, subject))
            {
                return true;
            }
            else
            {
                string errMessage = "An unexpected error occurred while sending the recovery password <br /> Please try again later";
                errorModel.AddErrorMessage(errMessage);

                return false;
            }
        }

        private static void savePasswordRecoveryRequest(UnitOfWork unitOfWork, Guid userId, string accessCode)
        {
            PasswordRecoveryRequest pwdRecoveryRequest = new PasswordRecoveryRequest()
            {
                ID = Guid.NewGuid(),
                UserID = userId,
                AccessCode = accessCode,
                Processed = false,
                ExpiryDate = DateTime.Now.AddMinutes(10),
                DateTCreated = DateTime.Now
            };

            unitOfWork.PasswordRecoveryRequest.Add(pwdRecoveryRequest);
            unitOfWork.save();
        }

        /// <summary>
        /// resets a user's password
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        public static bool ResetPassword(Guid userId, string password, ErrorModel errorModel)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var user = unitOfWork.User.Get(userId);

                if (user == null)
                {
                    string errMessage = "The user does not exist";
                    errorModel.AddErrorMessage(errMessage);
                    return false;
                }

                var membershipUsr = Membership.GetUser(user.Email);

                if (membershipUsr == null)
                {
                    string errMessage = "The user does not exist";
                    errorModel.AddErrorMessage(errMessage);
                    return false;
                }

                try
                {
                    if (membershipUsr.IsLockedOut)
                        membershipUsr.UnlockUser();

                    var tempOldPassword = membershipUsr.ResetPassword();
                    membershipUsr.ChangePassword(tempOldPassword, password);

                    return true;
                }
                catch (MembershipPasswordException e)
                {
                    string errMessage = "Unable to reset your password : "+e.Message;
                    errorModel.AddErrorMessage(errMessage);
                    return false;
                }
                catch (Exception e)
                {
                    string errMessage = "An unexpected error occurred - please contact system administrator or try again later";
                    errorModel.AddErrorMessage(errMessage);
                    return false;
                }
            }
        }
        /// <summary>
        /// validates the access code used to reset a user's password
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accessCode"></param>
        /// <returns></returns>
        public static bool ValidateAccessCode(Guid userId, string accessCode)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                return unitOfWork.PasswordRecoveryRequest.DoesAccessCodeExistForUser(userId, accessCode);
            }
        }

        /// <summary>
        /// returns the subscription types
        /// </summary>
        /// <returns></returns>
        public static List<SubscriptionType> GetSubscriptionTypes()
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                return unitOfWork.SubscriptionType.GetAll().ToList();
            }
        }
    }
}

