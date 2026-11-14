IF OBJECT_ID(N'dbo.Books', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Books
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Books PRIMARY KEY,
        Title nvarchar(200) NOT NULL,
        Author nvarchar(160) NOT NULL,
        Isbn nvarchar(30) NULL,
        Category nvarchar(100) NULL,
        TotalCopies int NOT NULL,
        AvailableCopies int NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Name nvarchar(120) NOT NULL,
        Email nvarchar(256) NOT NULL,
        PasswordHash nvarchar(256) NOT NULL,
        Role nvarchar(20) NOT NULL,
        CreatedAt datetime2 NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.Members', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Members
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Members PRIMARY KEY,
        Name nvarchar(120) NOT NULL,
        Email nvarchar(256) NOT NULL,
        Phone nvarchar(30) NULL,
        JoinedAt datetime2 NOT NULL,
        UserId int NULL,
        CONSTRAINT FK_Members_Users_UserId FOREIGN KEY (UserId)
            REFERENCES dbo.Users (Id) ON DELETE SET NULL
    );
END;

IF OBJECT_ID(N'dbo.BorrowRecords', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BorrowRecords
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_BorrowRecords PRIMARY KEY,
        BookId int NOT NULL,
        MemberId int NOT NULL,
        BorrowedAt datetime2 NOT NULL,
        DueAt datetime2 NOT NULL,
        ReturnedAt datetime2 NULL,
        CONSTRAINT FK_BorrowRecords_Books_BookId FOREIGN KEY (BookId)
            REFERENCES dbo.Books (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_BorrowRecords_Members_MemberId FOREIGN KEY (MemberId)
            REFERENCES dbo.Members (Id) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Books_Isbn' AND object_id = OBJECT_ID(N'dbo.Books'))
    CREATE UNIQUE INDEX IX_Books_Isbn ON dbo.Books (Isbn) WHERE Isbn IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_Email' AND object_id = OBJECT_ID(N'dbo.Users'))
    CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users (Email);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Members_Email' AND object_id = OBJECT_ID(N'dbo.Members'))
    CREATE UNIQUE INDEX IX_Members_Email ON dbo.Members (Email);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Members_UserId' AND object_id = OBJECT_ID(N'dbo.Members'))
    CREATE UNIQUE INDEX IX_Members_UserId ON dbo.Members (UserId) WHERE UserId IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BorrowRecords_BookId' AND object_id = OBJECT_ID(N'dbo.BorrowRecords'))
    CREATE INDEX IX_BorrowRecords_BookId ON dbo.BorrowRecords (BookId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BorrowRecords_MemberId_ReturnedAt' AND object_id = OBJECT_ID(N'dbo.BorrowRecords'))
    CREATE INDEX IX_BorrowRecords_MemberId_ReturnedAt ON dbo.BorrowRecords (MemberId, ReturnedAt);
