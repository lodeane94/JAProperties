﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public class PropertyImageRepository : Repository<PropertyImage>, IPropertyImageRepository
    {
        public EasyFindPropertiesEntities EasyFindPropertiesEntities
        {
            get { return (EasyFindPropertiesEntities)dbCtx; }
        }

        public PropertyImageRepository(EasyFindPropertiesEntities dbctx)
            : base(dbctx)
        { }

        public IEnumerable<string> GetImageURLsByPropertyId(Guid Id,int take = 0)
        {
            if (!take.Equals(0))
            {
                return EasyFindPropertiesEntities.PropertyImage
                    .Where(x => x.PropertyID.Equals(Id))
                    .Select(x => x.ImageURL).Take(take).ToList();
            }
            else
            {
                return EasyFindPropertiesEntities.PropertyImage.Where(x => x.PropertyID.Equals(Id)).Select(x => x.ImageURL).ToList();
            }
        }

        public string GetPrimaryImageURLByPropertyId(Guid Id)
        {
            return EasyFindPropertiesEntities.PropertyImage.Where(x => x.PropertyID.Equals(Id) && x.IsPrimaryDisplay.Equals(true)).Select(x => x.ImageURL).Single();
        }

        public PropertyImage GetPrimaryImageByPropertyId(Guid Id)
        {
            return EasyFindPropertiesEntities.PropertyImage.Where(x => x.PropertyID.Equals(Id) && x.IsPrimaryDisplay.Equals(true)).Single();
        }

        public string GetImageURLById(Guid Id)
        {
            return EasyFindPropertiesEntities.PropertyImage.Find(Id).ImageURL;
        }

        public IEnumerable<PropertyImage> GetAllPrimaryPropertyImageByOwnerId(Guid Id)
        {
            return EasyFindPropertiesEntities.PropertyImage.Where(x => x.Property.OwnerID.Equals(Id) && x.IsPrimaryDisplay.Equals(true));
        }

        public IEnumerable<PropertyImage> GetAllImagesByPropertyId(Guid Id, int take)
        {
            if (!take.Equals(0))
            {
                return EasyFindPropertiesEntities.PropertyImage
                    .Where(x => x.PropertyID.Equals(Id))
                    .OrderByDescending(x => x.IsPrimaryDisplay)
                    .ThenByDescending(x => x.DateTCreated)
                    .Take(take)
                    .ToList();
            }
            else
            {
                return EasyFindPropertiesEntities.PropertyImage
                    .Where(x => x.PropertyID.Equals(Id))
                    .OrderByDescending(x => x.IsPrimaryDisplay)
                    .ThenByDescending(x => x.DateTCreated)
                    .ToList();
            }
        }
    }
}