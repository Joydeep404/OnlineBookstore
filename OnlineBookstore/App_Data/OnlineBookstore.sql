-- Create database (will be created via EF)
USE [master]
GO
IF DB_ID('OnlineBookstore') IS NOT NULL
    DROP DATABASE OnlineBookstore
GO

CREATE DATABASE OnlineBookstore
GO

USE OnlineBookstore
GO

-- 1. Tables
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE Books (
    BookId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Author NVARCHAR(150) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    Stock INT NOT NULL CHECK (Stock >= 0),
    CategoryId INT NOT NULL,
    ImageUrl NVARCHAR(255) NULL,
    Description NVARCHAR(MAX) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE Orders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending',
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE OrderDetails (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    BookId INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
    FOREIGN KEY (BookId) REFERENCES Books(BookId)
);

-- 2. Sample Data
INSERT INTO Categories (Name) VALUES 
('Fiction'), 
('Science'), 
('History'), 
('Programming');

INSERT INTO Books (Title, Author, Price, Stock, CategoryId, ImageUrl, Description) VALUES
('The Great Gatsby', 'F. Scott Fitzgerald', 12.99, 10, 1, 'https://via.placeholder.com/150?text=Book1', 'A classic American novel.'),
('Clean Code', 'Robert C. Martin', 35.00, 8, 4, 'https://via.placeholder.com/150?text=Book2', 'A handbook of agile software craftsmanship.'),
('A Brief History of Time', 'Stephen Hawking', 18.50, 15, 2, 'https://via.placeholder.com/150?text=Book3', 'Exploring cosmology and black holes.'),
('Sapiens', 'Yuval Noah Harari', 22.00, 5, 3, 'https://via.placeholder.com/150?text=Book4', 'A brief history of humankind.');

INSERT INTO Users (Username, Email, PasswordHash, FullName) VALUES
('admin', 'admin@example.com', 'admin123', 'Admin User');

-- 3. STORED PROCEDURES

CREATE PROCEDURE GetBooksByCategory
    @CategoryId INT = NULL
AS
BEGIN
    IF @CategoryId IS NULL
        SELECT * FROM Books
    ELSE
        SELECT * FROM Books WHERE CategoryId = @CategoryId
END
GO

CREATE TYPE OrderDetailTable AS TABLE (
    BookId INT,
    Quantity INT,
    UnitPrice DECIMAL(10,2)
);
GO

CREATE PROCEDURE PlaceOrder
    @UserId INT,
    @TotalAmount DECIMAL(10,2),
    @OrderDetailTable OrderDetailTable READONLY
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO Orders (UserId, TotalAmount, Status)
        VALUES (@UserId, @TotalAmount, 'Pending');
        
        DECLARE @OrderId INT = SCOPE_IDENTITY();

        INSERT INTO OrderDetails (OrderId, BookId, Quantity, UnitPrice)
        SELECT @OrderId, BookId, Quantity, UnitPrice
        FROM @OrderDetailTable;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE PROCEDURE GetUserOrders
    @UserId INT
AS
BEGIN
    SELECT 
        o.OrderId, o.OrderDate, o.TotalAmount, o.Status,
        od.Quantity, od.UnitPrice,
        b.Title, b.Author
    FROM Orders o
    JOIN OrderDetails od ON o.OrderId = od.OrderId
    JOIN Books b ON od.BookId = b.BookId
    WHERE o.UserId = @UserId
    ORDER BY o.OrderDate DESC;
END
GO

-- 4. TRIGGERS

CREATE TRIGGER trg_DecreaseStockOnOrder
ON OrderDetails
AFTER INSERT
AS
BEGIN
    UPDATE Books
    SET Stock = Stock - i.Quantity
    FROM Books b
    INNER JOIN inserted i ON b.BookId = i.BookId
    WHERE b.Stock >= i.Quantity;
END
GO

CREATE TRIGGER trg_CheckStockBeforeOrder
ON OrderDetails
INSTEAD OF INSERT
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN Books b ON i.BookId = b.BookId
        WHERE b.Stock < i.Quantity
    )
    BEGIN
        RAISERROR('Insufficient stock for one or more books.', 16, 1);
        RETURN;
    END

    INSERT INTO OrderDetails (OrderId, BookId, Quantity, UnitPrice)
    SELECT OrderId, BookId, Quantity, UnitPrice FROM inserted;
END
GO