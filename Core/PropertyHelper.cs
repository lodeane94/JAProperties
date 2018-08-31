using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using log4net;
using SS.Models;
using SS.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using static SS.Core.EFPConstants;

namespace SS.Core
{
    public static class PropertyHelper
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static List<String> uploadedImageNames = new List<string>();//used to store names of uploaded images. Needed in the case of removing uploaded images during rollback
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

        public static String MapPropertySubscriptionTypeToCode(String subscriptionTypeName)
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
        public static void RemoveUploadedImages(List<String> fileNames)
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

                        RemoveUploadedImages(imageNameList);

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

        /// <summary>
        /// Create a filter list to be used for property searching purposes
        /// </summary>
        /// <param name="model"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public static List<Filter> CreateFilterList(PropertySearchViewModel model, UnitOfWork unitOfWork)
        {
            List<Filter> filters = new List<Filter>();

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

        /// <summary>
        /// used to produce a random key for the enrolment key 
        /// </summary>
        public static string GetRandomKey(int size)
        {
            string input = "abcdefghijklmnopqrstuvwxyz0123456789";

            Random random = new Random();

            var chars = Enumerable.Range(0, size)
                        .Select(x => input[random.Next(0, input.Length)]);

            return new string(chars.ToArray());
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
        /// Replaces an image
        /// </summary>
        /// <param name="fileNameToRemove"></param>
        /// <param name="fileToAdd"></param>
        /// <returns></returns>
        public static String ReplaceUplodedImage(string fileNameToRemove, HttpPostedFileBase fileToAdd)
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
    }
}

