-- ============================================================
-- Currency Exchange Office System - Database Setup
-- Run this once against the ExchangeDB database in
-- SQL Server Object Explorer to create the required tables.
-- ============================================================

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    BalancePLN FLOAT NOT NULL DEFAULT 0
);

CREATE TABLE Transactions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Operation NVARCHAR(20) NOT NULL,      -- 'BUY' or 'SELL'
    CurrencyCode NVARCHAR(10) NOT NULL,   -- e.g. USD, EUR, GBP, CHF
    Amount FLOAT NOT NULL,                -- amount of foreign currency
    ValuePLN FLOAT NOT NULL,              -- value in PLN
    Date DATETIME NOT NULL DEFAULT GETDATE()
);
