IF OBJECT_ID('dbo.DemoTasks', 'U') IS NOT NULL
    DROP TABLE dbo.[DemoTasks];
GO

CREATE TABLE [dbo].[DemoTasks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[IsCompleted] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DemoTasks] ADD  DEFAULT ((0)) FOR [IsCompleted]
GO

ALTER TABLE [dbo].[DemoTasks] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

-- ============================================================================
-- T-SQL SCRIPT: Tabela Transakcji i Generator 10 000 rekordów (Big Data Demo)
-- Baza: Azure SQL Database (BlazorDemoDatabase)
-- ============================================================================

-- 1. Usunięcie starej tabeli jeśli istnieje
IF OBJECT_ID('dbo.SystemTransactions', 'U') IS NOT NULL
    DROP TABLE dbo.SystemTransactions;

-- 2. Utworzenie tabeli transakcji
CREATE TABLE dbo.SystemTransactions (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    TransactionNumber UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    CustomerEmail NVARCHAR(150) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency VARCHAR(3) DEFAULT 'PLN' NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    Description NVARCHAR(255) NULL,
    TransactionDate DATETIME2 NOT NULL
);

-- 3. Indeksy pod błyskawiczną paginację serwerową i filtrowanie
CREATE NONCLUSTERED INDEX IX_SystemTransactions_Date 
ON dbo.SystemTransactions (TransactionDate DESC);

CREATE NONCLUSTERED INDEX IX_SystemTransactions_Status 
ON dbo.SystemTransactions (Status);

-- 4. Błyskawiczny generator 10 000 rekordów (bezpieczny CASE)
;WITH Tally AS (
    SELECT TOP (10000)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
    FROM sys.all_columns a
    CROSS JOIN sys.all_columns b
)
INSERT INTO dbo.SystemTransactions (
    TransactionNumber,
    CustomerEmail,
    Amount,
    Currency,
    PaymentMethod,
    Status,
    Description,
    TransactionDate
)
SELECT
    NEWID(),
    CONCAT('klient_', (N % 850) + 1, '@',
        CASE (N % 4)
            WHEN 0 THEN 'gmail.com'
            WHEN 1 THEN 'outlook.com'
            WHEN 2 THEN 'wp.pl'
            ELSE 'onet.pl'
        END
    ),
    -- Losowa kwota od 10.00 PLN do 4999.99 PLN
    CAST(10.00 + ((N * 37) % 499000) / 100.0 AS DECIMAL(18,2)),
    'PLN',
    -- Metoda płatności
    CASE (N % 6)
        WHEN 0 THEN 'BLIK'
        WHEN 1 THEN 'Karta Visa'
        WHEN 2 THEN 'Karta Mastercard'
        WHEN 3 THEN 'Przelew online (PayU)'
        WHEN 4 THEN 'Apple Pay'
        ELSE 'Google Pay'
    END,
    -- Status (przewaga 'Zakończona')
    CASE (N % 5)
        WHEN 0 THEN 'Odrzucona'
        WHEN 1 THEN 'Oczekująca'
        ELSE 'Zakończona'
    END,
    CONCAT('Opłata za zamówienie #', 100000 + N),
    -- Daty rozłożone wstecz w czasie
    DATEADD(MINUTE, -N * 18, SYSUTCDATETIME())
FROM Tally;

-- 5. Weryfikacja
SELECT 
    COUNT(*) AS TotalTransactions,
    MIN(TransactionDate) AS OldestDate,
    MAX(TransactionDate) AS NewestDate,
    SUM(Amount) AS TotalVolumePLN
FROM dbo.SystemTransactions;