CREATE DATABASE EasyFindProperties;
GO
BEGIN TRAN
	USE EasyFindProperties;
	GO
	CREATE TABLE Property
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	OwnerID UNIQUEIDENTIFIER NOT NULL,
	Title VARCHAR(255) NULL,
	PurposeCode CHAR(1) NOT NULL,
	TypeID UNIQUEIDENTIFIER NOT NULL,
	AdTypeCode CHAR(1) NOT NULL,
	AdPriorityCode CHAR(1) NOT NULL,
	ConditionCode CHAR(2) NOT NULL,
	CategoryCode CHAR(1) NOT NULL,
	StreetAddress VARCHAR(100) NOT NULL,
	Division VARCHAR(100) NOT NULL,
	Community VARCHAR(100) NOT NULL,
	NearByEstablishment VARCHAR(100) NULL,
	Country VARCHAR(50) NOT NULL,
	Longitude VARCHAR(25) NULL,
	Latitude VARCHAR(25) NULL,
	NearByEstablishmentLng VARCHAR(25) NULL,
	NearByEstablishmentLat VARCHAR(25) NULL,
	Price DECIMAL NOT NULL,
	SecurityDeposit DECIMAL NULL,
	Occupancy INTEGER Null,
	GenderPreferenceCode CHAR(1) NULL,
	Description VARCHAR(255) NOT NULL,
	Availability BIT DEFAULT 0 NOT NULL,
	EnrolmentKey VARCHAR(6) NULL,
	TermsAgreement VARCHAR(MAX) NULL,
	TotAvailableBathroom INTEGER NULL,
	TotRooms INTEGER NULL,
	Area DECIMAL NULL,
	IsReviewable BIT NULL,
	ModifiedBy VARCHAR(40) NULL,
	DateTModified DATETIME NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE PropertyPurpose
	(
	ID CHAR(1) PRIMARY KEY,
	Name VARCHAR(20) NOT NULL,
	Description VARCHAR(255) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE PropertyType
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	Name VARCHAR(50) NOT NULL,
	CategoryCode CHAR(1) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE PropertyCategory
	(
	ID CHAR(1) PRIMARY KEY,
	Name VARCHAR(20) NOT NULL,
	Description VARCHAR(255) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE PropertyCondition
	(
	ID CHAR(2) PRIMARY KEY,
	Name VARCHAR(20) NOT NULL,
	Description VARCHAR(255) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE PropertyRating
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	PropertyID UNIQUEIDENTIFIER NOT NULL,
	Ratings INTEGER NOT NULL,
	Comments VARCHAR(255) NULL,
	CritqueBy VARCHAR(50) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL,
	);

	CREATE TABLE Tags
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	PropertyID UNIQUEIDENTIFIER NOT NULL,
	TypeID UNIQUEIDENTIFIER NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);
	
	CREATE TABLE TagType
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	Name VARCHAR(40) NOT NULL,
	PropertyCategoryCode CHAR(1) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE AdType
	(
	ID CHAR(1) PRIMARY KEY,
	Name VARCHAR(20) NOT NULL,
	Description VARCHAR(255) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE AdPriority
	(
	ID CHAR(1) PRIMARY KEY,
	Name VARCHAR(20) NOT NULL,
	Cost DECIMAL NOT NULL,
	Value INTEGER NOT NULL,
	Description VARCHAR(255) NOT NULL,
	ModifiedBy VARCHAR(40) NULL,
	DateTModified DATETIME NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE "User"
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	FirstName VARCHAR(50) NOT NULL,
	LastName VARCHAR(50) NOT NULL,
	CellNum VARCHAR(15) UNIQUE NOT NULL,
	Email VARCHAR(255)  UNIQUE NOT NULL,
	DOB DATE NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE UserTypeAssoc
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	UserID UNIQUEIDENTIFIER NOT NULL,
	UserTypeCode CHAR(1) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE UserType
	(
	ID CHAR(1) PRIMARY KEY,
	Name VARCHAR(20) NOT NULL,
	Description VARCHAR(255) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE Owner
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	UserID UNIQUEIDENTIFIER NOT NULL,
	Organization VARCHAR(50) NULL,
	LogoUrl VARCHAR(255)  NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);


	CREATE TABLE PropertyRequisition
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	PropertyID UNIQUEIDENTIFIER NOT NULL,
	UserID UNIQUEIDENTIFIER NOT NULL,
	Msg VARCHAR(255)  NULL,
	IsAccepted BIT NULL,
	ExpiryDate DATE DEFAULT DATEADD(MONTH,1,GETDATE()) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);
	
	CREATE TABLE Meeting
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	InviterUserID UNIQUEIDENTIFIER NOT NULL,
	Location VARCHAR (200) NOT NULL,
	MeetingTitle VARCHAR(100) NOT NULL,
	MeetingDate DATETIME NOT NULL,
	MeetingHour VARCHAR(2) NOT NULL,
	MeetingMinute VARCHAR(2) NOT NULL,
	MeetingPeriod VARCHAR(2) NOT NULL,
	Purpose VARCHAR(255) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE MeetingMembers
	(
	MeetingID UNIQUEIDENTIFIER NOT NULL,
	InviteesUserID UNIQUEIDENTIFIER NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE Subscription
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	OwnerID UNIQUEIDENTIFIER NOT NULL,
	TypeCode CHAR(1) NOT NULL,
	Period INTEGER NOT NULL,
	StartDate DATE NULL,--date when payment was received
	ExpiryDate DATE NULL,
	ModifiedBy VARCHAR(40) NULL,
	DateTModified DATETIME NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE SubscriptionType
	(
	ID CHAR(1) PRIMARY KEY,
	Name VARCHAR(20) NOT NULL,
	MonthlyCost DECIMAL NOT NULL,
	Description VARCHAR(255) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE Tennant
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	UserID UNIQUEIDENTIFIER NOT NULL,
	PropertyID UNIQUEIDENTIFIER NOT NULL,
	RentAmt DECIMAL NOT NULL,
	SettlementPeriod INTEGER NOT NULL,
	InstitutionName VARCHAR(255) NULL,
	ProgrammeName VARCHAR(255) NULL,
	ProgrammeStartDate DATE NULL,
	ProgrammeEndDate DATE NULL,
	PhotoUrl VARCHAR(255) NULL,
	ReferencedLetterURL VARCHAR(255) NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE Bill
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	OwnerID UNIQUEIDENTIFIER NOT NULL,
	PropertyID UNIQUEIDENTIFIER NOT NULL,
	TennantID UNIQUEIDENTIFIER NOT NULL,
	TypeCode CHAR(1) NOT NULL,
	AmtDue DECIMAL NOT NULL,
	IsPaid BIT DEFAULT 0 NOT NULL,
	BillURL VARCHAR(255) NULL,
	DateDue DATE NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE BillType
	(
	ID CHAR(1) PRIMARY KEY,
	Name VARCHAR(50) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE Complaint
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	OwnerID UNIQUEIDENTIFIER NOT NULL,
	PropertyID UNIQUEIDENTIFIER NOT NULL,
	TennantID UNIQUEIDENTIFIER NOT NULL,
	Issue NVARCHAR(MAX) NOT NULL,
	IsResolved BIT DEFAULT 0 NULL,
	DateResolved DATETIME NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE Message
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	"To" UNIQUEIDENTIFIER NOT NULL,
	"From" UNIQUEIDENTIFIER NOT NULL,
	Msg NVARCHAR(MAX) NOT NULL,
	Seen Bit DEFAULT 0 NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE PropertyImage
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	PropertyID UNIQUEIDENTIFIER NOT NULL,
	ImageURL VARCHAR(255) NOT NULL,
	IsPrimaryDisplay BIT DEFAULT 0 NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE MessageTrash
	(
	UserID UNIQUEIDENTIFIER NOT NULL,
	MessageID UNIQUEIDENTIFIER NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL,
	);

	CREATE TABLE SavedProperties
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	UserID UNIQUEIDENTIFIER NOT NULL,
	PropertyID UNIQUEIDENTIFIER NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL,
	);

	CREATE TABLE Payment
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	SubscriptionID UNIQUEIDENTIFIER NOT NULL,
	PaymentMethodID CHAR(1) NOT NULL,
	Amount DECIMAL NOT NULL,
	VoucherNumber VARCHAR(255) NULL, -- used for mobile credits
	IsVerified BIT DEFAULT 0 NOT NULL,
	DateTModified DATETIME NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE PaymentMethod
	(
	ID CHAR(1) PRIMARY KEY,
	Name VARCHAR(100) NOT NULL,
	PaymentApplicationInstructions VARCHAR(255) NULL,
	DateTModified DATETIME NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE SubscriptionExtension
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	PaymentID UNIQUEIDENTIFIER NOT NULL,
	Period INT NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE Division
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	Name VARCHAR(100) NOT NULL,
	CountryCode VARCHAR(10) NOT NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);

	CREATE TABLE PasswordRecoveryRequest
	(
	ID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	UserID UNIQUEIDENTIFIER NOT NULL,
	AccessCode VARCHAR(5) NOT NULL,
	Processed BIT NOT NULL,
	ExpiryDate DATETIME NULL,
	DateTModified DATETIME NULL,
	DateTCreated DATETIME DEFAULT GETDATE() NOT NULL
	);
	
COMMIT

/*tags*
ForSingle BIT DEFAULT 0 NULL,
ForCouple BIT DEFAULT 0 NULL,
ForFamily BIT DEFAULT 0 NULL,
ForStudent BIT DEFAULT 0 NULL,
ForFarming BIT DEFAULT 0 NULL,
HasAC BIT DEFAULT 0 NULL,
IsWaterIncl BIT DEFAULT 0 NULL,
IsElectricityIncl BIT DEFAULT 0 NULL,
IsCableIncl BIT DEFAULT 0 NULL,
IsGasIncl BIT DEFAULT 0 NULL,
IsInternetIncl BIT DEFAULT 0 NULL,
IsFurnished BIT DEFAULT 0 NULL,
HasWashRoom BIT DEFAULT 0 NULL,
HasLivingRoom BIT DEFAULT 0 NULL,
HasGate BIT DEFAULT 0 NULL,
HasAutomaticGate BIT DEFAULT 0 NULL,
HasSecurity BIT DEFAULT 0 NULL,
HasGarage BIT DEFAULT 0 NULL
*/