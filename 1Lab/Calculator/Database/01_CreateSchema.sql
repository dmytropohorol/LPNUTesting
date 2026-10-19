-- Лаба 1, варіант 4: структура бази тестових даних для AnalaizerClass.Format()

IF DB_ID(N'CalcTestDb') IS NULL
    CREATE DATABASE [CalcTestDb];
GO

USE [CalcTestDb];
GO

/*---- Перестворення таблиць (порядок важливий через зовнішні ключі) ---------*/
IF OBJECT_ID(N'dbo.vFormatTestCase', N'V') IS NOT NULL DROP VIEW dbo.vFormatTestCase;
IF OBJECT_ID(N'dbo.FormatTestCase',  N'U') IS NOT NULL DROP TABLE dbo.FormatTestCase;
IF OBJECT_ID(N'dbo.ErrorCode',       N'U') IS NOT NULL DROP TABLE dbo.ErrorCode;
IF OBJECT_ID(N'dbo.TestCategory',    N'U') IS NOT NULL DROP TABLE dbo.TestCategory;
GO

-- Зберігається лише імʼя константи помилки, текст тест бере з ErrorLibrary.
CREATE TABLE dbo.ErrorCode
(
    ErrorCodeId     INT           NOT NULL,
    ErrorName       NVARCHAR(16)  NOT NULL,
    HasPosition     BIT           NOT NULL,
    MessageTemplate NVARCHAR(400) NOT NULL,
    CONSTRAINT PK_ErrorCode        PRIMARY KEY (ErrorCodeId),
    CONSTRAINT UQ_ErrorCode_Name   UNIQUE (ErrorName)
);
GO

-- Класи еквівалентності, кожен привʼязаний до гілки Format().
CREATE TABLE dbo.TestCategory
(
    TestCategoryId INT           NOT NULL,
    CategoryName   NVARCHAR(60)  NOT NULL,
    CoveredBranch  NVARCHAR(160) NOT NULL,
    Description    NVARCHAR(300) NOT NULL,
    CONSTRAINT PK_TestCategory      PRIMARY KEY (TestCategoryId),
    CONSTRAINT UQ_TestCategory_Name UNIQUE (CategoryName)
);
GO

-- Вхід = InputPattern x InputRepeat: випадки на 65536+ символів займають один рядок.
CREATE TABLE dbo.FormatTestCase
(
    TestCaseId            INT IDENTITY(1,1) NOT NULL,
    CaseCode              NVARCHAR(16)  NOT NULL,
    TestCategoryId        INT           NOT NULL,
    Description           NVARCHAR(300) NOT NULL,

    InputPattern          NVARCHAR(MAX) NOT NULL,
    InputRepeat           INT           NOT NULL
        CONSTRAINT DF_FormatTestCase_InputRepeat DEFAULT (1),

    ExpectedErrorCodeId   INT           NULL,
    ExpectedPosition      INT           NULL,
    ExpectedOutputPattern NVARCHAR(MAX) NULL,
    ExpectedOutputRepeat  INT           NOT NULL
        CONSTRAINT DF_FormatTestCase_OutputRepeat DEFAULT (1),

    IsActive              BIT           NOT NULL
        CONSTRAINT DF_FormatTestCase_IsActive DEFAULT (1),

    CONSTRAINT PK_FormatTestCase           PRIMARY KEY (TestCaseId),
    CONSTRAINT UQ_FormatTestCase_CaseCode  UNIQUE (CaseCode),
    CONSTRAINT FK_FormatTestCase_Category  FOREIGN KEY (TestCategoryId)
        REFERENCES dbo.TestCategory (TestCategoryId),
    CONSTRAINT FK_FormatTestCase_ErrorCode FOREIGN KEY (ExpectedErrorCodeId)
        REFERENCES dbo.ErrorCode (ErrorCodeId),

    CONSTRAINT CK_FormatTestCase_Repeat CHECK
        (InputRepeat >= 0 AND ExpectedOutputRepeat >= 0),

    -- або очікувана помилка, або очікуваний результат, не обидва
    CONSTRAINT CK_FormatTestCase_Outcome CHECK
    (
        (ExpectedErrorCodeId IS NOT NULL AND ExpectedOutputPattern IS NULL)
     OR (ExpectedErrorCodeId IS NULL     AND ExpectedOutputPattern IS NOT NULL)
    ),

    CONSTRAINT CK_FormatTestCase_Position CHECK
        (ExpectedPosition IS NULL OR ExpectedPosition >= 0)
);
GO

CREATE INDEX IX_FormatTestCase_Category ON dbo.FormatTestCase (TestCategoryId);
GO

CREATE VIEW dbo.vFormatTestCase
AS
SELECT  tc.TestCaseId,
        tc.CaseCode,
        cat.CategoryName,
        cat.CoveredBranch,
        tc.Description,
        tc.InputPattern,
        tc.InputRepeat,
        ec.ErrorName  AS ExpectedErrorName,
        tc.ExpectedPosition,
        tc.ExpectedOutputPattern,
        tc.ExpectedOutputRepeat
FROM    dbo.FormatTestCase AS tc
        INNER JOIN dbo.TestCategory AS cat ON cat.TestCategoryId = tc.TestCategoryId
        LEFT  JOIN dbo.ErrorCode    AS ec  ON ec.ErrorCodeId     = tc.ExpectedErrorCodeId
WHERE   tc.IsActive = 1;
GO
