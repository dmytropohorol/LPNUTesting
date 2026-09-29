/*==============================================================================
  Лабораторна робота №1 — "Модульне тестування ПЗ"
  Варіант 4: бібліотека AnalaizerClassLibrary, метод Format()

  01_CreateSchema.sql — створення бази даних та структури таблиць,
  у яких зберігається набір даних для модульних тестів.

  Запуск: SSMS / sqlcmd / Database\Deploy-TestDatabase.ps1
==============================================================================*/

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

/*------------------------------------------------------------------------------
  dbo.ErrorCode — довідник кодів помилок зі специфікації (п. 2.2.3 SpecCalc).
  Зберігається лише ІМ'Я константи (ERROR_01 ... ERROR_09); повний текст
  повідомлення тест бере з ErrorLibrary.ErrorsExpression, щоб мати одне
  єдине джерело правди для тексту помилки.
------------------------------------------------------------------------------*/
CREATE TABLE dbo.ErrorCode
(
    ErrorCodeId     INT           NOT NULL,
    ErrorName       NVARCHAR(16)  NOT NULL,
    HasPosition     BIT           NOT NULL,   -- чи містить шаблон маркер <i>
    MessageTemplate NVARCHAR(400) NOT NULL,   -- довідково, як у специфікації
    CONSTRAINT PK_ErrorCode        PRIMARY KEY (ErrorCodeId),
    CONSTRAINT UQ_ErrorCode_Name   UNIQUE (ErrorName)
);
GO

/*------------------------------------------------------------------------------
  dbo.TestCategory — класи еквівалентності вхідних даних. Кожна категорія
  прив'язана до конкретної гілки методу Format(), яку вона покриває.
------------------------------------------------------------------------------*/
CREATE TABLE dbo.TestCategory
(
    TestCategoryId INT           NOT NULL,
    CategoryName   NVARCHAR(60)  NOT NULL,
    CoveredBranch  NVARCHAR(160) NOT NULL,   -- гілка Format(), що покривається
    Description    NVARCHAR(300) NOT NULL,
    CONSTRAINT PK_TestCategory      PRIMARY KEY (TestCategoryId),
    CONSTRAINT UQ_TestCategory_Name UNIQUE (CategoryName)
);
GO

/*------------------------------------------------------------------------------
  dbo.FormatTestCase — набір тестових даних.

  InputPattern + InputRepeat  задають вхідний вираз як повторення фрагмента.
  Це дозволяє зберігати граничні випадки довжини (65536 / 65537 символів)
  без роздування таблиці: InputPattern = N'1', InputRepeat = 65537.

  Очікуваний результат задається ВЗАЄМОВИКЛЮЧНО:
    * ExpectedErrorCodeId (+ ExpectedPosition)  — очікується повідомлення
      про помилку, тобто рядок, що починається зі спец. символу '&';
    * ExpectedOutputPattern (+ ExpectedOutputRepeat) — очікується
      відформатований вираз.
------------------------------------------------------------------------------*/
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

    -- або помилка, або результат — але не обидва одночасно
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

/*------------------------------------------------------------------------------
  dbo.vFormatTestCase — представлення, з якого читає модульний тест.
------------------------------------------------------------------------------*/
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
