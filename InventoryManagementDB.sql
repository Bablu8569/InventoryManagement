-- Create Database
CREATE DATABASE InventoryDB;
GO

USE InventoryDB;
GO

-- Create Categories Table
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Create Products Table
CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(200) NOT NULL,
    CategoryId INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);
GO

-- Create StockTransactions Table
CREATE TABLE StockTransactions (
    TransactionId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    TransactionType NVARCHAR(20) NOT NULL, -- 'IN' or 'OUT'
    Quantity INT NOT NULL,
    Remarks NVARCHAR(500),
    TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
GO

-- STORED PROCEDURE

-- Category Procedures
CREATE PROCEDURE USP_GetCategories
AS
BEGIN
    SELECT CategoryId, CategoryName, IsActive, CreatedDate
    FROM Categories
    ORDER BY CategoryName
END
GO

CREATE PROCEDURE USP_GetCategoryById
    @CategoryId INT
AS
BEGIN
    SELECT CategoryId, CategoryName, IsActive, CreatedDate
    FROM Categories
    WHERE CategoryId = @CategoryId
END
GO

CREATE PROCEDURE USP_InsertCategory
    @CategoryName NVARCHAR(100),
    @IsActive BIT
AS
BEGIN
    IF EXISTS(SELECT 1 FROM Categories WHERE CategoryName = @CategoryName)
    BEGIN
        SELECT -1 AS Result, 'Category name already exists!' AS Message
        RETURN
    END
    
    INSERT INTO Categories (CategoryName, IsActive, CreatedDate)
    VALUES (@CategoryName, @IsActive, GETDATE())
    
    SELECT 1 AS Result, 'Category added successfully!' AS Message
END
GO

CREATE PROCEDURE USP_UpdateCategory
    @CategoryId INT,
    @CategoryName NVARCHAR(100),
    @IsActive BIT
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM Categories WHERE CategoryId = @CategoryId)
    BEGIN
        SELECT -1 AS Result, 'Category not found!' AS Message
        RETURN
    END
    
    UPDATE Categories 
    SET CategoryName = @CategoryName, IsActive = @IsActive
    WHERE CategoryId = @CategoryId
    
    SELECT 1 AS Result, 'Category updated successfully!' AS Message
END
GO

CREATE PROCEDURE USP_DeleteCategory
    @CategoryId INT
AS
BEGIN
    IF EXISTS(SELECT 1 FROM Products WHERE CategoryId = @CategoryId)
    BEGIN
        SELECT -1 AS Result, 'Cannot delete category as it has associated products!' AS Message
        RETURN
    END
    
    DELETE FROM Categories WHERE CategoryId = @CategoryId
    SELECT 1 AS Result, 'Category deleted successfully!' AS Message
END
GO

-- Product Procedures
CREATE PROCEDURE USP_GetProducts
AS
BEGIN
    SELECT p.ProductId, p.ProductName, p.CategoryId, c.CategoryName, 
           p.Price, p.Quantity, p.CreatedDate
    FROM Products p
    INNER JOIN Categories c ON p.CategoryId = c.CategoryId
    ORDER BY p.ProductName
END
GO

CREATE PROCEDURE USP_GetProductById
    @ProductId INT
AS
BEGIN
    SELECT p.ProductId, p.ProductName, p.CategoryId, c.CategoryName, 
           p.Price, p.Quantity, p.CreatedDate
    FROM Products p
    INNER JOIN Categories c ON p.CategoryId = c.CategoryId
    WHERE p.ProductId = @ProductId
END
GO

CREATE PROCEDURE USP_InsertProduct
    @ProductName NVARCHAR(200),
    @CategoryId INT,
    @Price DECIMAL(18,2),
    @Quantity INT
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM Categories WHERE CategoryId = @CategoryId)
    BEGIN
        SELECT -1 AS Result, 'Invalid category selected!' AS Message
        RETURN
    END
    
    INSERT INTO Products (ProductName, CategoryId, Price, Quantity, CreatedDate)
    VALUES (@ProductName, @CategoryId, @Price, @Quantity, GETDATE())
    
    SELECT 1 AS Result, 'Product added successfully!' AS Message
END
GO

CREATE PROCEDURE USP_UpdateProduct
    @ProductId INT,
    @ProductName NVARCHAR(200),
    @CategoryId INT,
    @Price DECIMAL(18,2),
    @Quantity INT
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM Products WHERE ProductId = @ProductId)
    BEGIN
        SELECT -1 AS Result, 'Product not found!' AS Message
        RETURN
    END
    
    UPDATE Products 
    SET ProductName = @ProductName, CategoryId = @CategoryId, 
        Price = @Price, Quantity = @Quantity
    WHERE ProductId = @ProductId
    
    SELECT 1 AS Result, 'Product updated successfully!' AS Message
END
GO

CREATE PROCEDURE USP_DeleteProduct
    @ProductId INT
AS
BEGIN
    -- Delete stock transactions first
    DELETE FROM StockTransactions WHERE ProductId = @ProductId
    DELETE FROM Products WHERE ProductId = @ProductId
    
    SELECT 1 AS Result, 'Product deleted successfully!' AS Message
END
GO

CREATE PROCEDURE USP_SearchProducts
    @ProductName NVARCHAR(200) = NULL,
    @CategoryId INT = NULL
AS
BEGIN
    SELECT p.ProductId, p.ProductName, p.CategoryId, c.CategoryName, 
           p.Price, p.Quantity, p.CreatedDate
    FROM Products p
    INNER JOIN Categories c ON p.CategoryId = c.CategoryId
    WHERE (@ProductName IS NULL OR p.ProductName LIKE '%' + @ProductName + '%')
      AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
    ORDER BY p.ProductName
END
GO

-- Stock Transaction Procedures
CREATE PROCEDURE USP_InsertStockTransaction
    @ProductId INT,
    @TransactionType NVARCHAR(20),
    @Quantity INT,
    @Remarks NVARCHAR(500)
AS
BEGIN
    DECLARE @CurrentQuantity INT
    
    SELECT @CurrentQuantity = Quantity FROM Products WHERE ProductId = @ProductId
    
    IF @TransactionType = 'OUT' AND @CurrentQuantity < @Quantity
    BEGIN
        SELECT -1 AS Result, 'Insufficient stock available!' AS Message
        RETURN
    END
    
    INSERT INTO StockTransactions (ProductId, TransactionType, Quantity, Remarks, TransactionDate)
    VALUES (@ProductId, @TransactionType, @Quantity, @Remarks, GETDATE())
    
    -- Update product quantity
    IF @TransactionType = 'IN'
        UPDATE Products SET Quantity = Quantity + @Quantity WHERE ProductId = @ProductId
    ELSE
        UPDATE Products SET Quantity = Quantity - @Quantity WHERE ProductId = @ProductId
    
    SELECT 1 AS Result, 'Stock transaction completed successfully!' AS Message
END
GO

CREATE PROCEDURE USP_GetStockTransactionsByProduct
    @ProductId INT
AS
BEGIN
    SELECT TransactionId, ProductId, TransactionType, Quantity, 
           Remarks, TransactionDate
    FROM StockTransactions
    WHERE ProductId = @ProductId
    ORDER BY TransactionDate DESC
END
GO

CREATE PROCEDURE USP_GetDashboardStats
AS
BEGIN
    SELECT 
        (SELECT COUNT(*) FROM Categories WHERE IsActive = 1) AS TotalCategories,
        (SELECT COUNT(*) FROM Products) AS TotalProducts,
        (SELECT COUNT(*) FROM Products WHERE Quantity <= 5) AS LowStockCount,
        (SELECT COUNT(*) FROM StockTransactions WHERE CAST(TransactionDate AS DATE) = CAST(GETDATE() AS DATE)) AS TodayTransactions
END
GO

-- Insert sample data
INSERT INTO Categories (CategoryName, IsActive) VALUES 
('Electronics', 1),
('Clothing', 1),
('Books', 1);

INSERT INTO Products (ProductName, CategoryId, Price, Quantity) VALUES
('Laptop', 1, 50000, 10),
('Mobile Phone', 1, 15000, 25),
('T-Shirt', 2, 500, 100),
('Novel', 3, 300, 50);


SELECT * FROM Products