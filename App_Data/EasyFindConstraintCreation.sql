BEGIN TRAN
	USE EasyFindProperties;
	GO

	ALTER TABLE UserTypeAssoc
	ADD CONSTRAINT fk_userTypeAssoc_user_type_code FOREIGN KEY(UserTypeCode) REFERENCES UserType(ID),
	CONSTRAINT fk_userTypeAssoc_user_id FOREIGN KEY(UserID) REFERENCES "User"(ID);

	ALTER TABLE Owner
	ADD CONSTRAINT fk_owner_user_id FOREIGN KEY(UserID) REFERENCES "User"(ID) ON DELETE CASCADE;
	
	ALTER TABLE Property
	ADD CONSTRAINT fk_property_owner_id FOREIGN KEY(OwnerID) REFERENCES Owner(ID) ON DELETE CASCADE,
	CONSTRAINT fk_property_purpose_code FOREIGN KEY(PurposeCode) REFERENCES PropertyPurpose(ID),
	CONSTRAINT fk_property_type_id FOREIGN KEY(TypeID) REFERENCES PropertyType(ID),
	CONSTRAINT fk_property_ad_type_code FOREIGN KEY(AdTypeCode) REFERENCES AdType(ID),
	CONSTRAINT fk_property_condition_code FOREIGN KEY(ConditionCode) REFERENCES PropertyCondition(ID),
	CONSTRAINT fk_property_ad_priority_code FOREIGN KEY(AdPriorityCode) REFERENCES AdPriority(ID),
	CONSTRAINT fk_property_category_code FOREIGN KEY(CategoryCode) REFERENCES PropertyCategory(ID);

	ALTER TABLE PropertyType
	ADD CONSTRAINT fk_property_type_category_code FOREIGN KEY(CategoryCode) REFERENCES PropertyCategory(ID);

	ALTER TABLE TagType
	ADD CONSTRAINT fk_tag_type_property_category_id FOREIGN KEY(PropertyCategoryCode) REFERENCES PropertyCategory(ID);

	ALTER TABLE Tags
	ADD CONSTRAINT fk_tags_property_id FOREIGN KEY(PropertyID) REFERENCES Property(ID) ON DELETE CASCADE,
	CONSTRAINT fk_tags_type_id FOREIGN KEY(TypeID) REFERENCES TagType(ID);

	ALTER TABLE PropertyRating
	ADD CONSTRAINT fk_property_rating_property_id FOREIGN KEY(PropertyID) REFERENCES Property(ID) ON DELETE CASCADE;

	ALTER TABLE Meeting
	ADD CONSTRAINT fk_meeting_user_id FOREIGN KEY(InviterUserID) REFERENCES "User"(ID) ON DELETE CASCADE;

	ALTER TABLE MeetingMembers
	ADD CONSTRAINT pk_meeting_members PRIMARY KEY(MeetingID,InviteesUserID),
	CONSTRAINT fk_meeting_members_member_id FOREIGN KEY(MeetingID) REFERENCES Meeting(ID) ON DELETE CASCADE,
	CONSTRAINT fk_meeting_members_invitee_id FOREIGN KEY(InviteesUserID) REFERENCES "User"(ID);

	ALTER TABLE Subscription
	ADD CONSTRAINT fk_subscription_owner_id FOREIGN KEY(OwnerID) REFERENCES Owner(ID) ON DELETE CASCADE,
	CONSTRAINT fk_subscription_type_code FOREIGN KEY(TypeCode) REFERENCES SubscriptionType(ID);

	ALTER TABLE Tennant
	ADD CONSTRAINT fk_tennant_property_id FOREIGN KEY(PropertyID) REFERENCES Property(ID) ON DELETE CASCADE,
	CONSTRAINT fk_tennant_user_id FOREIGN KEY(UserID) REFERENCES "User"(ID);

	ALTER TABLE Bill
	ADD CONSTRAINT fk_bill_owner_id FOREIGN KEY(OwnerID) REFERENCES Owner(ID) ON DELETE CASCADE,
	CONSTRAINT fk_bill_tennant_id FOREIGN KEY(TennantID) REFERENCES Tennant(ID),
	CONSTRAINT fk_bill_property_id FOREIGN KEY(PropertyID) REFERENCES Property(ID),
	CONSTRAINT fk_bill_type_code FOREIGN KEY(TypeCode) REFERENCES BillType(ID);

	ALTER TABLE Complaint
	ADD CONSTRAINT fk_complaint_owner_id FOREIGN KEY(OwnerID) REFERENCES Owner(ID) ON DELETE CASCADE,
	CONSTRAINT fk_complaint_tennant_id FOREIGN KEY(TennantID) REFERENCES Tennant(ID),
	CONSTRAINT fk_complaint_property_id FOREIGN KEY(PropertyID) REFERENCES Property(ID) ;

	ALTER TABLE PropertyImage
	ADD CONSTRAINT fk_property_image_property_id FOREIGN KEY(PropertyID) REFERENCES Property(ID) ON DELETE CASCADE;

	ALTER TABLE PropertyRequisition
	ADD CONSTRAINT fk_property_requisition_property_id FOREIGN KEY(PropertyID) REFERENCES Property(ID) ON DELETE CASCADE,
	CONSTRAINT fk_property_requisition_user_id FOREIGN KEY(UserID) REFERENCES "User"(ID);

	ALTER TABLE MessageTrash
	ADD CONSTRAINT pk_msg_id_usr_id PRIMARY KEY(UserID,MessageID),
	CONSTRAINT fk_msg_trash_user_id FOREIGN KEY(UserID) REFERENCES "User"(ID),
	CONSTRAINT fk_msg_trash_msg_id FOREIGN KEY(MessageID) REFERENCES Message(ID) ON DELETE CASCADE;

	ALTER TABLE SavedProperties
	ADD CONSTRAINT fk_user_sp FOREIGN KEY(UserID) REFERENCES "User"(ID),
	CONSTRAINT fk_properties_sp FOREIGN KEY(PropertyID) REFERENCES Property(ID) ON DELETE CASCADE;

	ALTER TABLE Payment
	ADD CONSTRAINT fk_payment_payment_type FOREIGN KEY(PaymentMethodID) REFERENCES PaymentMethod(ID),
	CONSTRAINT fk_payment_subscription_type FOREIGN KEY(SubscriptionID) REFERENCES Subscription(ID);

	ALTER TABLE SubscriptionExtension
	ADD CONSTRAINT fk_subscriptionext_payment FOREIGN KEY(PaymentID) REFERENCES Payment(ID);

	ALTER TABLE PasswordRecoveryRequest
	ADD CONSTRAINT fk_pswd_recvry_user_id FOREIGN KEY(UserID) REFERENCES "User"(ID) ON DELETE CASCADE;
COMMIT

