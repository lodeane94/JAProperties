using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SS.Models.Repositories;

namespace SS.Models
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EasyFindPropertiesEntities _dbCtx;
        public IPropertyTypeRepository PropertyType { get; private set; }
        public IAdPriorityRepository AdPriority { get; private set; }
        public IAdTypeRepository AdType { get; private set; }
        public IPropertyCategoryRepository PropertyCategory { get; private set; }
        public IPropertyConditionRepository PropertyCondition { get; private set; }
        public IPropertyImageRepository PropertyImage { get; private set; }
        public IPropertyPurposeRepository PropertyPurpose { get; private set; }
        public IPropertyRatingRepository PropertyRating { get; private set; }
        public IPropertyRepository Property { get; private set; }
        public IPropertyRequisitionRepository PropertyRequisition { get; private set; }
        public ISubscriptionTypeRepository SubscriptionType { get; private set; }
        public ISubscriptionRepository Subscription { get; private set; }
        public ITagTypeRepository TagType { get; private set; }
        public ITennantRepository Tennant { get; private set; }
        public IOwnerRepository Owner { get; private set; }
        public ITagsRepository Tags { get; private set; }
        public IMessageRepository Message { get; private set; }

        public UnitOfWork(EasyFindPropertiesEntities dbCtx)
        {
            _dbCtx = dbCtx;
            PropertyType = new PropertyTypeRepository(_dbCtx);
            AdPriority = new AdPriorityRepository(_dbCtx);
            AdType = new AdTypeRepository(_dbCtx);
            PropertyCategory = new PropertyCategoryRepository(_dbCtx);
            PropertyCondition = new PropertyConditionRepository(_dbCtx);
            PropertyImage = new PropertyImageRepository(_dbCtx);
            PropertyPurpose = new PropertyPurposeRepository(_dbCtx);
            PropertyRating = new PropertyRatingRepository(_dbCtx);
            Property = new PropertyRepository(_dbCtx);
            PropertyRequisition = new PropertyRequisitionRepository(_dbCtx);
            SubscriptionType = new SubscriptionTypeRepository(_dbCtx);
            Subscription = new SubscriptionRepository(_dbCtx);
            TagType = new TagTypeRepository(_dbCtx);
            Tennant = new TennantRepository(_dbCtx);
            Owner = new OwnerRepository(_dbCtx);
            Tags = new TagsRepository(_dbCtx);
            Message = new MessageRepository(_dbCtx);
        }

        public int save()
        {
            return _dbCtx.SaveChanges();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _dbCtx.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~UnitOfWork() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}