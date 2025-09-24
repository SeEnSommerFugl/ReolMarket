-- Opret tabeller
CREATE TABLE Customer(
    Customer_ID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    CustomerName NvarChar(50) NOT NULL,
    Email NvarChar(50) NOT NULL,
    PhoneNumber NvarChar(20) NOT NULL,
    [Address] NvarChar(155) NOT NULL,
    PostalCode NvarChar(10) NOT NULL
);
GO

CREATE TABLE Booth(
    Booth_ID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    BoothNumber int NOT NULL,
    NumberOfShelves int NOT NULL,
    HasHangerBar bit NOT NULL,
    IsRented bit NOT NULL,
    [Status] int NOT NULL,
    StartRentDate Date,
    EndRentDate Date,
    Customer_ID UNIQUEIDENTIFIER,
    CONSTRAINT FK_Booth_Customer
        FOREIGN KEY(Customer_ID)
        REFERENCEs Customer(Customer_ID)
);
GO

CREATE TABLE Item(
    Item_ID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    ItemName NvarChar(255) NOT NULL,
    ItemPrice Decimal(10,2) NOT NULL,
    Booth_ID UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_Item_Booth
        FOREIGN KEY(Booth_ID)
        REFERENCES Booth(Booth_ID)
);
GO

CREATE TABLE ShoppingCart(
    ShoppingCart_ID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY
);
GO

CREATE TABLE ItemShoppingCart(
    Item_ID UNIQUEIDENTIFIER NOT NULL,
    ShoppingCart_ID UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_Item_ItemShoppingCart
        FOREIGN KEY(Item_ID)
        REFERENCES Item(Item_ID),
    CONSTRAINT FK_ShoppingCart_ItemShoppingCart
        FOREIGN KEY(ShoppingCart_ID)
        REFERENCES ShoppingCart(ShoppingCart_ID)
);
GO

CREATE TABLE Payment(
    Payment_ID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    PaymentMethod NvarChar(100) NOT NULL
);
GO

CREATE TABLE Sale(
    Sale_ID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    SaleDate Date NOT NULL,
    ShoppingCart_ID UNIQUEIDENTIFIER NOT NULL,
    Payment_ID UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_ShoppingCart_Sale
        FOREIGN KEY(ShoppingCart_ID)
        REFERENCES ShoppingCart(ShoppingCart_ID),
    CONSTRAINT FK_Payment_Sale
        FOREIGN KEY(Payment_ID)
        REFERENCES Payment(Payment_ID)
);
GO

-- Opret dummy kunder
INSERT INTO Customer(Customer_ID, CustomerName, Email, PhoneNumber, [Address], PostalCode)
VALUES
    (NEWID(), 'John Doe', 'johndoe@example.dk', '12345678', '123 Main Street, City A', '1000'),
    (NEWID(), 'Jane Smith', 'janesmith@example.dk', '23456789', '321 Second Street, City A', '1000'),
    (NEWID(), 'John Doesmith', 'doesmith@example.dk', '34567890', '213 Third Street City A', '1000');

-- Opret dummy booths og tildel dem til kunder
DECLARE @Customer1 UNIQUEIDENTIFIER = (SELECT Customer_ID FROM Customer WHERE CustomerName = 'John Doe');
DECLARE @Customer2 UNIQUEIDENTIFIER = (SELECT Customer_ID FROM Customer WHERE CustomerName = 'Jane Smith');
DECLARE @Customer3 UNIQUEIDENTIFIER = (SELECT Customer_ID FROM Customer WHERE CustomerName = 'John Doesmith');

-- Insert booths (med og uden hangerbar)
INSERT INTO Booth(Booth_ID, NumberOfShelves, HasHangerBar, IsRented, [Status], BoothNumber, StartRentDate, EndRentDate, Customer_ID)
VALUES
    (NEWID(), 3, 1, 1, 1, 1, '2025-09-01', '2025-09-30', @Customer1),
    (NEWID(), 6, 0, 1, 1, 2, '2025-09-01', '2025-09-30', @Customer1),
    (NEWID(), 3, 1, 1, 1, 3, '2025-09-01', '2025-09-30', @Customer2),
    (NEWID(), 6, 0, 1, 1, 4, '2025-09-01', '2025-09-30', @Customer2),
    (NEWID(), 3, 1, 1, 1, 5, '2025-09-01', '2025-09-30', @Customer3),
    (NEWID(), 6, 0, 0, 0, 6, NULL, NULL, NULL),
    (NEWID(), 3, 1, 0, 0, 7, NULL, NULL, NULL),
    (NEWID(), 6, 0, 0, 0, 8, NULL, NULL, NULL),
    (NEWID(), 3, 1, 0, 0, 9, NULL, NULL, NULL),
    (NEWID(), 6, 0, 0, 0, 10, NULL, NULL, NULL),
    (NEWID(), 3, 1, 0, 0, 11, NULL, NULL, NULL),
    (NEWID(), 6, 0, 0, 0, 12, NULL, NULL, NULL),
    (NEWID(), 3, 1, 0, 0, 13, NULL, NULL, NULL),
    (NEWID(), 6, 0, 0, 0, 14, NULL, NULL, NULL),
    (NEWID(), 3, 1, 0, 0, 15, NULL, NULL, NULL),
    (NEWID(), 6, 0, 0, 0, 16, NULL, NULL, NULL),
    (NEWID(), 3, 1, 0, 0, 17, NULL, NULL, NULL),
    (NEWID(), 6, 0, 0, 0, 18, NULL, NULL, NULL),
    (NEWID(), 3, 1, 0, 0, 19, NULL, NULL, NULL),
    (NEWID(), 6, 0, 0, 0, 20, NULL, NULL, NULL);

-- Hent booth-id'er ved at bruge JOIN
DECLARE @Booth1 UNIQUEIDENTIFIER;
DECLARE @Booth2 UNIQUEIDENTIFIER;
DECLARE @Booth3 UNIQUEIDENTIFIER;

SELECT TOP 1 @Booth1 = Booth_ID FROM Booth WHERE Customer_ID = @Customer1;
SELECT TOP 1 @Booth2 = Booth_ID FROM Booth WHERE Customer_ID = @Customer2;
SELECT TOP 1 @Booth3 = Booth_ID FROM Booth WHERE Customer_ID = @Customer3;

-- Opret dummy varer og tilknyt dem til booths
INSERT INTO Item(Item_ID, ItemName, ItemPrice, Booth_ID)
VALUES 
  (NEWID(), 'Fad', 100.00, @Booth1),
  (NEWID(), 'Vase', 150.00, @Booth1),
  (NEWID(), 'Verdenskort', 200.00, @Booth2),
  (NEWID(), 'Sko', 250.00, @Booth2),
  (NEWID(), 'Ur', 450.00, @Booth3),
  (NEWID(), 'Skjorte', 350.00, @Booth3);

-- Opret indkøbskurv
DECLARE @Cart1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Cart2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO ShoppingCart(ShoppingCart_ID)
VALUES 
    (@Cart1),
    (@Cart2);

DECLARE @Item1 UNIQUEIDENTIFIER, @Item2 UNIQUEIDENTIFIER, @Item3 UNIQUEIDENTIFIER, @Item4 UNIQUEIDENTIFIER, @Item5 UNIQUEIDENTIFIER, @Item6 UNIQUEIDENTIFIER;

SELECT @Item1 = Item_ID FROM Item WHERE ItemName = 'Fad';
SELECT @Item2 = Item_ID FROM Item WHERE ItemName = 'Vase';
SELECT @Item3 = Item_ID FROM Item WHERE ItemName = 'Verdenskort';
SELECT @Item4 = Item_ID FROM Item WHERE ItemName = 'Sko';
SELECT @Item5 = Item_ID FROM Item WHERE ItemName = 'Ur';
SELECT @Item6 = Item_ID FROM Item WHERE ItemName = 'Skjorte';

-- Varer lægges i kurv
INSERT INTO ItemShoppingCart(Item_ID, ShoppingCart_ID)
VALUES
    (@Item1, @Cart1),
    (@Item2, @Cart1),
    (@Item3, @Cart1);

INSERT INTO ItemShoppingCart(Item_ID, ShoppingCart_ID)
VALUES
    (@Item4, @Cart2),
    (@Item5, @Cart2),
    (@Item6, @Cart2);

-- Opret betalingsmetoder
DECLARE @Payment1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Payment2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO Payment(Payment_ID, PaymentMethod, PaymentDate)
VALUES
    (@Payment1, 'Kort'),
    (@Payment2, 'Kontant');

-- Opret Salg
INSERT INTO Sale(Sale_ID, ShoppingCart_ID, Payment_ID)
VALUES
    (NEWID(), @Cart1, @Payment1, GETDATE()),
    (NEWID(), @Cart2, @Payment2, GETDATE());
