IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AccessHistoryLogs] (
    [Id] int NOT NULL IDENTITY,
    [IpAddress] nvarchar(max) NULL,
    [DateTime] datetime2 NOT NULL,
    [AccessPath] nvarchar(max) NULL,
    [SessionId] nvarchar(max) NULL,
    CONSTRAINT [PK_AccessHistoryLogs] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210406061003_InitStudyDocker', N'5.0.0');
GO

COMMIT;
GO

