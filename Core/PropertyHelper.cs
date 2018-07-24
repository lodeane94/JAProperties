using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using log4net;
using SS.Models;
using SS.SignalR;
using SS.ViewModels;
using SS.ViewModels.Management;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Security;
using static SS.Core.EFPConstants;

namespace SS.Core
{
    public static class PropertyHelper
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static List<String> uploadedImageNames = new List<string>();//used to store names of uploaded images. Needed in the case of removing uploaded images during rollback
        private static List<String> searchResultPropertyTags = null;
        private static String adAccessErrMessage = String.Empty;
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
                else if (propertyCategoryName.Equals(nameof(EFPConstants.PropertyCategory.Machinery)))
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
                else if (code.Equals(EFPConstants.PropertyCategory.Machinery))
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

            ISupportedImageFormat format = new JpegFormat { Quality = 100 };

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

        /// <summary>
        /// retrieves the total properties found for a normal search
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async static Task<int> GetPropertiesCount(PropertySearchViewModel model)
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

                filteredProperties = await unitOfWork.Property.FindProperties(deleg, 0, 0);
                searchTermProperties = await unitOfWork.Property.FindPropertiesBySearchTerm(model.SearchTerm, model.PropertyCategory, 0, 0);
                var combinedProperties = filteredProperties.Concat(searchTermProperties).Distinct();

                properties = getTaggedFilterPropsIfApplicable(combinedProperties, model, unitOfWork);
            }

            return properties.Count();
        }

        /// <summary>
        /// retrieves the total properties found for a near by search
        /// </summary>
        /// <param name="revisedModel"></param>
        /// <param name="svModel"></param>
        /// <returns></returns>
        public async static Task<int> GetNearbyPropertiesCount(List<NearbyPropertySearchModel> revisedModel, PropertySearchViewModel svModel)
        {
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = new List<FeaturedPropertiesSlideViewModel>();
            IEnumerable<Property> properties = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var saProperties = await unitOfWork.Property.FindPropertiesByStreetAddress(revisedModel, 0, 0);
                properties = getTaggedFilterPropsIfApplicable(saProperties, svModel, unitOfWork);
            }

            return properties.Count();
        }

        /// <summary>
        /// retrieve all properties based on the criteria 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<List<FeaturedPropertiesSlideViewModel>> PopulatePropertiesViewModel(PropertySearchViewModel model, Guid userId)
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

                filteredProperties = await unitOfWork.Property.FindProperties(deleg, model.take, model.PgNo);
                searchTermProperties = await unitOfWork.Property.FindPropertiesBySearchTerm(model.SearchTerm, model.PropertyCategory, model.take, model.PgNo);
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
                    propertyModel.AdType = property.AdType.Name;
                    propertyModel.DateAddedModified = property.DateTModified.HasValue ? property.DateTModified.Value.ToShortDateString() : property.DateTCreated.ToShortDateString();

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
        /// <summary>
        /// returns the requisitions for a property owner or a property requestor
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shouldGetHist"></param>
        /// <returns></returns>
        public static IEnumerable<RequisitionViewModel> GetRequisitions(Guid userId, bool shouldGetHist)
        {
            IEnumerable<RequisitionViewModel> requisitionInfo = null;
            IEnumerable<PropertyRequisition> requisitions = null;
            IEnumerable<PropertyRequisition> ownerRequisitions = null;
            IEnumerable<PropertyRequisition> requestorRequisitions = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var userTypes = unitOfWork.UserTypeAssoc.GetUserTypesByUserID(userId);
                bool isUserPropOwner = PropertyHelper.isUserOfType(userTypes, EFPConstants.UserType.PropertyOwner);
                bool isUserConsumer = PropertyHelper.isUserOfType(userTypes, EFPConstants.UserType.Consumer);

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

                    var requisitionInfoOwner = PropertyHelper.populateRequisitionVMForOwner(unitOfWork, ownerRequisitions);
                    var requisitionInfoRequestor = PropertyHelper.populateRequisitionVMForRequestor(unitOfWork, requestorRequisitions);

                    requisitionInfo = requisitionInfoOwner.Concat(requisitionInfoRequestor);
                }
                else if (isUserPropOwner)
                {
                    var owner = unitOfWork.Owner.GetOwnerByUserID(userId);

                    if (!shouldGetHist)
                        requisitions = unitOfWork.PropertyRequisition.GetRequestsByOwnerId(owner.ID);
                    else
                        requisitions = unitOfWork.PropertyRequisition.GetRequestHistoryByOwnerId(owner.ID);

                    requisitionInfo = PropertyHelper.populateRequisitionVMForOwner(unitOfWork, requisitions);
                }
                else
                {
                    if (!shouldGetHist)
                        requisitions = unitOfWork.PropertyRequisition.GetRequestsMadeByUserId(userId);
                    else
                        requisitions = unitOfWork.PropertyRequisition.GetRequestsHistoryByUserId(userId);

                    requisitionInfo = PropertyHelper.populateRequisitionVMForRequestor(unitOfWork, requisitions);
                }
            }

            return requisitionInfo;
        }

        /// <summary>
        /// Allows viewers to request the property or ask a question about the property
        /// </summary>
        /// <param name="request"></param>
        /// <param name="contactPurpose"></param>
        public static void RequestProperty(PropertyRequisition request, string contactPurpose, Guid userId, ErrorModel errorModel)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                String errMsg = String.Empty;
                User ownerUser = unitOfWork.Property.GetPropertyOwnerByPropID(request.PropertyID).User;
                User user = unitOfWork.User.Get(userId);

                if (contactPurpose.Equals("requisition"))
                {
                    PropertyRequisition requisition = new PropertyRequisition()
                    {
                        ID = Guid.NewGuid(),
                        UserID = userId,
                        PropertyID = request.PropertyID,
                        Msg = request.Msg,
                        IsAccepted = false,
                        ExpiryDate = DateTime.Now.AddDays(7),//requisition should last for a week
                        DateTCreated = DateTime.Now
                    };

                    unitOfWork.PropertyRequisition.Add(requisition);

                    MailHelper mail = new MailHelper(ownerUser.Email, "JProps - Your have a property requisition",
                        createRequestEmail(user, true), ownerUser.FirstName);

                    if (mail.SendMail())
                    {
                        unitOfWork.save();

                        var userTo = unitOfWork.Property.GetPropertyOwnerByPropID(request.PropertyID).User;
                        DashboardHub.alertRequisition(userTo.Email);
                    }
                    else
                    {
                        errMsg = "Unable to send request confirmation email";
                        errorModel.AddErrorMessage(errMsg);

                        throw new Exception(errMsg);
                    }
                }
                else
                {
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
                        createRequestEmail(user, false), ownerUser.FirstName);

                    if (mail.SendMail())
                    {
                        unitOfWork.save();

                        DashboardHub.BroadcastUserMessages(ownerUser.Email);
                    }
                    else
                    {
                        errMsg = "Unable to send message sent confirmation email";
                        errorModel.AddErrorMessage(errMsg);

                        throw new Exception(errMsg);
                    }
                }
            }
        }

        /// <summary>
        /// Create the email that is send to alert a property owner
        /// that a message / request came in
        /// </summary>
        /// <param name="req"></param>
        /// <param name="isRequisition"></param>
        /// <returns></returns>
        private static string createRequestEmail(User user, bool isRequisition)
        {
            String body = String.Empty;

            if (isRequisition)
            {
                body = "<p>" + user.FirstName + " " + user.LastName + " is requesting your property.</p> ";
                body += "<p>Click the following link to go to your portal ";
                body += "http://www." + EFPConstants.Application.Host + "/landlordmanagement/dashboard</p>";
            }
            else
            {
                body = "</p>" + user.FirstName + " " + user.LastName + " sent a message to you regarding your property.</p> ";
                body += "Click the following link to go to your portal ";
                body += "http://www." + EFPConstants.Application.Host + "/landlordmanagement/dashboard</p>";
            }

            return body;
        }

        public async static Task<List<FeaturedPropertiesSlideViewModel>> PopulatePropertiesViewModel(List<NearbyPropertySearchModel> revisedModel, PropertySearchViewModel svModel, Guid userId)
        {
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = new List<FeaturedPropertiesSlideViewModel>();
            IEnumerable<Property> properties = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                var saProperties = await unitOfWork.Property.FindPropertiesByStreetAddress(revisedModel, svModel.take, svModel.PgNo);
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
                    model.AdType = property.AdType.Name;
                    model.DateTCreated = property.DateTCreated;
                    model.DateAddedModified = property.DateTModified.HasValue ? property.DateTModified.Value.ToShortDateString() : property.DateTCreated.ToShortDateString();

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
                            .Select(x => x.Distance).FirstOrDefault();

                        model.DuratiionFromSearchedAddress = revisedModel.Where(x => x.StreetAddress.Equals(property.StreetAddress))
                            .Select(x => x.Duration).FirstOrDefault();
                    }

                    featuredPropertiesSlideViewModelList.Add(model);
                }
            }
            //TODO add sort button on page
            return featuredPropertiesSlideViewModelList
                    .OrderBy(x => double.Parse(x.DistanceFromSearchedAddress))
                    .ThenBy(x => x.Price)
                    .ThenByDescending(x => x.DateTCreated).ToList();
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

        public async static Task<HomePageViewModel> PopulateHomePageViewModel(int take, Guid userId)
        {
            log.Info("Populating home page");

            HomePageViewModel ViewModel = new HomePageViewModel();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                FeaturedProperty Temporary = new FeaturedProperty();
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                IEnumerable<int> avgPropRatings;

                try
                {
                    foreach (var property in await unitOfWork.Property.GetFeaturedProperties(take))
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
                            IsPropertySaved = !userId.Equals(Guid.Empty) ? unitOfWork.SavedProperties.IsPropertySavedForUser(userId, property.ID) : false,
                            DateAddedModified = property.DateTModified.HasValue ? property.DateTModified.Value.ToShortDateString() : property.DateTCreated.ToShortDateString()
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
                }
                catch (Exception ex)
                {
                    log.Error("Error occurred while loading properties for home", ex);
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
                log.Error("Http Request to " + Url + " failed", ex);
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

                //only include the availability filter unless it is accompanied by other filters
                if (filters.Count > 0)
                {
                    //availability
                    Core.Filter availabilityFilter = new Core.Filter()
                    {
                        PropertyName = "Availability",
                        Operation = Op.Equals,
                        Value = true
                    };

                    filters.Add(availabilityFilter);
                }

                return filters;
            }
        }

        /// <summary>
        /// Creates a membership account for the property owner
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="userType"></param>
        /// <param name="model"></param>
        public static void createUserAccount(String email, String password)
        {
            MembershipCreateStatus status = new MembershipCreateStatus();

            MembershipUser newUser = Membership.CreateUser(email, password, email, "null", "null", true, out status);

            if (newUser == null)
            {
                throw new Exception(GetMembershipErrorMessage(status));
            }
        }

        /// <summary>
        /// removes a membership account for the property owner
        /// </summary>
        /// <param name="email"></param>
        public static bool RemoveUserAccount(String email)
        {
            return Membership.DeleteUser(email, true);
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
                    FirstName = MiscellaneousHelper.UppercaseFirst(fName),
                    LastName = MiscellaneousHelper.UppercaseFirst(lName),
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
                    FirstName = MiscellaneousHelper.UppercaseFirst(fName),
                    LastName = MiscellaneousHelper.UppercaseFirst(lName),
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
            createRolesIfNotExist();

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
        public static IEnumerable<RequisitionViewModel> populateRequisitionVMForOwner(UnitOfWork unitOfWork, IEnumerable<PropertyRequisition> requisitions)
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
        public static IEnumerable<RequisitionViewModel> populateRequisitionVMForRequestor(UnitOfWork unitOfWork, IEnumerable<PropertyRequisition> requisitions)
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
            //var propCategoryCode = property.CategoryCode;
            //var adTypeCode = property.AdTypeCode;

            //email address which acceptance letter should be sent to
            string emailTo = reqUser.Email;
            string subject = "JProps - Property Requisition Accepted";
            //body of the email
            string body = string.Empty;

            /*
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
                body = "Congratulations!!, your property request was accepted. The property owner will contact you with any additional information "
                        + "or concerns. Thank you for using JProps.";*/

            body = "Congratulations!!, your property request was accepted. Please make the necessary communication with the property owner to acquire any additional information "
                        + "to secure your reservation. Thank you for using JProps.";

            //getting information about the owner of the property to give back to the requestee
            body += "<br/><br/><strong>Property Owner Information</strong><br/><br/> First Name:&nbsp;"
                + propertyUser.FirstName + "<br/><br/>Last Name:&nbsp;" + propertyUser.LastName
                            + "<br/><br/>Cellphone Number:&nbsp;" + propertyUser.CellNum + "<br/><br/>Email:&nbsp;" + propertyUser.Email;

            MailHelper mail = new MailHelper(emailTo, subject, body, reqUser.FirstName);

            if (mail.SendMail())
            {
                //sets the accepted field of the requisition table to true for the accepted property request
                requisition.IsAccepted = true;
                requisition.Seen = true;
                unitOfWork.save();

                DashboardHub.alertRequisition(propertyUser.Email);
                DashboardHub.alertRequisition(reqUser.Email);

                return true;
            }
            else
            {
                log.Error("Mail Exception - Unable to send out mail - Accept property requisition");
                throw new Exception("Mail Exception");
            }

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
            var ownerUser = requisition.Property.Owner.User;

            if (isUserPropOwner)
            {
                body = "<p> We are sorry to advise you that " +
                        " the owner ( " + ownerUser.FirstName + " " + ownerUser.LastName + " ) of the property have declined your requisition." +
                        " Please feel free to request more properties. <br /> http://www." + EFPConstants.Application.Host + "</p>";

                subject = "JProps - Property Requisition Declined";
                emailTo = requisition.User.Email;
            }
            else
            {
                body = "<p> The property requisition for (" + requisition.User.FirstName + " " + requisition.User.LastName + " ) has been cancelled. </p>";
                subject = "JProps - Property Requisition Cancelled";
                emailTo = requisition.Property.Owner.User.Email;
            }

            MailHelper mail = new MailHelper(emailTo, subject, body, requisition.User.FirstName);

            if (mail.SendMail())
            {
                requisition.IsAccepted = null;
                requisition.Seen = true;
                unitOfWork.save();

                DashboardHub.alertRequisition(ownerUser.Email);
                DashboardHub.alertRequisition(requisition.User.Email);

                return true;
            }
            else
            {
                log.Error("Mail Exception - Unable to send out mail - CancelOrDenyPropertyRequisition");
                throw new Exception("Mail Exception");
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
        public async static Task<Array> PopulateModelForPropertyCoordinates(PropertySearchViewModel model)
        {
            Array propertyCoordinates = null;

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                List<Core.Filter> filters = createFilterList(model);
                var deleg = ExpressionBuilder.GetExpression<Property>(filters);

                propertyCoordinates = await unitOfWork.Property.FindPropertiesCoordinates(deleg);
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
                ViewModel.SecurityDeposit = property.SecurityDeposit.HasValue ? property.SecurityDeposit.Value : 0;
                ViewModel.Area = property.Area.HasValue ? property.Area.Value : 0;
                ViewModel.Description = property.Description;
                ViewModel.OwnerFirstName = user.FirstName;
                ViewModel.OwnerLastName = user.LastName;
                ViewModel.OwnerCellNumber = user.CellNum;
                ViewModel.PropertyPrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(property.ID);
                ViewModel.PropertyImageURLs = unitOfWork.PropertyImage.GetImageURLsByPropertyId(id, 0);
                ViewModel.PropRatings = unitOfWork.PropertyRating.GetPropertyRatingsByPropertyId(id);
                ViewModel.PropertyAverageRatings = ViewModel.PropRatings.Count() > 0 ? (int)ViewModel.PropRatings.Select(x => x.Ratings).Average() : 0;
                ViewModel.Tags = unitOfWork.Tags.GetTagNamesByPropertyId(id);
                ViewModel.isAvailable = property.Availability;
                ViewModel.DateAddedModified = property.DateTModified.HasValue ? property.DateTModified.Value.ToShortDateString() : property.DateTCreated.ToShortDateString();

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
                    log.Error("Update property failed", ex);
                    return false;
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
                    log.Error("Update property primary image failed", ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// Removes the selected property image 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static bool DeletePropertyImage(Guid ID)
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
                    log.Error("Property deletion failed + " + img.ToString(), ex);
                    return false;
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
                    log.Error("Error occurred while adding image for property ID + " + propertyID, ex);
                    return null;
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
                    PropertyCategory = property.PropertyCategory.ID,
                    Title = property.Title,
                    GenderPreferenceCode = property.GenderPreferenceCode,
                    AdType = property.AdType.ID,
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
                var propertyId = Guid.Empty;
                var isUserCreated = false;
                User user = null;

                try
                {
                    var unitOfWork = new UnitOfWork(dbCtx);

                    validateAdRequest(model, errorModel);

                    if (!userId.Equals(Guid.Empty))
                    {
                        user = unitOfWork.User.Get(userId);
                    }
                    else
                    {
                        user = unitOfWork.User.GetUserByEmail(model.Email);
                        user = user ?? unitOfWork.User.GetUserByCellNum(model.CellNum);
                    }

                    //if user already exists and they are not a property owner, then associate user with that user type as well
                    if (user != null)
                    {
                        //ensuring that the user's email matches
                        if (!model.Email.Equals(user.Email))
                        {
                            String errMsg = "There is already an account that is registered to that cell number. Please use that email address instead";
                            errorModel.AddErrorMessage(errMsg);
                            throw new Exception(errMsg);
                        }

                        var userTypes = unitOfWork.UserTypeAssoc.GetUserTypesByUserID(user.ID);
                        bool isUserPropOwner = PropertyHelper.isUserOfType(userTypes, EFPConstants.UserType.PropertyOwner);

                        if (!isUserPropOwner)
                            PropertyHelper.associateUserWithUserType(unitOfWork, user.ID, EFPConstants.UserType.PropertyOwner);
                    }
                    else
                    {
                        user = PropertyHelper.createUser(unitOfWork, EFPConstants.UserType.PropertyOwner, model.SubscriptionType, model.Email, model.FirstName,
                        model.LastName, model.CellNum, DateTime.MinValue);

                        PropertyHelper.createUserAccount(model.Email, model.Password);

                        isUserCreated = true;
                    }

                    propertyId = insertProperty(model, unitOfWork, user);

                    if (!isUserCreated)
                        unitOfWork.save();
                    else
                    {
                        unitOfWork.save();
                        if (!SendUserCreatedEmail(user, true))
                            throw new Exception("Unable to send user created email");
                    }
                }
                catch (Exception ex)
                {
                    if (!String.IsNullOrEmpty(model.Email) && isUserCreated)
                        RemoveUserAccount(model.Email);

                    if(!propertyId.Equals(Guid.Empty))
                        RemoveProperty(propertyId);

                    if (uploadedImageNames != null && uploadedImageNames.Count > 0)
                    {
                        removeUploadedImages(uploadedImageNames);
                    }

                    errorModel.AddErrorMessage("Error occurred - Property was not added");
                    log.Error("Could not advertise property", ex);
                }
            }
        }
        /// <summary>
        /// valiidates the advertisment request
        /// </summary>
        /// <param name="model"></param>
        private static void validateAdRequest(AdvertisePropertyViewModel model, ErrorModel errorModel)
        {
            if (model.Password != null && !model.Password.Equals(model.ConfirmPassword))
            {
                String errMsg = "The fields Password and Confirm Password are not equal";
                errorModel.AddErrorMessage(errMsg);
                throw new Exception(errMsg);
            }
        }

        /// <summary>
        /// Sends the email that informs the user that their
        /// account was successfully created
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isOwner"></param>
        /// <returns></returns>
        public static bool SendUserCreatedEmail(User user, bool isOwner)
        {
            string subject = "JProps - Thank you for signing up";
            string body = "<p>Your account was successfully created on <b>JProps</b></p>";

            if (isOwner)
                body += "<p>You can now upload properties, request properties, ask questions about a " +
                    "property and save properties to review later.</p>";
            else
                body += "<p>You can now request properties, ask questions about a property and save properties to review later.</p>";

            MailHelper mail = new MailHelper(user.Email, subject, body, user.FirstName);

            if (mail.SendMail())
                return true;

            return false;
        }

        /// <summary>
        /// Inserts the property along with it's owner, subscription period and also the property images
        /// </summary>
        /// <param name="model"></param>
        private static Guid insertProperty(AdvertisePropertyViewModel model, UnitOfWork unitOfWork, User user)
        {
            bool doesOwnerExist = user != null && user.Owner.Select(x => x.ID).Count() > 0 ? true : false;
            Guid ownerID = doesOwnerExist ? user.Owner.Select(x => x.ID).Single() : Guid.NewGuid();
            Subscription currSubscription = null;

            if (doesOwnerExist)
                currSubscription = unitOfWork.Subscription.GetSubscriptionByOwnerID(ownerID);

            Guid propertyID = Guid.NewGuid();
            String lat = String.Empty;
            String lng = String.Empty;

            //generate enrolment key for users with Landlord subscription
            if (currSubscription == null
                && !String.IsNullOrEmpty(model.SubscriptionType)
                && model.SubscriptionType.Equals(nameof(EFPConstants.PropertySubscriptionType.Landlord)))
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
                Availability = false,
                AvailabilityModifiedBy = EFPConstants.Audit.System,
                EnrolmentKey = model.EnrolmentKey,
                TermsAgreement = model.TermsAgreement,
                TotAvailableBathroom = model.TotAvailableBathroom,
                TotRooms = model.TotRooms,
                Area = model.Area,
                IsReviewable = model.IsReviewable,
                DateTCreated = DateTime.Now
            };

            if (currSubscription != null
                && currSubscription.SubscriptionType.ID.Equals(PropertySubscriptionType.Basic)
                && currSubscription.ExpiryDate.HasValue
                && currSubscription.ExpiryDate.Value > DateTime.Now)
            {
                property.Availability = true;
            }

            if (currSubscription == null && !String.IsNullOrEmpty(model.SubscriptionType))
            {
                if (mapPropertySubscriptionTypeToCode(model.SubscriptionType)
                               .Equals(PropertySubscriptionType.Basic))
                {
                    property.Availability = true;
                }

                Subscription subscription = new Subscription()
                {
                    ID = Guid.NewGuid(),
                    OwnerID = ownerID,
                    TypeCode = mapPropertySubscriptionTypeToCode(model.SubscriptionType),
                    Period = model.SubscriptionPeriod,
                    IsActive = false,
                    DateTCreated = DateTime.Now
                };

                //start subscription immediately if subscription type is BASIC
                if (subscription.TypeCode.Equals(PropertySubscriptionType.Basic))
                {
                    subscription.StartDate = DateTime.Now;
                    subscription.ExpiryDate = DateTime.Now.AddMonths(subscription.Period).AddDays(-1);
                    subscription.IsActive = true;
                    subscription.DateTModified = DateTime.Now;
                    subscription.ModifiedBy = Audit.System;
                }

                unitOfWork.Subscription.Add(subscription);
            }

            if (!doesOwnerExist)
            {
                /*
                Guid guid = Guid.NewGuid();
                String fileName = String.Empty;
                
                if (model.organizationLogo != null)
                {
                    fileName = guid.ToString() + Path.GetExtension(model.organizationLogo.FileName);
                    UploadPropertyImages(model.organizationLogo, fileName);
                }*/

                Owner owner = new Owner()
                {
                    ID = ownerID,
                    UserID = user.ID,
                    Organization = null,//model.Organization,
                    LogoUrl = null,//fileName,
                    DateTCreated = DateTime.Now
                };

                unitOfWork.Owner.Add(owner);
            }

            unitOfWork.Property.Add(property);

            associateTagsWithProperty(unitOfWork, propertyID, model.selectedTags);
            PropertyHelper.AssociateImagesWithProperty(unitOfWork, model.flPropertyPics, propertyID);

            return property.ID;
        }

        /// <summary>
        /// Associates a property with the selected tags
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="propertyID"></param>
        /// <param name="selectedTags"></param>
        private static void associateTagsWithProperty(UnitOfWork unitOfWork, Guid propertyID, string[] selectedTags)
        {
            List<String> distinctTags = new List<String>();

            if (selectedTags != null)
            {
                foreach (var tagName in selectedTags)
                {
                    if (!distinctTags.Contains(tagName))//added to prevent the same tags from being associated with one property || BUG unkown
                    {
                        distinctTags.Add(tagName);

                        Tags tag = new Tags
                        {
                            ID = Guid.NewGuid(),
                            PropertyID = propertyID,
                            TypeID = unitOfWork.TagType.GetTagTypeIDByTagName(tagName),
                            DateTCreated = DateTime.Now
                        };

                        unitOfWork.Tags.Add(tag);
                    }
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
                    imageInfo.Add("availability", unitOfWork.Property.IsPropertyAvailable(image.PropertyID).ToString());

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
                    log.Error("Cannot save liked property", ex);
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
                    log.Error("Profile update unsuccessful");
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

                try
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

                    if (sendPaymentReviewEmail(user) && sendPaymentMadeEmail(adminUser, user, payment))
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
                    log.Error("Payment unsuccessful", ex);
                    return false; //indicating faliure
                }
            }
        }

        /// <summary>
        /// sends email to the admin regarding new payments that are made
        /// </summary>
        /// <param name="adminUser"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private static bool sendPaymentMadeEmail(User adminUser, User user, Payment payment)
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
        private static bool sendPaymentReviewEmail(User user)
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
        private static bool sendPaymentVerifiedEmail(User user)
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
        public static ErrorModel VerifyPayment(Guid paymentID, Guid userId)
        {
            ErrorModel errorModel = new ErrorModel();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                try
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    var userName = unitOfWork.User.Get(userId).Email;
                    var payment = unitOfWork.Payment.Get(paymentID);
                    var propertyOwnerID = payment.Subscription.Owner.ID;

                    payment.IsVerified = true;
                    payment.DateTModified = DateTime.Now;

                    var subscription = startSubscription(unitOfWork, payment, userName);
                    extendSubscription(unitOfWork, payment, userName);//extending subscription date if necessary
                    makePropertiesAvailableForOwner(unitOfWork, subscription, propertyOwnerID, userName); //make properties available after payment

                    if (sendPaymentVerifiedEmail(subscription.Owner.User))
                    {
                        unitOfWork.save();
                    }
                    else
                    {
                        String errString = "Unable to send verification email";
                        errorModel.AddErrorMessage(errString);
                    }

                    return errorModel;
                }
                catch (Exception ex)
                {
                    errorModel.AddErrorMessage(ex.Message);
                    log.Error("Payment verification error", ex);
                    return errorModel;
                }
            }
        }
        /// <summary>
        /// Allows owner's properties to be now available
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="propertyOwnerID"></param>
        private static void makePropertiesAvailableForOwner(UnitOfWork unitOfWork, Subscription subscription, Guid propertyOwnerID, String userName)
        {
            RequestModel requestModel = new RequestModel();
            String errMsg = String.Empty;
            var properties = unitOfWork.Property.GetPropertiesByOwnerId(propertyOwnerID);

            if (subscription == null)
                subscription = unitOfWork.Subscription.GetActiveSubscriptionByOwnerID(propertyOwnerID);

            if (subscription == null || (subscription != null && !subscription.IsActive))
            {
                errMsg = "No active subscription found for property owner";
                requestModel.AddErrorMessage(errMsg);
                throw new Exception(errMsg);
            }

            if (subscription.ExpiryDate.HasValue && subscription.ExpiryDate.Value > DateTime.Now)
            {
                foreach (var property in properties)
                {
                    //Not modifying properties that the user already set to not available
                    if ((property.AvailabilityModifiedBy != null && property.AvailabilityModifiedBy.Equals(EFPConstants.Audit.System))
                        || String.IsNullOrEmpty(property.AvailabilityModifiedBy))
                    {
                        property.Availability = true;
                        property.AvailabilityModifiedBy = EFPConstants.Audit.System;
                    }
                }
            }
            else
            {
                errMsg = "Properties cannot be available if user subscription is expired";
                requestModel.AddErrorMessage(errMsg);
                throw new Exception(errMsg);
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
        /// Starts subscription if it has not been started as yet upon
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="payment"></param>
        /// <param name="userName"></param>
        private static Subscription startSubscription(UnitOfWork unitOfWork, Payment payment, string userName)
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
                    log.Error("Error occurred while removing saved property", ex);
                    return false;
                }
            }
        }
        /// <summary>
        /// Toggles the availability of a property
        /// </summary>
        /// <param name="propertyID"></param>
        /// <returns></returns>
        public static ErrorModel TogglePropertyAvailability(Guid propertyID)
        {
            ErrorModel errorModel = new ErrorModel();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                try
                {
                    var property = unitOfWork.Property.Get(propertyID);

                    if (!property.Availability)
                    {
                        var subscription = unitOfWork.Subscription.GetActiveSubscriptionByOwnerID(property.OwnerID);

                        if (subscription == null
                            || (subscription != null && !subscription.StartDate.HasValue))
                        {
                            var errMsg = "No active subscription found for property owner";
                            errorModel.AddErrorMessage(errMsg);
                            throw new Exception(errMsg);
                        }

                        //don't allow property availability if subscription expiry date is past
                        if (DateTime.Now <= subscription.ExpiryDate.Value)
                        {
                            var username = unitOfWork.Property.GetPropertyOwnerByPropID(propertyID).User.Email;
                            property.Availability = true;
                            property.AvailabilityModifiedBy = username;
                        }
                        else
                        {
                            var errMsg = "Your subscription expired on " + subscription.ExpiryDate.Value.ToShortDateString();
                            errMsg += "</br> <b>Extend your subscription to make your property available </b>";
                            errorModel.AddErrorMessage(errMsg);
                        }
                    }
                    else
                        property.Availability = false;

                    unitOfWork.save();

                    return errorModel;
                }
                catch (Exception ex)
                {
                    log.Error("Unable to toggle property availability ", ex);
                    return errorModel;
                }
            }
        }

        public static bool RemoveProperty(Guid propertyID)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                try
                {
                    var property = unitOfWork.Property.Get(propertyID);
                    unitOfWork.Property.Remove(property);

                    unitOfWork.save();

                    return true;
                }
                catch (Exception ex)
                {
                    log.Error("Error occurred while removing property", ex);
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
                using (var txscope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    try
                    {
                        UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                        var userExist = unitOfWork.User.DoesUserExist(email);

                        if (!userExist)
                        {
                            string errMessage = "The email address was not recognised";
                            errorModel.AddErrorMessage(errMessage);

                            return false;
                        }

                        var fName = unitOfWork.User.GetUserByEmail(email).FirstName;
                        var userId = unitOfWork.User.GetUserByEmail(email).ID;
                        var uniqueKey = getRandomKey(5);

                        savePasswordRecoveryRequest(unitOfWork, userId, uniqueKey);
                        var emailSent = sendRecoverPasswordEmail(email, userId, fName, uniqueKey, errorModel);

                        txscope.Complete();

                        return emailSent;
                    }
                    catch (Exception ex)
                    {
                        string errMsg = "An error occurred while recovering password";
                        errorModel.AddErrorMessage(errMsg);
                        log.Error(errMsg, ex);

                        return false;
                    }
                }
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
        private static bool sendRecoverPasswordEmail(string email, Guid userId, string fName, string accessCode, ErrorModel errorModel)
        {
            try
            {
                string emailTo = email;
                string subject = "JProps - Password Recovery";
                string body = "Your access code to reset your password is <b>" + accessCode + "</b> ";
                body += ". Please click the link below or copy and paste it in a new browser window to reset your password: <br/><br/>";
                body += "http://www." + EFPConstants.Application.Host + "/accounts/resetpassword?userId=" + userId.ToString();
                body += "<br/><br/><small>Your access code will expire 10 minutes after recieving this mail</small>";

                MailHelper mail = new MailHelper(emailTo, subject, body, fName);

                return mail.SendMail();
            }
            catch (Exception ex)
            {
                string errMessage = "An unexpected error occurred while sending the recovery password <br /> Please try again later";
                errorModel.AddErrorMessage(errMessage);
                log.Error(errMessage);

                throw new Exception(errMessage, ex);
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
                    string errMessage = "Unable to reset your password : " + e.Message;
                    errorModel.AddErrorMessage(errMessage);
                    log.Error(errMessage, e);
                    return false;
                }
                catch (Exception e)
                {
                    string errMessage = "An unexpected error occurred - please contact system administrator or try again later";
                    errorModel.AddErrorMessage(errMessage);
                    log.Error(errMessage, e);
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
        /// <summary>
        /// Changes the user's subscription 
        /// Compare subsciption prices to determine if a payment will be required 
        /// for subscription change i.e. if it is an upgrade
        /// </summary>
        /// <param name="subscriptionID"></param>
        /// <param name="subscriptionType"></param>
        /// <returns></returns>
        public static RequestModel ChangeSubscription(Guid subscriptionID, String subscriptionTypeName, int? period)
        {
            var requestModel = new RequestModel();
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                var userName = String.Empty;
                try
                {
                    var subscription = unitOfWork.Subscription.Get(subscriptionID);
                    userName = subscription.Owner.User.Email;
                    subscription.IsActive = false;

                    var newSubscriptionType = unitOfWork.SubscriptionType
                    .GetSubscriptionTypeByID(mapPropertySubscriptionTypeToCode(subscriptionTypeName));

                    Subscription newSubscription = new Subscription()
                    {
                        ID = Guid.NewGuid(),
                        OwnerID = subscription.OwnerID,
                        TypeCode = mapPropertySubscriptionTypeToCode(subscriptionTypeName),
                        DateTCreated = DateTime.Now
                    };

                    //ensure that users who have not subscribed can change their subscription

                    if (newSubscriptionType.MonthlyCost < subscription.SubscriptionType.MonthlyCost)
                    {
                        var msg = "Subscription was changed successfully. You are now on the <b>" + newSubscriptionType.Name + "</b> subscription";

                        newSubscription.Period = DateDiff(Intervals.Months, DateTime.Now, subscription.ExpiryDate.Value);
                        newSubscription.StartDate = DateTime.Now;
                        newSubscription.ExpiryDate = subscription.ExpiryDate.Value;
                        newSubscription.IsActive = true;

                        requestModel.AddMessage(msg);

                        makePropertiesAvailableForOwner(unitOfWork, newSubscription, Guid.Empty, userName);
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
        }

        public static int DateDiff(Intervals eInterval, DateTime dtInit, DateTime dtEnd)
        {
            if (dtEnd < dtInit)
                throw new ArithmeticException("Init date should be previous to End date.");

            switch (eInterval)
            {
                //case Intervals.Days:
                //     return (dtEnd - dtInit).TotalDays;
                case Intervals.Months:
                    return ((dtEnd.Year - dtInit.Year) * 12) + dtEnd.Month - dtInit.Month;
                case Intervals.Years:
                    return dtEnd.Year - dtInit.Year;
                default:
                    throw new ArgumentException("Incorrect interval code.");
            }
        }
        /// <summary>
        /// Gets the subscription type of a user
        /// </summary>
        /// <param name="subscriptionID"></param>
        /// <returns></returns>
        public static SubscriptionType GetSubscriptionTypeByUserSubId(Guid subscriptionID)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);
                var subscription = unitOfWork.Subscription.Get(subscriptionID);

                return subscription.SubscriptionType;
            }
        }
        /// <summary>
        /// Renews the basic subscription for a user
        /// </summary>
        /// <param name="subscriptionID"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static RequestModel RenewSubscription(Guid subscriptionID, Guid userId)
        {
            var requestModel = new RequestModel();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                var msg = String.Empty;
                try
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    var userName = unitOfWork.User.Get(userId).Email;
                    var subscription = unitOfWork.Subscription.Get(subscriptionID);
                    var propertyOwnerID = subscription.OwnerID;

                    subscription.ExpiryDate = DateTime.Now.AddMonths(1).AddDays(-1);
                    subscription.Period = 1;
                    subscription.DateTModified = DateTime.Now;
                    subscription.ModifiedBy = userName;

                    makePropertiesAvailableForOwner(unitOfWork, subscription, propertyOwnerID, userName);
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
        }

        /// <summary>
        /// checks for an active subscription that is associated with 
        /// that email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static RequestModel SubscriptionCheck(string email)
        {
            RequestModel requestModel = new RequestModel();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                try
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

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
        }
        /// <summary>
        /// validates the number of properties that can be published 
        /// based on the subscription type
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsAdAccessValid(Guid userId)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                try
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

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
        }

        /// <summary>
        /// returns the ad access error message to the user
        /// </summary>
        /// <returns></returns>
        public static String GetAdAccessErrMessage()
        {
            return adAccessErrMessage;
        }
        /// <summary>
        /// Cancels a user's subscription and removes the associated properties
        /// </summary>
        /// <param name="subscriptionID"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static RequestModel CancelSubscription(Guid subscriptionID, Guid userId)
        {
            var requestModel = new RequestModel();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                var msg = String.Empty;
                try
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    var userName = unitOfWork.User.Get(userId).Email;
                    var subscription = unitOfWork.Subscription.Get(subscriptionID);
                    var propertyOwnerID = subscription.OwnerID;

                    subscription.ExpiryDate = DateTime.Now;
                    subscription.Period = 0;
                    subscription.IsActive = false;
                    subscription.DateTModified = DateTime.Now;
                    subscription.ModifiedBy = userName;

                    removePropertiesForOwner(unitOfWork, propertyOwnerID);
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
        }

        /// <summary>
        /// Remove properties belonging to an owner
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="ownerId"></param>
        private static void removePropertiesForOwner(UnitOfWork unitOfWork, Guid ownerId)
        {
            String errMsg = String.Empty;
            var properties = unitOfWork.Property.GetPropertiesByOwnerId(ownerId);

            foreach (var property in properties)
            {
                unitOfWork.Property.Remove(property);
            }
        }

        /// <summary>
        /// returns the requisition history for any user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static IEnumerable<RequisitionViewModel> GetRequisitionHistory(Guid userId)
        {
            return GetRequisitions(userId, true);
        }

        /// <summary>
        /// Gets the total unseen messages
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static int GetUnseenMsgsCount(Guid userId)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                try
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                    return unitOfWork.Message.GetTotUnseenForUser(userId);
                }
                catch (Exception ex)
                {
                    log.Error("Error occurred while retrieving the total unseen messages for user + " + userId, ex);
                    return 0;
                }
            }
        }

        public static int GetUnseenReqsCount(Guid userId)
        {
            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                try
                {
                    UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

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
}

