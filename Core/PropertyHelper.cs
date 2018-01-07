using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using SS.Models;
using SS.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
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

        public static List<FeaturedPropertiesSlideViewModel> PopulatePropertiesViewModel(PropertySearchViewModel model, String calledBy)
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


        public static List<FeaturedPropertiesSlideViewModel> PopulatePropertiesViewModel(int take, String calledBy)
        {
            int slideTake = 5;
            int slideTakeCount = 0;//used to determine whether to get retrieve multiple images or one
            
            List<FeaturedPropertiesSlideViewModel> featuredPropertiesSlideViewModelList = new List<FeaturedPropertiesSlideViewModel>();

            using (EasyFindPropertiesEntities dbCtx = new EasyFindPropertiesEntities())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbCtx);

                foreach (var property in unitOfWork.Property.GetFeaturedProperties(take))
                {
                    slideTakeCount++;

                    IEnumerable<int> avgPropRatings = unitOfWork.PropertyRating.GetPropertyRatingsCountByPropertyId(property.ID);                    

                    featuredPropertiesSlideViewModelList.Add(new FeaturedPropertiesSlideViewModel()
                    {
                        property = property,
                        propertyImageURLs = (calledBy.Equals("Home") && slideTakeCount <= slideTake) ? unitOfWork.PropertyImage.GetImageURLsByPropertyId(property.ID, slideTake) : null,
                        propertyPrimaryImageURL = unitOfWork.PropertyImage.GetPrimaryImageURLByPropertyId(property.ID),
                        averageRating = avgPropRatings.Count() > 0 ? (int)avgPropRatings.Average() : 0
                    });
                }
            };

            return featuredPropertiesSlideViewModelList;
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
    }
}

