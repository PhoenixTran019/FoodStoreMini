Create Database FoodStoredb
go

Use FoodStoredb;
go


--Table Roles
Create table Roles
(
	RoleID nvarchar(200) Primary Key,
	RoleName nvarchar(100),
	Description nvarchar(200)
);
go

--Table Users
Create table Users
(
	UserID nvarchar(200) Primary Key, --Using UUIDv7
	Username nvarchar(50),
	PhoneNumber nvarchar(20),
	PasswordHash nvarchar(555),
	RoleID nvarchar(200) Foreign Key References Roles(RoleID),
	IsActive Bit Default 1,
	CreateAt DateTime2 Default GetDate()
); 
go

--Table Profiles for user
Create Table UserProfiles
(
	ProfileID nvarchar(200) Primary Key, --Using UUIDv7
	UserID nvarchar(200) Foreign Key References Users(UserID),
	FirstName nvarchar (50),
	LastName nvarchar(50),
	Email nvarchar(200),
	Address nvarchar(555),
	AvatarURL nvarchar(MAX)
);
go

--Table Categories
Create Table Categories
(
	CategoryID nvarchar(200) Primary Key, --Using UUIDv7 or self-define
	CategoryName nvarchar (150)
);
go

--Table for food
Create Table FoodItems
(
	FoodID nvarchar(200) Primary Key, --Using UUIDv7 or self-define
	CategoryID nvarchar(200) Foreign Key References Categories (CategoryID),
	FoodName nvarchar(100),
	Description nvarchar(555),
	Price Decimal (18,2),
	ImageURL nvarchar(Max),
	IsAvailable Bit Default 1
);
go

Create Table  Combos
(
	ComboID nvarchar(200) Primary Key,
	ComboName nvarchar(255),
	Description nvarchar(555),
	Price Decimal(18,2),
	ImageURL varchar(MAX),
	IsAvailable BIT Default 1
);
go

Create Table ComboDetails
(
	DetailID nvarchar(200) Primary Key,
	ComboID nvarchar(200) Foreign Key References Combos (ComboID),
	FoodID nvarchar(200) Foreign Key References FoodItems (FoodID),
	Quantity int Default 1
);
go

Create Table OrderStatus
(
	StatusID nvarchar(200) Primary Key, --Using UUIDv7 or self-define
	StatusName nvarchar(155)
);
go

Create Table Orders
(
	OrderID nvarchar (200) Primary Key,
	CustomerID nvarchar(200) Foreign Key References Users(UserID), -- NguoiDat
	ShipperID nvarchar(200) Foreign Key References Users(UserID),
	StatusID nvarchar(200) Foreign Key References OrderStatus (StatusID),
	OrderDate Datetime2 Default GetDate(),
	TotalAmout Decimal(18,2),
	DeliveryAddress nvarchar(555),
	Note nvarchar(Max)
);
go

Create Table OrderDetails
(
	OrderDetailID nvarchar(200) Primary Key,
	OrderID nvarchar(200) Foreign Key References Orders (OrderID),
	FoodID nvarchar(200) Foreign Key References FoodItems(FoodID),
	ComboID nvarchar(200) Foreign Key References Combos (ComboID), 
	Quantity int,
	UnitPrice Decimal(18, 2)
);
go

Create Table OrderTracking
(
	TrackingID nvarchar(200) Primary Key,
	OrderID nvarchar(200) Foreign Key References Orders(OrderID),
	StatusID nvarchar(200) Foreign Key References OrderStatus (StatusID),
	UpdateTime DateTime2 Default GetDate()
);
go

Create Table ActivityLogs
(
	LogID nvarchar(200) Primary Key,
	UserID nvarchar(200) Foreign Key References Users (UserID),
	Action nvarchar(555),
	TagetTable nvarchar (255),
	TargetID nvarchar (200),
	TargetName nvarchar(200),
	TimeStamp DateTime2 Default GetDate()
); 
