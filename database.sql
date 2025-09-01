-- SQL Server schema aligned with diagram (Owner, Property, PropertyImage, PropertyTrace)
IF DB_ID('RealEstateDb') IS NULL

    --Create Database RealEstateDb
    CREATE DATABASE RealEstateDb;
GO
USE RealEstateDb;
GO

IF OBJECT_ID('dbo.PropertyImage', 'U') IS NOT NULL DROP TABLE dbo.PropertyImage;
IF OBJECT_ID('dbo.PropertyTrace', 'U') IS NOT NULL DROP TABLE dbo.PropertyTrace;
IF OBJECT_ID('dbo.Property', 'U') IS NOT NULL DROP TABLE dbo.Property;
IF OBJECT_ID('dbo.Owner', 'U') IS NOT NULL DROP TABLE dbo.Owner;
GO

--Create Table Owner
CREATE TABLE dbo.Owner (
    IdOwner         INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(200) NOT NULL,
    Address         NVARCHAR(300) NULL,
    Photo           NVARCHAR(MAX) NULL,
    Birthday        DATETIME2 NULL
);
GO

--Create Table Property
CREATE TABLE Property (
    IdProperty INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Address NVARCHAR(300) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CodeInternal NVARCHAR(50) UNIQUE NOT NULL,
    Year INT NOT NULL,
    IdOwner INT NOT NULL FOREIGN KEY REFERENCES Owner(IdOwner),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    RowVersion ROWVERSION
);


--Create Table PropertyImage
CREATE TABLE dbo.PropertyImage (
    IdPropertyImage INT IDENTITY(1,1) PRIMARY KEY,
    IdProperty      INT NOT NULL,
    [file]          VARBINARY(MAX) NOT NULL,
    Enabled         BIT NOT NULL CONSTRAINT DF_PropertyImage_IsPrimary DEFAULT(1),
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_PropertyImage_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_PropertyImage_Property FOREIGN KEY (IdProperty) REFERENCES dbo.Property(IdProperty) ON DELETE CASCADE
);
GO

--Create Table PropertyTrace
CREATE TABLE dbo.PropertyTrace (
    IdPropertyTrace INT IDENTITY(1,1) PRIMARY KEY,
    IdProperty      INT NOT NULL,
    DateSale        DATETIME2(0) NOT NULL CONSTRAINT DF_PropertyTrace_DateSale DEFAULT (SYSUTCDATETIME()),
    Name            NVARCHAR(200) NULL,
    Value           DECIMAL(18,2) NOT NULL,
    Tax             DECIMAL(18,2) NOT NULL DEFAULT(0),
    CONSTRAINT FK_PropertyTrace_Property FOREIGN KEY (IdProperty) REFERENCES dbo.Property(IdProperty) ON DELETE CASCADE
);
GO

-- Indexes
CREATE INDEX IX_Property_Year ON dbo.Property ([Year]);
CREATE INDEX IX_Property_Price ON dbo.Property (Price);
CREATE INDEX IX_Property_CreatedAt ON dbo.Property (CreatedAt);
CREATE INDEX IX_PropertyImage_PropertyId ON dbo.PropertyImage (IdProperty);
CREATE INDEX IX_PropertyTrace_PropertyId ON dbo.PropertyTrace (IdProperty);
GO
