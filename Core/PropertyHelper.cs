using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

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

        public static String mapPropertyPurposeNameToCode(String propertyPurposeName)
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

        public static String mapPropertyAdTypeNameToCode(String propertyAdTypeName)
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

        public static String mapPropertyAdpriorityNameToCode(String adpriorityName)
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

        public static String mapPropertySubscriptionTypeToCode(String subscriptionTypeName)
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
    }
}
 
