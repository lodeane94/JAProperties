using log4net;
using SS.Core;
using SS.Models;
using SS.SignalR;
using SS.ViewModels;
using SS.ViewModels.Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using static SS.Core.EFPConstants;

namespace SS.Services
{
    public class PropertyService
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<String> searchResultPropertyTags;
        private readonly UserService userService;

        public PropertyService()
        {
            searchResultPropertyTags = null;
            userService = new UserService();
        }

        /// <summary>
        /// Populates the SelectedPropertyViewModel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SelectedPropertyViewModel GetProperty(Guid id, UnitOfWork unitOfWork)
        {
            var property = unitOfWork.Property.Get(id);
            var user = property.Owner.User;

            SelectedPropertyViewModel ViewModel = new SelectedPropertyViewModel
            {
                ID = property.ID.ToString(),
                StreetNumber = property.StreetNumber,
                StreetAddress = property.StreetAddress,
                Community = property.Community,
                Division = property.Division,
                Country = property.Country,
                PropertyType = property.PropertyType.Name,
                AdType = property.AdType.Name,
                TotalBedrooms = property.TotRooms.HasValue ? property.TotRooms.Value : 0,
                TotalBathrooms = property.TotAvailableBathroom.HasValue ? property.TotAvailableBathroom.Value : 0,
                PropertyCategoryCode = property.CategoryCode,
                PropertyCondition = property.PropertyCondition.Name,
                Occupancy = property.Occupancy.ToString(),
                Price = property.Price,
                SecurityDeposit = property.SecurityDeposit.HasValue ? property.SecurityDeposit.Value : 0,
                Area = property.Area.HasValue ? property.Area.Value : 0,
                Description = property.Description,
                OwnerFirstName = user.FirstName,
                OwnerLastName = user.LastName,
                OwnerCellNumber = user.CellNum,
                PropertyPrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(property.ID),
                PropertyImageURLs = unitOfWork.PropertyImage.GetImageURLsByPropertyId(id, 0),
                PropRatings = unitOfWork.PropertyRating.GetPropertyRatingsByPropertyId(id)
            };
            ViewModel.PropertyAverageRatings = ViewModel.PropRatings.Count() > 0 ? (int)ViewModel.PropRatings.Select(x => x.Ratings).Average() : 0;
            ViewModel.Tags = unitOfWork.Tags.GetTagNamesByPropertyId(id);
            ViewModel.isAvailable = property.Availability;
            ViewModel.DateAddedModified = property.DateTModified.HasValue ? property.DateTModified.Value.ToShortDateString() : property.DateTCreated.ToShortDateString();

            return ViewModel;
        }

        public async Task<HomePageViewModel> PopulateHomePageViewModel(int take, Guid userId, UnitOfWork unitOfWork)
        {
            log.Info("Populating home page");

            HomePageViewModel ViewModel = new HomePageViewModel();
            FeaturedProperty Temporary = new FeaturedProperty();
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

            return ViewModel;
        }

        /// <summary>
        /// Retrieve all properties based on the criteria 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<FeaturedPropertiesSlideViewModel>> PopulatePropertiesViewModel(PropertySearchViewModel model, Guid userId, UnitOfWork unitOfWork)
        {
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = new List<FeaturedPropertiesSlideViewModel>();

            IEnumerable<Property> filteredProperties = null;
            IEnumerable<Property> searchTermProperties = null;
            IEnumerable<Property> properties = null;

            List<Filter> filters = PropertyHelper.CreateFilterList(model, unitOfWork);
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

                var propertyModel = new FeaturedPropertiesSlideViewModel
                {
                    ID = property.ID.ToString(),
                    StreetNumber = property.StreetNumber,
                    StreetAddress = property.StreetAddress,
                    Division = property.Division,
                    Country = property.Country,
                    PropertyType = property.PropertyType.Name,
                    Price = property.Price,
                    Description = property.Description,
                    TotalBedrooms = property.TotRooms.HasValue ? property.TotRooms.Value : 0,
                    TotalBathrooms = property.TotAvailableBathroom.HasValue ? property.TotAvailableBathroom.Value : 0,
                    User = property.Owner.User,
                    PropertyPrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(property.ID),
                    PropertyAverageRatings = avgPropRatings.Count() > 0 ? (int)avgPropRatings.Average() : 0,
                    IsPropertySaved = !userId.Equals(Guid.Empty) ? unitOfWork.SavedProperties.IsPropertySavedForUser(userId, property.ID) : false,
                    AdType = property.AdType.Name,
                    DateAddedModified = property.DateTModified.HasValue ? property.DateTModified.Value.ToShortDateString() : property.DateTCreated.ToShortDateString()
                };

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

            return featuredPropertiesSlideViewModelList;
        }
        /// <summary>
        /// Retrieve all properties based on the criteria for the nearBY properties
        /// </summary>
        /// <param name="revisedModel"></param>
        /// <param name="svModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<FeaturedPropertiesSlideViewModel>> PopulatePropertiesViewModel(List<NearbyPropertySearchModel> revisedModel, PropertySearchViewModel svModel, Guid userId, UnitOfWork unitOfWork)
        {
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = new List<FeaturedPropertiesSlideViewModel>();
            IEnumerable<Property> properties = null;

            var saProperties = await unitOfWork.Property.FindPropertiesByStreetAddress(revisedModel, svModel.take, svModel.PgNo);
            properties = getTaggedFilterPropsIfApplicable(saProperties, svModel, unitOfWork);

            //TODO optimize by removing extra calls to the database
            //this could be done via a single query
            foreach (var property in properties)
            {
                IEnumerable<int> avgPropRatings = unitOfWork.PropertyRating.GetPropertyRatingsCountByPropertyId(property.ID);

                var model = new FeaturedPropertiesSlideViewModel
                {
                    ID = property.ID.ToString(),
                    StreetNumber = property.StreetNumber,
                    StreetAddress = property.StreetAddress,
                    Division = property.Division,
                    Country = property.Country,
                    PropertyType = property.PropertyType.Name,
                    Price = property.Price,
                    Description = property.Description,
                    TotalBedrooms = property.TotRooms.HasValue ? property.TotRooms.Value : 0,
                    TotalBathrooms = property.TotAvailableBathroom.HasValue ? property.TotAvailableBathroom.Value : 0,
                    User = property.Owner.User,
                    PropertyPrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(property.ID),
                    PropertyAverageRatings = avgPropRatings.Count() > 0 ? (int)avgPropRatings.Average() : 0,
                    IsPropertySaved = !userId.Equals(Guid.Empty) ? unitOfWork.SavedProperties.IsPropertySavedForUser(userId, property.ID) : false,
                    AdType = property.AdType.Name,
                    DateTCreated = property.DateTCreated,
                    DateAddedModified = property.DateTModified.HasValue ? property.DateTModified.Value.ToShortDateString() : property.DateTCreated.ToShortDateString()
                };

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

            //TODO add sort button on page
            return featuredPropertiesSlideViewModelList
                    .OrderBy(x => double.Parse(x.DistanceFromSearchedAddress))
                    .ThenBy(x => x.Price)
                    .ThenByDescending(x => x.DateTCreated).ToList();
        }

        /// <summary>
        /// Retrieves the coordinates of properties based on conditions
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Array> PopulateModelForPropertyCoordinates(PropertySearchViewModel model, UnitOfWork unitOfWork)
        {
            Array propertyCoordinates = null;

            List<Filter> filters = PropertyHelper.CreateFilterList(model, unitOfWork);
            var deleg = ExpressionBuilder.GetExpression<Property>(filters);

            propertyCoordinates = await unitOfWork.Property.FindPropertiesCoordinates(deleg);

            return propertyCoordinates;
        }

        /// <summary>
        /// narrows the search result to those properties that are
        /// within the distance radius
        /// </summary>
        /// <param name="model"></param>
        /// <param name="distanceRadius"></param>
        /// <returns></returns>
        public List<NearbyPropertySearchModel> NarrowSearchResultsToDistanceRadius(NearbyPropertySearchViewModel model, double distanceRadius)
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

        /// <summary>
        /// retrieves the total properties found for a normal search
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<int> GetPropertiesCount(PropertySearchViewModel model, UnitOfWork unitOfWork)
        {
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = new List<FeaturedPropertiesSlideViewModel>();

            IEnumerable<Property> filteredProperties = null;
            IEnumerable<Property> searchTermProperties = null;
            IEnumerable<Property> properties = null;

            List<Filter> filters = PropertyHelper.CreateFilterList(model, unitOfWork);
            var deleg = ExpressionBuilder.GetExpression<Property>(filters);

            filteredProperties = await unitOfWork.Property.FindProperties(deleg, 0, 0);
            searchTermProperties = await unitOfWork.Property.FindPropertiesBySearchTerm(model.SearchTerm, model.PropertyCategory, 0, 0);
            var combinedProperties = filteredProperties.Concat(searchTermProperties).Distinct();

            properties = getTaggedFilterPropsIfApplicable(combinedProperties, model, unitOfWork);


            return properties.Count();
        }

        /// <summary>
        /// retrieves the total properties found for a near by search
        /// </summary>
        /// <param name="revisedModel"></param>
        /// <param name="svModel"></param>
        /// <returns></returns>
        public async Task<int> GetNearbyPropertiesCount(List<NearbyPropertySearchModel> revisedModel, PropertySearchViewModel svModel, UnitOfWork unitOfWork)
        {
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = new List<FeaturedPropertiesSlideViewModel>();
            IEnumerable<Property> properties = null;

            var saProperties = await unitOfWork.Property.FindPropertiesByStreetAddress(revisedModel, 0, 0);
            properties = getTaggedFilterPropsIfApplicable(saProperties, svModel, unitOfWork);

            return properties.Count();
        }

        /// <summary>
        /// Futher filters a list of properties based on
        /// their tags
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="model"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        private IEnumerable<Property> getTaggedFilterPropsIfApplicable(IEnumerable<Property> properties, PropertySearchViewModel model, UnitOfWork unitOfWork)
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

        /// <summary>
        /// returns the property tags that were generated from the properties 
        /// search results
        /// </summary>
        /// <returns></returns>
        public List<String> GetSearchResultPropertyTags()
        {
            return searchResultPropertyTags;
        }

        /// <summary>
        /// Checks if a property is furnished
        /// </summary>
        /// <param name="property"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        private bool isPropertyFurnished(Property property, UnitOfWork unitOfWork)
        {
            string furnishedTagVal = unitOfWork.Tags.GetTagNamesByPropertyId(property.ID)
                    .Where(x => x.ToString().Contains("Furnished")).SingleOrDefault();

            return furnishedTagVal != null ? true : false;
        }

        /// <summary>
        /// Allows viewers to request the property or ask a question about the property
        /// </summary>
        /// <param name="request"></param>
        /// <param name="contactPurpose"></param>
        public void ContactPropertyOwner(PropertyRequisition request, string contactPurpose, Guid userId, ErrorModel errorModel, UnitOfWork unitOfWork)
        {
            String errMsg = String.Empty;
            MessageService messageService = new MessageService();

            try
            {
                if (contactPurpose.Equals("requisition"))
                    requestProperty(request, userId, errorModel, unitOfWork);
                else
                    messageService.SendAnonymousMessage(request, userId, errorModel, unitOfWork);
            }
            catch (Exception ex)
            {
                errMsg = "Unable to send request confirmation email - Please contact the system administrator";
                errorModel.AddErrorMessage(errMsg);

                log.Error(errMsg, ex);
            }
        }

        /// <summary>
        /// Requests a property from a property owner
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userId"></param>
        /// <param name="errorModel"></param>
        /// <param name="unitOfWork"></param>
        private void requestProperty(PropertyRequisition request, Guid userId, ErrorModel errorModel, UnitOfWork unitOfWork)
        {
            using (var txscope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                MailingService mailingService = new MailingService();
                String errMsg = String.Empty;
                User ownerUser = unitOfWork.Property.GetPropertyOwnerByPropID(request.PropertyID).User;
                User user = unitOfWork.User.Get(userId);

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
                    mailingService.CreateRequestEmail(user, true), ownerUser.FirstName);

                unitOfWork.save();
                mail.SendMail();

                var userTo = unitOfWork.Property.GetPropertyOwnerByPropID(request.PropertyID).User;
                DashboardHub.alertRequisition(userTo.Email);

                txscope.Complete();
            }
        }

        /// <summary>
        /// Save liked property for users to review on their dashboards
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="propertyId"></param>
        public bool SaveLikedProperty(Guid userId, Guid propertyId, UnitOfWork unitOfWork)
        {
            SavedProperties savedProperties = new SavedProperties()
            {
                ID = Guid.NewGuid(),
                UserID = userId,
                PropertyID = propertyId,
                DateTCreated = DateTime.Now
            };

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

        /// <summary>
        /// creates user account as well as Advertise property
        /// and uploads the images associated with the property
        /// </summary>
        /// <param name="model"></param>
        /// <param name="errorModel"></param>
        public void AdvertiseProperty(AdvertisePropertyViewModel model, Guid userId, ErrorModel errorModel, UnitOfWork unitOfWork)
        {
            var propertyId = Guid.Empty;
            var isUserCreated = false;
            User user = null;

            try
            {
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
                    bool isUserPropOwner = userService.IsUserOfType(userTypes, EFPConstants.UserType.PropertyOwner);

                    if (!isUserPropOwner)
                        userService.AssociateUserWithUserType(unitOfWork, user.ID, EFPConstants.UserType.PropertyOwner);
                }
                else
                {
                    user = userService.CreateUser(unitOfWork, EFPConstants.UserType.PropertyOwner, model.SubscriptionType, model.Email, model.FirstName,
                    model.LastName, model.CellNum, model.AreaCode, DateTime.MinValue);

                    userService.CreateUserAccount(model.Email, model.Password);

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
                    userService.RemoveUserAccount(unitOfWork, model.Email);

                if (!propertyId.Equals(Guid.Empty))
                    RemoveProperty(propertyId, unitOfWork);

                if (PropertyHelper.uploadedImageNames != null && PropertyHelper.uploadedImageNames.Count > 0)
                {
                    PropertyHelper.RemoveUploadedImages(PropertyHelper.uploadedImageNames);
                }

                errorModel.AddErrorMessage("Error occurred - Property was not added");
                log.Error("Could not advertise property", ex);
            }
        }

        /// <summary>
        /// Sends the email that informs the user that their
        /// account was successfully created
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isOwner"></param>
        /// <returns></returns>
        public bool SendUserCreatedEmail(User user, bool isOwner)
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
        /// valiidates the advertisment request
        /// </summary>
        /// <param name="model"></param>
        private void validateAdRequest(AdvertisePropertyViewModel model, ErrorModel errorModel)
        {
            if (model.Password != null && !model.Password.Equals(model.ConfirmPassword))
            {
                String errMsg = "The fields Password and Confirm Password are not equal";
                errorModel.AddErrorMessage(errMsg);
                throw new Exception(errMsg);
            }
        }

        /// <summary>
        /// Inserts the property along with it's owner, subscription period and also the property images
        /// </summary>
        /// <param name="model"></param>
        private Guid insertProperty(AdvertisePropertyViewModel model, UnitOfWork unitOfWork, User user)
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
                model.EnrolmentKey = PropertyHelper.GetRandomKey(6);
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
                if (PropertyHelper.MapPropertySubscriptionTypeToCode(model.SubscriptionType)
                               .Equals(PropertySubscriptionType.Basic))
                {
                    property.Availability = true;
                }

                Subscription subscription = new Subscription()
                {
                    ID = Guid.NewGuid(),
                    OwnerID = ownerID,
                    TypeCode = PropertyHelper.MapPropertySubscriptionTypeToCode(model.SubscriptionType),
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
            AssociateImagesWithProperty(unitOfWork, model.flPropertyPics, propertyID);

            return property.ID;
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

                PropertyHelper.UploadPropertyImages(file, fileName);
            }
        }

        /// <summary>
        /// associates the image with the property that was uploaded
        /// </summary>
        /// <param name="file"></param>
        public String AssociateImageWithProperty(HttpPostedFileBase file, Guid propertyID, UnitOfWork unitOfWork)
        {
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
                PropertyHelper.UploadPropertyImages(file, fileName);

                unitOfWork.save();

                return fileName;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while adding image for property ID + " + propertyID, ex);
                return null;
            }
        }

        /// <summary>
        /// Associates a property with the selected tags
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="propertyID"></param>
        /// <param name="selectedTags"></param>
        private void associateTagsWithProperty(UnitOfWork unitOfWork, Guid propertyID, string[] selectedTags)
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
        /// Removes a property
        /// </summary>
        /// <param name="propertyID"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public bool RemoveProperty(Guid propertyID, UnitOfWork unitOfWork)
        {
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

        /// <summary>
        /// updates the property table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool UpdateProperty(UpdatePropertyViewModel model, UnitOfWork unitOfWork)
        {
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

        public bool UpdatePropertyPrimaryImg(Guid propertyId, Guid imgId, UnitOfWork unitOfWork)
        {
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

        /// <summary>
        /// Removes the selected property image 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool DeletePropertyImage(Guid ID, UnitOfWork unitOfWork)
        {
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

        /// <summary>
        /// Returns the update property view model that will be used when updating information
        /// on a property
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public UpdatePropertyViewModel GetUpdatePropertyVM(Guid ID, UnitOfWork unitOfWork)
        {
            Property property = null;
            UpdatePropertyViewModel newModel = null;

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

        /// <summary>
        /// Returns the update property view model that will be used when updating information
        /// on a property
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public UpdatePropertyViewModel GetUpdatePropertyVM_PropertyImages(Guid ID, UnitOfWork unitOfWork)
        {
            Property property = null;
            UpdatePropertyViewModel newModel = null;

            property = unitOfWork.Property.Get(ID);

            newModel = new UpdatePropertyViewModel()
            {
                PrimaryImageUrl = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(ID),
                PropertyImages = unitOfWork.PropertyImage.GetAllImagesByPropertyId(ID, 0)
            };

            return newModel;
        }

        /// <summary>
        /// returns all properties owned by the current user that is signed in in json format
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public List<Dictionary<String, String>> GetAllPropertyImages(Guid userId, UnitOfWork unitOfWork)
        {
            IEnumerable<PropertyImage> propertyImages;
            List<Dictionary<String, String>> imageList = new List<Dictionary<string, string>>();

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

        /// <summary>
        /// returns all properties owned by the current user that is signed in in json format
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public List<Dictionary<String, String>> GetAllSavedPropertyImages(Guid userId, UnitOfWork unitOfWork)
        {
            List<Dictionary<String, String>> imageList = new List<Dictionary<string, string>>();

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

        /// <summary>
        /// Allows owner's properties to be now available
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="propertyOwnerID"></param>
        public void MakePropertiesAvailableForOwner(UnitOfWork unitOfWork, Subscription subscription, Guid propertyOwnerID, String userName)
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
        /// Remove properties that have been saved by users
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="propertyID"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public bool RemoveSavedProperty(Guid userId, Guid propertyID, UnitOfWork unitOfWork)
        {
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

        /// <summary>
        /// Toggles the availability of a property
        /// </summary>
        /// <param name="propertyID"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public ErrorModel TogglePropertyAvailability(Guid propertyID, UnitOfWork unitOfWork)
        {
            ErrorModel errorModel = new ErrorModel();

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

        /// <summary>
        /// Remove properties belonging to an owner
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="ownerId"></param>
        public void RemovePropertiesForOwner(UnitOfWork unitOfWork, Guid ownerId)
        {
            String errMsg = String.Empty;
            var properties = unitOfWork.Property.GetPropertiesByOwnerId(ownerId);

            foreach (var property in properties)
            {
                unitOfWork.Property.Remove(property);
            }
        }
    }
}