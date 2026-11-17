-- Food Hero: NEW project, Microsoft SQL Server (MSSQL).
-- Run once using SSMS or another SQL client that supports GO.
-- Creates a new database and seven related tables; sample organizations are at the end.
-- If FoodHero already exists, use a DIFFERENT new database name in BOTH lines below.
-- This script is a fresh installation, not a migration of existing tables.
CREATE DATABASE FoodHero;
GO
USE FoodHero;
GO
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.Organizations (
        OrganizationId INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationName NVARCHAR(150) NOT NULL,
        OrganizationType VARCHAR(20) NOT NULL,
        Address NVARCHAR(500) NOT NULL,
        Phone NVARCHAR(30),
        CreatedAt DATETIME2 NOT NULL
    );

    CREATE TABLE dbo.Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(254) NOT NULL UNIQUE,
        AuthSubject NVARCHAR(200) NOT NULL UNIQUE,
        Role VARCHAR(20) NOT NULL,
        CreatedAt DATETIME2 NOT NULL ,
        Password NVARCHAR(100)  NOT NULL,
        CONSTRAINT FK_Users_Organization FOREIGN KEY (OrganizationId)
            REFERENCES dbo.Organizations(OrganizationId),
        CONSTRAINT UQ_Users_OrganizationUser UNIQUE (OrganizationId, UserId)
    );

    CREATE TABLE dbo.FoodCategories (
        CategoryId INT IDENTITY(1,1) PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL UNIQUE
    );

    CREATE TABLE dbo.FoodListings (
        FoodId INT IDENTITY(1,1) PRIMARY KEY,
        SchoolOrganizationId INT NOT NULL,
        CreatedByUserId INT NOT NULL,
        CategoryId INT NOT NULL,
        FoodName NVARCHAR(150) NOT NULL,
        Description NVARCHAR(1000),
        QuantityAvailable INT NOT NULL ,
        Unit NVARCHAR(30) NOT NULL ,
        PickupAddress NVARCHAR(500) NOT NULL,
        AvailableFrom DATETIME2 NOT NULL,
        CollectBy DATETIME2 NOT NULL,
        Status VARCHAR(20) NOT NULL ,
        CreatedAt DATETIME2 NOT NULL ,
        CONSTRAINT CK_FoodListings_CollectionWindow CHECK (CollectBy > AvailableFrom),
        CONSTRAINT FK_FoodListings_School FOREIGN KEY (SchoolOrganizationId)
            REFERENCES dbo.Organizations(OrganizationId),
        -- Creator must belong to the organization offering the food.
        CONSTRAINT FK_FoodListings_Creator FOREIGN KEY (SchoolOrganizationId, CreatedByUserId)
            REFERENCES dbo.Users(OrganizationId, UserId),
        CONSTRAINT FK_FoodListings_Category FOREIGN KEY (CategoryId)
            REFERENCES dbo.FoodCategories(CategoryId)
    );

    CREATE TABLE dbo.FoodImages (
        ImageId INT IDENTITY(1,1) PRIMARY KEY,
        FoodId INT NOT NULL,
        ImageObjectKey NVARCHAR(500) NOT NULL,
        UploadedAt DATETIME2 NOT NULL ,
        CONSTRAINT FK_FoodImages_Food FOREIGN KEY (FoodId)
            REFERENCES dbo.FoodListings(FoodId)
    );

    CREATE TABLE dbo.PickupRequests (
        RequestId INT IDENTITY(1,1) PRIMARY KEY,
        FoodId INT NOT NULL,
        RecipientOrganizationId INT NOT NULL,
        RequestedByUserId INT NOT NULL,
        QuantityRequested INT NOT NULL CHECK (QuantityRequested > 0),
        Status VARCHAR(20) NOT NULL,
        Notes NVARCHAR(1000),
        RequestedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_PickupRequests_Food FOREIGN KEY (FoodId)
            REFERENCES dbo.FoodListings(FoodId),
        CONSTRAINT FK_PickupRequests_Recipient FOREIGN KEY (RecipientOrganizationId)
            REFERENCES dbo.Organizations(OrganizationId),
        -- Requester must belong to the recipient organization.
        CONSTRAINT FK_PickupRequests_User FOREIGN KEY (RecipientOrganizationId, RequestedByUserId)
            REFERENCES dbo.Users(OrganizationId, UserId)
    );

    CREATE TABLE dbo.Pickups (
        PickupId INT IDENTITY(1,1) PRIMARY KEY,
        RequestId INT NOT NULL UNIQUE,
        ConfirmedByUserId INT NOT NULL,
        CollectedAt DATETIME2 NOT NULL ,
        Notes NVARCHAR(1000),
        -- UNIQUE RequestId means one request can have at most one pickup record.
        CONSTRAINT FK_Pickups_Request FOREIGN KEY (RequestId)
            REFERENCES dbo.PickupRequests(RequestId),
        CONSTRAINT FK_Pickups_Confirmer FOREIGN KEY (ConfirmedByUserId)
            REFERENCES dbo.Users(UserId)
    );

    CREATE INDEX IX_Users_Organization ON dbo.Users(OrganizationId);
    CREATE INDEX IX_FoodListings_Category ON dbo.FoodListings(CategoryId);
    CREATE INDEX IX_FoodListings_Browse ON dbo.FoodListings(Status, CollectBy);
    CREATE INDEX IX_FoodImages_Food ON dbo.FoodImages(FoodId);
    CREATE INDEX IX_PickupRequests_Food ON dbo.PickupRequests(FoodId);
    CREATE INDEX IX_PickupRequests_Recipient ON dbo.PickupRequests(RecipientOrganizationId);
    CREATE INDEX IX_Pickups_Confirmer ON dbo.Pickups(ConfirmedByUserId);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- RELATIONSHIPS:
-- Organizations 1 -> many Users
-- Organizations (school) 1 -> many FoodListings
-- Users (creator) 1 -> many FoodListings
-- FoodCategories 1 -> many FoodListings
-- FoodListings 1 -> many FoodImages
-- FoodListings 1 -> many PickupRequests
-- Organizations (recipient) 1 -> many PickupRequests
-- Users (requester) 1 -> many PickupRequests
-- PickupRequests 1 -> zero or one Pickups
-- Users (confirmer) 1 -> many Pickups
--
-- BACKEND RULES:
-- All times are UTC. AuthSubject comes from the authentication provider; no passwords here.
-- Only school organizations offer food; eligible recipient organizations request it.
-- Check active identity, organization permissions, deadlines and status transitions.
-- Approve a PENDING request and reserve stock in ONE transaction. Use a conditional
-- stock update requiring QuantityAvailable >= requested quantity; check affected rows.
-- Cancellation restores reserved stock exactly once. Confirm collection and insert its
-- pickup record in ONE transaction; verify the confirming user is authorized.
-- FoodImages stores file keys, not image bytes. Upload files separately.
-- No cascading deletion: referenced records cannot be deleted accidentally.

-- USER ROLES:
-- SCHOOL_STAFF: manage their school food listings and requests for those listings.
-- RECIPIENT_STAFF: request food for their charity, food bank or community organization.
-- VOLUNTEER: view permitted collection details and assist with pickups.
-- ADMIN: manage users and organizations across the system.
-- Role permissions must be implemented in the backend; CHECK only restricts role names.
-- Volunteers belong to an organization. This schema does not assign volunteers to pickups.

-- SAMPLE ORGANIZATIONS ONLY:
-- For an existing database, run only this section.
USE FoodHero;
GO

INSERT INTO dbo.Organizations
    (OrganizationName, OrganizationType, Address, Phone, CreatedAt)
VALUES
    (N'Sample School', 'SCHOOL', N'Sample school address, Kuala Lumpur', NULL, SYSUTCDATETIME()),
    (N'Sample Charity', 'CHARITY', N'Sample charity address, Petaling Jaya', NULL, SYSUTCDATETIME()),
    (N'Sample Food Bank', 'FOOD_BANK', N'Sample food bank address, Shah Alam', NULL, SYSUTCDATETIME()),
    (N'Sample Community Centre', 'COMMUNITY', N'Sample community address, Kuala Lumpur', NULL, SYSUTCDATETIME());
GO
