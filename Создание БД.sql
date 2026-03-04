USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'BankDB')
DROP DATABASE BankDB;
GO

CREATE DATABASE BankDB;
GO

USE BankDB;
GO

IF OBJECT_ID('dbo.Transaction_Account', 'U') IS NOT NULL DROP TABLE dbo.Transaction_Account;
IF OBJECT_ID('dbo.CreditPayment', 'U') IS NOT NULL DROP TABLE dbo.CreditPayment;
IF OBJECT_ID('dbo.Transaction', 'U') IS NOT NULL DROP TABLE dbo.[Transaction];
IF OBJECT_ID('dbo.Credit', 'U') IS NOT NULL DROP TABLE dbo.Credit;
IF OBJECT_ID('dbo.Card', 'U') IS NOT NULL DROP TABLE dbo.Card;
IF OBJECT_ID('dbo.Account', 'U') IS NOT NULL DROP TABLE dbo.Account;
IF OBJECT_ID('dbo.Client', 'U') IS NOT NULL DROP TABLE dbo.Client;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE Client
(
    PassportNumber CHAR(10) NOT NULL PRIMARY KEY,
    LastName VARCHAR(50) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    MiddleName VARCHAR(50) NULL,
    BirthDate DATE NOT NULL,
    Phone VARCHAR(15) NOT NULL UNIQUE,
    City VARCHAR(50) NOT NULL,
    Street VARCHAR(100) NOT NULL,
    House VARCHAR(10) NOT NULL
);
GO

CREATE TABLE Users
(
    UserID INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	PassportNumber CHAR(10) NOT NULL UNIQUE,
    [Login] VARCHAR(20) NOT NULL UNIQUE,
    [Password] VARCHAR(15) NOT NULL
	FOREIGN KEY (PassportNumber) REFERENCES Client (PassportNumber)
);
GO

CREATE TABLE Account
(
    AccountID INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    PassportNumber CHAR(10) NOT NULL,
    AccountType VARCHAR(30) NOT NULL,
        CHECK (AccountType IN ('Текущий', 'Сберегательный')),
    Balance DECIMAL(15,2) NOT NULL DEFAULT 0.00,
        CHECK (Balance >= 0),
    OpenDate DATE NOT NULL,
    CloseDate DATE NULL,
    [Status] VARCHAR(20) NOT NULL,
        CHECK ([Status] IN ('Активен', 'Заблокирован', 'Закрыт')),
    FOREIGN KEY (PassportNumber) REFERENCES Client (PassportNumber)
);
GO

CREATE TABLE [Card]
(
    CardNumber CHAR(16) NOT NULL PRIMARY KEY,
    AccountID INT NOT NULL,
    CardType VARCHAR(20) NOT NULL,
        CHECK (CardType IN ('Дебетовая', 'Кредитная')),
    ExpiryDate DATE NOT NULL,
    IssueDate DATE NOT NULL,
    [Status] VARCHAR(20) NOT NULL,
        CHECK ([Status] IN ('Активна', 'Заблокирована')),
    FOREIGN KEY (AccountID) REFERENCES Account (AccountID)
);
GO

CREATE TABLE Credit
(
    CreditID INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    PassportNumber CHAR(10) NOT NULL,
    AccountID INT NOT NULL,
    Amount DECIMAL(15,2) NOT NULL,
        CHECK (Amount > 0),
    [Status] VARCHAR(20) NOT NULL,
        CHECK ([Status] IN ('Активен', 'Погашен', 'Просрочен')),
    TermMonths INT NOT NULL,
        CHECK (TermMonths > 0),
    InterestRate DECIMAL(5,2) NOT NULL,
        CHECK (InterestRate > 0),
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    FOREIGN KEY (PassportNumber) REFERENCES Client (PassportNumber),
    FOREIGN KEY (AccountID) REFERENCES Account (AccountID)
);
GO

CREATE TABLE CreditPayment
(
    PaymentID INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    CreditID INT NOT NULL,
    Amount DECIMAL(15,2) NOT NULL,
        CHECK (Amount > 0),
    PlannedDate DATE NOT NULL,
    ActualDate DATE NULL,
	[Status] VARCHAR(20) NOT NULL,
		CHECK ([Status] IN ('Оплачен', 'Просрочен', 'Ожидает')),
    FOREIGN KEY (CreditID) REFERENCES Credit (CreditID)
);
GO

CREATE TABLE [Transaction]
(
    TransactionID INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Amount DECIMAL(15,2) NOT NULL,
        CHECK (Amount > 0),
    [Status] VARCHAR(20) NOT NULL,
        CHECK ([Status] IN ('Выполнена', 'Отклонена', 'В обработке')),
    [Type] VARCHAR(30) NOT NULL,
        CHECK ([Type] IN ('Перевод', 'Пополнение', 'Снятие', 'Оплата')),
    Comment VARCHAR(255) NULL,
    OperationDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE Transaction_Account
(
	TransactionAccountID INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    TransactionID INT NOT NULL,
    AccountID INT NOT NULL,
    [Role] VARCHAR(20) NOT NULL,
        CHECK ([Role] IN ('Отправитель', 'Получатель')),
    FOREIGN KEY (TransactionID) REFERENCES [Transaction] (TransactionID),
    FOREIGN KEY (AccountID) REFERENCES Account (AccountID)
);
GO