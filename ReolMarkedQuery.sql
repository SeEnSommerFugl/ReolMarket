-- Opret tabeller
CREATE TABLE Customer(
    Customer_ID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    CustomerName NvarChar(50) NOT NULL,
    Email NvarChar(50) NOT NULL,
    Phone NvarChar(20) NOT NULL,
    [Address] NvarChar(155) NOT NULL,
    PostalCode int NOT NULL
);
GO

CREATE TABLE Booth(
    Booth_ID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    NumberOfShelves int NOT NULL,
    HasHangarbar bit NOT NULL,
    IsRented bit NOT NULL,
    BoothStatus int NOT NULL,
    BoothNumber int NOT NULL,
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
    ShoppingCart_ID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Quantity int NOT NULL,
    TotalPrice decimal(10,2) NOT NULL
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

CREATE TABLE Sale(
    Sale_ID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    ShoppingCart_ID UNIQUEIDENTIFIER NOT NULL,
    Payment_ID UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_ShoppingCart_Sale
        FOREIGN KEY(ShoppingCart_ID)
        REFERENCES ShoppingCart(ShoppingCart_ID)
);
GO

CREATE TABLE Payment(
    Payment_ID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    PaymentMethod NvarChar(100) NOT NULL,
    PaymentDate Date NOT NULL
);

-- Opret dummy kunder
INSERT INTO Customer(Customer_ID, CustomerName, Email, Phone, [Address], PostalCode)
VALUES
    (NEWID(), 'John Doe', 'johndoe@example.dk', '12345678', '123 Main Street, City A', '1000'),
    (NEWID(), 'Jane Smith', 'janesmith@example.dk', '23456789', '321 Second Street, City A', '1000');

-- Opret dummy booths og tildel dem til kunder
DECLARE @Customer1 UNIQUEIDENTIFIER = (SELECT Customer_ID FROM Customer WHERE CustomerName = 'John Doe');
DECLARE @Customer2 UNIQUEIDENTIFIER = (SELECT Customer_ID FROM Customer WHERE CustomerName = 'Jane Smith');

-- Insert booths (med og uden hangarbar)
INSERT INTO Booth(Booth_ID, NumberOfShelves, HasHangarbar, IsRented, BoothStatus, BoothNumber, Customer_ID)
VALUES
    (NEWID(), 3, 1, 1, 1, 1, @Customer1),
    (NEWID(), 6, 0, 1, 1, 2, @Customer1),
    (NEWID(), 3, 1, 1, 1, 3, @Customer2),
    (NEWID(), 6, 0, 1, 1, 4, @Customer2),
    (NEWID(), 3, 1, 0, 0, 5, NULL),
    (NEWID(), 6, 0, 0, 0, 6, NULL);

-- Hent booth-id'er ved at bruge JOIN
DECLARE @Booth1 UNIQUEIDENTIFIER;
DECLARE @Booth2 UNIQUEIDENTIFIER;

SELECT TOP 1 @Booth1 = Booth_ID FROM Booth WHERE Customer_ID = @Customer1;
SELECT TOP 1 @Booth2 = Booth_ID FROM Booth WHERE Customer_ID = @Customer2;

-- Opret dummy varer og tilknyt dem til booths
INSERT INTO Item(Item_ID, ItemName, ItemPrice, Booth_ID)
VALUES 
  (NEWID(), 'Item 1 - Product A', 100.00, @Booth1),
  (NEWID(), 'Item 2 - Product B', 150.00, @Booth1),
  (NEWID(), 'Item 3 - Product C', 200.00, @Booth2),
  (NEWID(), 'Item 4 - Product D', 250.00, @Booth2);