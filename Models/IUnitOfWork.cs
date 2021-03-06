﻿using SS.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models
{
    public interface IUnitOfWork : IDisposable
    {
        IPropertyTypeRepository PropertyType { get; }
        IAdPriorityRepository AdPriority { get; }
        IAdTypeRepository AdType { get; }
        IPropertyCategoryRepository PropertyCategory { get; } 
        IPropertyConditionRepository PropertyCondition { get; }
        IPropertyImageRepository PropertyImage { get; }
        IPropertyPurposeRepository PropertyPurpose { get; }
        IPropertyRatingRepository PropertyRating { get; }
        IPropertyRepository Property { get; }
        IPropertyRequisitionRepository PropertyRequisition { get; }
        ISubscriptionTypeRepository SubscriptionType { get; }
        ISubscriptionRepository Subscription { get; }
        ITagTypeRepository TagType { get; }
        ITennantRepository Tennant { get; }
        IOwnerRepository Owner { get; }
        ITagsRepository Tags { get; }
        IMessageRepository Message { get; }
        IUserRepository User { get; }
        IUserTypeRepository UserType { get; }
        IUserTypeAssocRepository UserTypeAssoc { get; }
        IMessageTrashRepository MessageTrash { get; }
        IMeetingRepository Meeting { get; }
        IMeetingMembersRepository MeetingMembers { get; }
        ISavedPropertiesRepository SavedProperties { get; }
        IPaymentRepository Payment { get; }
        IPaymentMethodRepository PaymentMethod { get; }
        ISubscriptionExtensionRepository SubscriptionExtension { get; }
        IDivisionRepository Division { get; }
        IPasswordRecoveryRequestRepository PasswordRecoveryRequest { get; }
        int save();
    }
}