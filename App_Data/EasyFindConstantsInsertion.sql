BEGIN TRAN
	USE EasyFindProperties;
	GO

	INSERT INTO UserType(ID,Name,Description)
	VALUES('C','Consumer','User generally browse the website for properties that are being rented, selling or leased'),
	('O','Property Owner','User who publishes there properties on the website'),
	('T','Tennant','User who resides or intends to reside at a property listed on the website');

	INSERT INTO PropertyPurpose(ID,Name,Description)
	VALUES('R','Residential','Properties that are used for living purposes'),
	('C','Commercial','Properties that are used to generate a profit'),
	('I','Industrial','Properties that are used for the manufactoring of goods');

	INSERT INTO PropertyCategory(ID,Name,Description)
	VALUES('R','Real Estate','Property that provides living accommodations for individuals'),
	('L','Lot','Land spaces on which can be used to create buildings or farming etc.'),
	('M','Machinery','Properties that can be used to reduce manual labour');

	INSERT INTO PropertyType(Name,CategoryCode)
	VALUES('Apartment','R'),
	('House','R'),
	('Town House','R'),
	('Resort','R'),
	('Office','R'),
	('Container','R'),
	('Farming Plot','L'),
	('Building Plot','L'),
	('Recreational Plot','L'),
	('Agricultural','M'),
	('Constructural','M'),
	('Motor Vehicle Part', 'M');

	INSERT INTO AdType(ID,Name,Description)
	VALUES('S','Sale','Properties being sold'),
	('R','Rent','Properties being rented'),
	('L','Lease','Properties being leased');

	INSERT INTO PropertyCondition(ID,Name,Description)
	VALUES('E','Excellent','Property was acceptable by 86% - 100% of people that it was surveyed by'),
	('G','Good','Property is acceptable by 66% - 85% of people that it was surveyed by'),
	('F','Fair','Property is acceptable by 46% - 65% of people that it was surveyed by'),
	('B','Bad','Property is acceptable by 0% - 45% of people that it was surveyed by'),
	('N','Not Surveryed','Property has not been surveyed');

	INSERT INTO AdPriority(ID,Name,Description,Cost,Value)
	VALUES('R','Regular','Property advertisments will be shown after high priority advertisments are displayed',0,3),
	('P','Adpro','Property advertisments will be given high priority, therefore displaying them first in the result set (Featured List)',1000,2),
	('M','AdPremium','Property advertisments will be given high priority, therefore displaying them first in the result set (Featured List).
		Properties having this Ad package will also be published on social media networks',4000,1);
	
	INSERT INTO SubscriptionType(ID,Name,MonthlyCost,Description)
	VALUES('B','Basic',1000,'Normal subsciption for advertisement of property'),
	('R','Realtor',5000,'Subcription that can be used by realtors which provides access to the portal that can be used to advertise and modify unlimited amounts of properties for the months specified'),
	('L','Landlord',3000,'Subcription that can be used by landlords which provides access to the portal that can be used to advertise properties for the months specified
		as well as it allows tennants and landlords to communicate');

	INSERT INTO BillType(ID,Name)
	VALUES('L','Light Bill'),
	('R','Rent'),
	('W','Water'),
	('M','Maintainace'),
	('C','Cable'),
	('I','Internet'),
	('P','Phone'),
	('B','Bundle');

	INSERT INTO TagType(Name,PropertyCategoryCode)
	VALUES('For Single','R'),
	('For Couple','R'),
	('For Family','R'),
	('For Vacation','R'),
	('For Farming','L'),
	('For Student','R'),
	('Has Washroom','R'),
	('Has Livingroom','R'),
	('Has Gate','R'),
	('Has Automatic Gate','R'),
	('Has Security','R'),
	('Has Garage','R'),
	('Has Internet','R'),
	('Is Furnished','R'),
	('Water Inclusive In Rent','R'),
	('Electricity Inclusive In Rent','R'),
	('Cable Inclusive In Rent','R'),
	('Gas Inclusive In Rent','R');

	INSERT INTO PaymentMethod(ID,Name)
	VALUES('D','Digicel Credit'),
	('F','Flow Credit'),
	('C','Credit Card'),
	('W','Western Union'),
	('P','PayPal');

	INSERT INTO Division(Name,CountryCode)
	VALUES('Kingston', 'JM'),
	('St Andrew', 'JM'),
	('Manchester', 'JM'),
	('St Catherine', 'JM'),
	('St Thomas', 'JM'),
	('St Mary', 'JM'),
	('Portland', 'JM'),
	('St Ann', 'JM'),
	('Trelawny', 'JM'),
	('St James', 'JM'),
	('Hanover', 'JM'),
	('Westmoreland', 'JM'),
	('St Elizabeth', 'JM'),
	('Clarendon', 'JM');



COMMIT

