-- ============================================================
-- WheelDeal Car Rental System — Database Migration Script
-- Run this against SQL Server (LocalDB or MSSQL)
-- Database: WheelDealDb_Dev
-- ============================================================

-- Create DB if needed (run separately as sysadmin if required)
-- CREATE DATABASE WheelDealDb_Dev;
-- GO
-- USE WheelDealDb_Dev;
-- GO

-- ── Branches ──────────────────────────────────────────────────
CREATE TABLE [Branches] (
    [Id]        INT IDENTITY(1,1) NOT NULL,
    [Name]      NVARCHAR(100)     NOT NULL,
    [City]      NVARCHAR(100)     NOT NULL,
    [Address]   NVARCHAR(300)     NOT NULL,
    [Phone]     NVARCHAR(20)      NOT NULL,
    [ManagerId] INT               NULL,
    [IsActive]  BIT               NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Branches] PRIMARY KEY ([Id])
);
CREATE INDEX [IX_Branches_Name] ON [Branches] ([Name]);
CREATE INDEX [IX_Branches_City] ON [Branches] ([City]);
GO

-- ── Users ──────────────────────────────────────────────────────
CREATE TABLE [Users] (
    [Id]                   INT IDENTITY(1,1) NOT NULL,
    [Username]             NVARCHAR(50)      NOT NULL,
    [PasswordHash]         NVARCHAR(500)     NOT NULL,
    [Email]                NVARCHAR(200)     NOT NULL,
    [FullName]             NVARCHAR(150)     NOT NULL,
    [Phone]                NVARCHAR(20)      NULL,
    [Role]                 INT               NOT NULL DEFAULT 2,
    [BranchId]             INT               NULL,
    [CreatedByUserId]      INT               NULL,
    [IsActive]             BIT               NOT NULL DEFAULT 1,
    [IsLocked]             BIT               NOT NULL DEFAULT 0,
    [FailedLoginAttempts]  INT               NOT NULL DEFAULT 0,
    [LockedUntil]          DATETIME2         NULL,
    [LastActivityAt]       DATETIME2         NULL,
    [LastLoginAt]          DATETIME2         NULL,
    [CreatedAt]            DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Users_Branches] FOREIGN KEY ([BranchId])
        REFERENCES [Branches] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Users_CreatedBy] FOREIGN KEY ([CreatedByUserId])
        REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
CREATE UNIQUE INDEX [IX_Users_Email]    ON [Users] ([Email]);
CREATE INDEX       [IX_Users_Role]     ON [Users] ([Role]);
CREATE INDEX       [IX_Users_BranchId] ON [Users] ([BranchId]);
GO

-- Add FK from Branches.ManagerId → Users.Id  (added after Users table)
ALTER TABLE [Branches]
    ADD CONSTRAINT [FK_Branches_Manager]
    FOREIGN KEY ([ManagerId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL;
GO

-- ── Vehicles ───────────────────────────────────────────────────
CREATE TABLE [Vehicles] (
    [Id]                 INT IDENTITY(1,1) NOT NULL,
    [RegistrationNumber] NVARCHAR(20)      NOT NULL,
    [BranchId]           INT               NOT NULL,
    [AddedByUserId]      INT               NOT NULL,
    [Brand]              NVARCHAR(100)     NOT NULL,
    [Model]              NVARCHAR(100)     NOT NULL,
    [Year]               INT               NOT NULL,
    [Category]           INT               NOT NULL,
    [DailyRate]          DECIMAL(10,2)     NOT NULL,
    [Status]             INT               NOT NULL DEFAULT 0,
    [IsActive]           BIT               NOT NULL DEFAULT 1,
    [IsOffer]            BIT               NOT NULL DEFAULT 0,
    [DiscountPercent]    INT               NULL,
    [ColorHex]           NVARCHAR(10)      NULL,
    [FuelType]           NVARCHAR(30)      NULL,
    [Transmission]       NVARCHAR(20)      NULL,
    [Seats]              INT               NULL,
    [Rating]             DECIMAL(3,2)      NULL,
    [CreatedAt]          DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]          DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Vehicles_Branch] FOREIGN KEY ([BranchId])
        REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Vehicles_AddedBy] FOREIGN KEY ([AddedByUserId])
        REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
CREATE UNIQUE INDEX [IX_Vehicles_RegistrationNumber] ON [Vehicles] ([RegistrationNumber]);
CREATE INDEX       [IX_Vehicles_Category]            ON [Vehicles] ([Category]);
CREATE INDEX       [IX_Vehicles_BranchId]            ON [Vehicles] ([BranchId]);
CREATE INDEX       [IX_Vehicles_Status]              ON [Vehicles] ([Status]);
GO

-- ── Clients ────────────────────────────────────────────────────
CREATE TABLE [Clients] (
    [Id]         INT IDENTITY(1,1) NOT NULL,
    [FullName]   NVARCHAR(150)     NOT NULL,
    [Email]      NVARCHAR(200)     NOT NULL,
    [Phone]      NVARCHAR(20)      NOT NULL,
    [Address]    NVARCHAR(300)     NULL,
    [IsFlagged]  BIT               NOT NULL DEFAULT 0,
    [FlagReason] NVARCHAR(500)     NULL,
    [IsActive]   BIT               NOT NULL DEFAULT 1,
    [CreatedAt]  DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]  DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
);
CREATE UNIQUE INDEX [IX_Clients_Email]   ON [Clients] ([Email]);
CREATE UNIQUE INDEX [IX_Clients_Phone]   ON [Clients] ([Phone]);
CREATE INDEX       [IX_Clients_FullName] ON [Clients] ([FullName]);
GO

-- ── Rentals ────────────────────────────────────────────────────
CREATE TABLE [Rentals] (
    [Id]                  INT IDENTITY(1,1) NOT NULL,
    [VehicleId]           INT               NOT NULL,
    [ClientId]            INT               NOT NULL,
    [BranchId]            INT               NOT NULL,
    [CreatedByUserId]     INT               NOT NULL,
    [CompletedByUserId]   INT               NULL,
    [BookingReference]    NVARCHAR(20)      NOT NULL,
    [StartDate]           DATETIME2         NOT NULL,
    [EndDate]             DATETIME2         NOT NULL,
    [PickupLocation]      NVARCHAR(200)     NOT NULL,
    [ReturnLocation]      NVARCHAR(200)     NOT NULL,
    [TotalCost]           DECIMAL(10,2)     NOT NULL,
    [Status]              INT               NOT NULL DEFAULT 0,
    [CancellationReason]  NVARCHAR(500)     NULL,
    [ProtectionPlan]      NVARCHAR(50)      NULL,
    [Extras]              NVARCHAR(500)     NULL,
    [Notes]               NVARCHAR(1000)    NULL,
    [PayNow]              BIT               NOT NULL DEFAULT 1,
    [RowVersion]          ROWVERSION        NOT NULL,
    [CreatedAt]           DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]           DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Rentals] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Rentals_Vehicle] FOREIGN KEY ([VehicleId])
        REFERENCES [Vehicles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Rentals_Client] FOREIGN KEY ([ClientId])
        REFERENCES [Clients] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Rentals_Branch] FOREIGN KEY ([BranchId])
        REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Rentals_CreatedBy] FOREIGN KEY ([CreatedByUserId])
        REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Rentals_CompletedBy] FOREIGN KEY ([CompletedByUserId])
        REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
CREATE UNIQUE INDEX [IX_Rentals_BookingReference]  ON [Rentals] ([BookingReference]);
CREATE INDEX       [IX_Rentals_VehicleId]           ON [Rentals] ([VehicleId]);
CREATE INDEX       [IX_Rentals_ClientId]            ON [Rentals] ([ClientId]);
CREATE INDEX       [IX_Rentals_BranchId]            ON [Rentals] ([BranchId]);
CREATE INDEX       [IX_Rentals_Status]              ON [Rentals] ([Status]);
CREATE INDEX       [IX_Rentals_StartDate_EndDate]   ON [Rentals] ([StartDate], [EndDate]);
GO

-- ── RefreshTokens ──────────────────────────────────────────────
CREATE TABLE [RefreshTokens] (
    [Id]         INT IDENTITY(1,1) NOT NULL,
    [UserId]     INT               NOT NULL,
    [Token]      NVARCHAR(500)     NOT NULL,
    [ExpiresAt]  DATETIME2         NOT NULL,
    [IsRevoked]  BIT               NOT NULL DEFAULT 0,
    [RevokedAt]  DATETIME2         NULL,
    [CreatedAt]  DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_User] FOREIGN KEY ([UserId])
        REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
CREATE UNIQUE INDEX [IX_RefreshTokens_Token]  ON [RefreshTokens] ([Token]);
CREATE INDEX       [IX_RefreshTokens_UserId]  ON [RefreshTokens] ([UserId]);
GO

-- ── AuditLogs ──────────────────────────────────────────────────
CREATE TABLE [AuditLogs] (
    [Id]         INT IDENTITY(1,1) NOT NULL,
    [UserId]     INT               NULL,
    [EntityType] NVARCHAR(100)     NOT NULL,
    [EntityId]   INT               NOT NULL,
    [Action]     NVARCHAR(100)     NOT NULL,
    [Details]    NVARCHAR(1000)    NOT NULL,
    [Timestamp]  DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AuditLogs_User] FOREIGN KEY ([UserId])
        REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
CREATE INDEX [IX_AuditLogs_EntityType_EntityId] ON [AuditLogs] ([EntityType], [EntityId]);
CREATE INDEX [IX_AuditLogs_Timestamp]           ON [AuditLogs] ([Timestamp]);
CREATE INDEX [IX_AuditLogs_UserId]              ON [AuditLogs] ([UserId]);
GO

-- ── SecurityAlerts ─────────────────────────────────────────────
CREATE TABLE [SecurityAlerts] (
    [Id]          INT IDENTITY(1,1) NOT NULL,
    [UserId]      INT               NOT NULL,
    [AlertType]   INT               NOT NULL,
    [Description] NVARCHAR(500)     NOT NULL,
    [IsRead]      BIT               NOT NULL DEFAULT 0,
    [CreatedAt]   DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_SecurityAlerts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SecurityAlerts_User] FOREIGN KEY ([UserId])
        REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
CREATE INDEX [IX_SecurityAlerts_UserId] ON [SecurityAlerts] ([UserId]);
CREATE INDEX [IX_SecurityAlerts_IsRead] ON [SecurityAlerts] ([IsRead]);
GO

-- ── EF Core Migrations history table ──────────────────────────
CREATE TABLE [__EFMigrationsHistory] (
    [MigrationId]    NVARCHAR(150) NOT NULL,
    [ProductVersion] NVARCHAR(32)  NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
);
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES ('20260504000001_InitialCreate', '8.0.11');
GO

PRINT 'WheelDeal database schema created successfully.';
GO
