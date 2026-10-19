-- Тестові випадки для AnalaizerClass.Format()

USE [CalcTestDb];
GO

SET NOCOUNT ON;
GO

/*---- Категорія 1: Empty ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-01-01', 1, N'Порожній рядок повертається без змін',
     N'', 1, NULL, NULL, N'', 1, 1),
    (N'TC-01-02', 1, N'Рядок з одного пробілу зводиться до порожнього',
     N' ', 1, NULL, NULL, N'', 1, 1),
    (N'TC-01-03', 1, N'Рядок лише з пробілів зводиться до порожнього',
     N'     ', 1, NULL, NULL, N'', 1, 1);
GO

/*---- Категорія 2: SpaceRemoval ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-02-01', 2, N'Пробіли навколо операндів та оператора видаляються',
     N' 2 + 3 ', 1, NULL, NULL, N'2+3', 1, 1),
    (N'TC-02-02', 2, N'Пробіл усередині числа видаляється, утворюючи одне число 12',
     N'1 2 + 3', 1, NULL, NULL, N'12+3', 1, 1),
    (N'TC-02-03', 2, N'Пробіли у виразі з дужками видаляються',
     N'( 2 + 3 ) * 4', 1, NULL, NULL, N'(2+3)*4', 1, 1),
    (N'TC-02-04', 2, N'Пробіл після унарного мінуса видаляється',
     N'm 5', 1, NULL, NULL, N'm5', 1, 1),
    (N'TC-02-05', 2, N'Кілька послідовних пробілів видаляються',
     N'2   +   3', 1, NULL, NULL, N'2+3', 1, 1);
GO

/*---- Категорія 3: ValidExpression ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-03-01', 3, N'Нуль як єдиний операнд',
     N'0', 1, NULL, NULL, N'0', 1, 1),
    (N'TC-03-02', 3, N'Багатоцифрове число без операторів',
     N'123', 1, NULL, NULL, N'123', 1, 1),
    (N'TC-03-03', 3, N'Додавання',
     N'2+3', 1, NULL, NULL, N'2+3', 1, 1),
    (N'TC-03-04', 3, N'Віднімання',
     N'9-4', 1, NULL, NULL, N'9-4', 1, 1),
    (N'TC-03-05', 3, N'Множення',
     N'6*7', 1, NULL, NULL, N'6*7', 1, 1),
    (N'TC-03-06', 3, N'Ділення',
     N'8/2', 1, NULL, NULL, N'8/2', 1, 1),
    (N'TC-03-07', 3, N'Залишок від ділення',
     N'9%4', 1, NULL, NULL, N'9%4', 1, 1),
    (N'TC-03-08', 3, N'Унарний мінус перед числом',
     N'm5', 1, NULL, NULL, N'm5', 1, 1),
    (N'TC-03-09', 3, N'Унарний плюс перед числом',
     N'p5', 1, NULL, NULL, N'p5', 1, 1),
    (N'TC-03-10', 3, N'Дужки з подальшим множенням',
     N'(2+3)*4', 1, NULL, NULL, N'(2+3)*4', 1, 1),
    (N'TC-03-11', 3, N'Вкладені дужки та кілька операцій',
     N'((2+3)*(4-1))%5', 1, NULL, NULL, N'((2+3)*(4-1))%5', 1, 1),
    (N'TC-03-12', 3, N'Унарний мінус перед дужкою',
     N'm(2+3)', 1, NULL, NULL, N'm(2+3)', 1, 1),
    (N'TC-03-13', 3, N'Унарний мінус після бінарного оператора',
     N'2+m3', 1, NULL, NULL, N'2+m3', 1, 1),
    (N'TC-03-14', 3, N'Унарний плюс після бінарного оператора',
     N'2*p3', 1, NULL, NULL, N'2*p3', 1, 1),
    (N'TC-03-15', 3, N'Подвійні дужки навколо числа',
     N'((1))', 1, NULL, NULL, N'((1))', 1, 1),
    (N'TC-03-16', 3, N'Дві дужкові групи, сполучені оператором',
     N'(2+3)*(4-1)', 1, NULL, NULL, N'(2+3)*(4-1)', 1, 1),
    (N'TC-03-17', 3, N'Гранична глибина вкладеності дужок — 3',
     N'(((1+2)))', 1, NULL, NULL, N'(((1+2)))', 1, 1),
    (N'TC-03-18', 3, N'Глибина вкладеності 4: Format() глибину не перевіряє (це робота CheckCurrency)',
     N'((((1+2))))', 1, NULL, NULL, N'((((1+2))))', 1, 1),
    (N'TC-03-19', 3, N'Незакрита дужка: Format() баланс дужок не перевіряє',
     N'(2+3', 1, NULL, NULL, N'(2+3', 1, 1),
    (N'TC-03-20', 3, N'Зайва закриваюча дужка: Format() баланс дужок не перевіряє',
     N'2+3)', 1, NULL, NULL, N'2+3)', 1, 1),
    (N'TC-03-21', 3, N'MAXINT + 1: Format() діапазон чисел не перевіряє (це робота CalcClass)',
     N'2147483647+1', 1, NULL, NULL, N'2147483647+1', 1, 1),
    (N'TC-03-22', 3, N'Ділення на нуль: Format() обчислень не виконує',
     N'0%0', 1, NULL, NULL, N'0%0', 1, 1),
    (N'TC-03-23', 3, N'Понад 30 операндів: Format() їх кількість не перевіряє (це робота RunEstimate)',
     N'1+2+3+4+5+6+7+8+9+10+11+12+13+14+15+16+17', 1, NULL, NULL, N'1+2+3+4+5+6+7+8+9+10+11+12+13+14+15+16+17', 1, 1),
    (N'TC-03-24', 3, N'Унарний мінус перед нулем',
     N'm0', 1, NULL, NULL, N'm0', 1, 1),
    (N'TC-03-25', 3, N'Унарний мінус усередині дужок',
     N'(m5)', 1, NULL, NULL, N'(m5)', 1, 1),
    (N'TC-03-26', 3, N'Ланцюжок дужкових груп з різними операціями',
     N'(2+3)*(4-1)%(5+6)', 1, NULL, NULL, N'(2+3)*(4-1)%(5+6)', 1, 1),
    (N'TC-03-27', 3, N'Закриваюча дужка перед закриваючою дужкою',
     N'((2+3))', 1, NULL, NULL, N'((2+3))', 1, 1);
GO

/*---- Категорія 4: MaxLength ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-04-01', 4, N'Рівно 65536 символів — межа НЕ перевищена, вираз коректний',
     N'1', 65536, NULL, NULL, N'1', 65536, 1),
    (N'TC-04-02', 4, N'65537 символів — перевищення максимальної довжини',
     N'1', 65537, 7, NULL, NULL, 1, 1),
    (N'TC-04-03', 4, N'Пробіли видаляються ДО перевірки довжини: 131072 символи -> 65536',
     N'1 ', 65536, NULL, NULL, N'1', 65536, 1),
    (N'TC-04-04', 4, N'Пробіли видаляються ДО перевірки довжини: після видалення лишається 65537',
     N'1 ', 65537, 7, NULL, NULL, 1, 1);
GO

/*---- Категорія 5: UnknownSymbol ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-05-01', 5, N'Латинська літера на початку виразу',
     N'a', 1, 2, 0, NULL, 1, 1),
    (N'TC-05-02', 5, N'Спецсимвол ''&'' усередині виразу',
     N'2&3', 1, 2, 1, NULL, 1, 1),
    (N'TC-05-03', 5, N'Десяткова крапка не підтримується',
     N'2+3.', 1, 2, 3, NULL, 1, 1),
    (N'TC-05-04', 5, N'Оператор піднесення до степеня не підтримується',
     N'2^3', 1, 2, 1, NULL, 1, 1),
    (N'TC-05-05', 5, N'Квадратні дужки заборонені (вимога 4.3.6)',
     N'[2+3]', 1, 2, 0, NULL, 1, 1),
    (N'TC-05-06', 5, N'Фігурні дужки заборонені (вимога 4.3.6)',
     N'{2+3}', 1, 2, 0, NULL, 1, 1),
    (N'TC-05-07', 5, N'Символ ''='' у виразі',
     N'2+3=', 1, 2, 3, NULL, 1, 1),
    (N'TC-05-08', 5, N'Велика літера ''M'' не є унарним мінусом (унарний мінус — ''m'')',
     N'M5', 1, 2, 0, NULL, 1, 1),
    (N'TC-05-09', 5, N'Велика літера ''P'' не є унарним плюсом (унарний плюс — ''p'')',
     N'P5', 1, 2, 0, NULL, 1, 1),
    (N'TC-05-10', 5, N'Ключове слово mod не розпізнається, очікується ''%''',
     N'2 mod 3', 1, 2, 2, NULL, 1, 1),
    (N'TC-05-11', 5, N'Кома як роздільник дробової частини',
     N'2,5+1', 1, 2, 1, NULL, 1, 1),
    (N'TC-05-12', 5, N'Табуляція не видаляється (видаляються лише пробіли)',
     N'2' + NCHAR(9) + N'+3', 1, 2, 1, NULL, 1, 1),
    (N'TC-05-13', 5, N'Кириличний символ у виразі',
     N'2+х', 1, 2, 2, NULL, 1, 1),
    (N'TC-05-14', 5, N'Символ ''#'' на початку виразу',
     N'#5', 1, 2, 0, NULL, 1, 1);
GO

/*---- Категорія 6: InvalidStart ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-06-01', 6, N'Вираз починається з ''+''',
     N'+3', 1, 3, NULL, NULL, 1, 1),
    (N'TC-06-02', 6, N'Вираз починається з ''-'' (унарний мінус позначається ''m'')',
     N'-3', 1, 3, NULL, NULL, 1, 1),
    (N'TC-06-03', 6, N'Вираз починається з ''*''',
     N'*3', 1, 3, NULL, NULL, 1, 1),
    (N'TC-06-04', 6, N'Вираз починається з ''/''',
     N'/3', 1, 3, NULL, NULL, 1, 1),
    (N'TC-06-05', 6, N'Вираз починається з ''%''',
     N'%3', 1, 3, NULL, NULL, 1, 1),
    (N'TC-06-06', 6, N'Вираз починається із закриваючої дужки',
     N')3+1(', 1, 3, NULL, NULL, 1, 1),
    (N'TC-06-07', 6, N'Вираз складається з однієї закриваючої дужки',
     N')', 1, 3, NULL, NULL, 1, 1);
GO

/*---- Категорія 7: InvalidEnd ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-07-01', 7, N'Вираз закінчується на ''+''',
     N'3+', 1, 5, NULL, NULL, 1, 1),
    (N'TC-07-02', 7, N'Вираз закінчується на ''-''',
     N'3-', 1, 5, NULL, NULL, 1, 1),
    (N'TC-07-03', 7, N'Вираз закінчується на ''*''',
     N'3*', 1, 5, NULL, NULL, 1, 1),
    (N'TC-07-04', 7, N'Вираз закінчується на ''/''',
     N'3/', 1, 5, NULL, NULL, 1, 1),
    (N'TC-07-05', 7, N'Вираз закінчується на ''%''',
     N'3%', 1, 5, NULL, NULL, 1, 1),
    (N'TC-07-06', 7, N'Вираз закінчується відкриваючою дужкою',
     N'(3+4(', 1, 5, NULL, NULL, 1, 1),
    (N'TC-07-07', 7, N'Вираз закінчується унарним мінусом',
     N'3m', 1, 5, NULL, NULL, 1, 1),
    (N'TC-07-08', 7, N'Вираз закінчується унарним плюсом',
     N'3p', 1, 5, NULL, NULL, 1, 1),
    (N'TC-07-09', 7, N'Вираз складається лише з унарного мінуса',
     N'm', 1, 5, NULL, NULL, 1, 1),
    (N'TC-07-10', 7, N'Вираз складається лише з унарного плюса',
     N'p', 1, 5, NULL, NULL, 1, 1),
    (N'TC-07-11', 7, N'Вираз складається лише з відкриваючої дужки',
     N'(', 1, 5, NULL, NULL, 1, 1);
GO

/*---- Категорія 8: DigitFollowedByBracketOrUnary ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-08-01', 8, N'Неявне множення ''3(4)'' заборонене',
     N'3(4)', 1, 1, 1, NULL, 1, 1),
    (N'TC-08-02', 8, N'Відкриваюча дужка після цифри всередині виразу',
     N'2+3(4)', 1, 1, 3, NULL, 1, 1),
    (N'TC-08-03', 8, N'Унарний мінус одразу після цифри',
     N'3m4', 1, 1, 1, NULL, 1, 1),
    (N'TC-08-04', 8, N'Унарний плюс одразу після цифри',
     N'3p4', 1, 1, 1, NULL, 1, 1),
    (N'TC-08-05', 8, N'Дужка після другої цифри багатоцифрового числа',
     N'12(3)', 1, 1, 2, NULL, 1, 1),
    (N'TC-08-06', 8, N'Дужка після цифри у виразі з дужковою групою',
     N'(2)+3(4)', 1, 1, 5, NULL, 1, 1),
    (N'TC-08-07', 8, N'Незбалансована дужка після цифри',
     N'3(4', 1, 1, 1, NULL, 1, 1);
GO

/*---- Категорія 9: TwoOperatorsInRow ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-09-01', 9, N'Комбінація ''+*''',
     N'3+*3', 1, 4, 2, NULL, 1, 1),
    (N'TC-09-02', 9, N'Приклад зі специфікації 3+*+3',
     N'3+*+3', 1, 4, 2, NULL, 1, 1),
    (N'TC-09-03', 9, N'Подвійне множення',
     N'3**3', 1, 4, 2, NULL, 1, 1),
    (N'TC-09-04', 9, N'Комбінація ''+-''',
     N'3+-3', 1, 4, 2, NULL, 1, 1),
    (N'TC-09-05', 9, N'Комбінація ''*/'' після дужкової групи',
     N'(3+4)*/2', 1, 4, 6, NULL, 1, 1),
    (N'TC-09-06', 9, N'Комбінація ''/%''',
     N'3/%4', 1, 4, 2, NULL, 1, 1),
    (N'TC-09-07', 9, N'Комбінація ''%-''',
     N'3%-4', 1, 4, 2, NULL, 1, 1);
GO

/*---- Категорія 10: OperatorFollowedByInvalid ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-10-01', 10, N'Закриваюча дужка одразу після оператора',
     N'(3+)', 1, 3, NULL, NULL, 1, 1),
    (N'TC-10-02', 10, N'Закриваюча дужка після множення',
     N'(3*)+1', 1, 3, NULL, NULL, 1, 1),
    (N'TC-10-03', 10, N'Закриваюча дужка після віднімання',
     N'(2+3-)', 1, 3, NULL, NULL, 1, 1);
GO

/*---- Категорія 11: OpenBracketFollowedByInvalid ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-11-01', 11, N'Порожня пара дужок',
     N'()', 1, 3, NULL, NULL, 1, 1),
    (N'TC-11-02', 11, N'Бінарний ''+'' одразу після відкриваючої дужки',
     N'(+3)', 1, 3, NULL, NULL, 1, 1),
    (N'TC-11-03', 11, N'Бінарний ''*'' одразу після відкриваючої дужки',
     N'(*3)', 1, 3, NULL, NULL, 1, 1),
    (N'TC-11-04', 11, N'Порожня пара дужок усередині виразу',
     N'2+()', 1, 3, NULL, NULL, 1, 1),
    (N'TC-11-05', 11, N'Бінарний ''/'' одразу після відкриваючої дужки',
     N'(/2)', 1, 3, NULL, NULL, 1, 1),
    (N'TC-11-06', 11, N'Бінарний ''-'' одразу після дужки (унарний мінус позначається ''m'')',
     N'(-3)', 1, 3, NULL, NULL, 1, 1);
GO

/*---- Категорія 12: CloseBracketFollowedByInvalid ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-12-01', 12, N'Цифра одразу після закриваючої дужки',
     N'(3)4', 1, 3, NULL, NULL, 1, 1),
    (N'TC-12-02', 12, N'Відкриваюча дужка одразу після закриваючої',
     N'(3)(4)', 1, 3, NULL, NULL, 1, 1),
    (N'TC-12-03', 12, N'Унарний мінус одразу після закриваючої дужки',
     N'(3)m4', 1, 3, NULL, NULL, 1, 1),
    (N'TC-12-04', 12, N'Унарний плюс одразу після закриваючої дужки',
     N'(3)p4', 1, 3, NULL, NULL, 1, 1),
    (N'TC-12-05', 12, N'Число одразу після дужкової групи',
     N'(2+3)5', 1, 3, NULL, NULL, 1, 1);
GO

/*---- Категорія 13: UnaryFollowedByInvalid ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-13-01', 13, N'Закриваюча дужка після унарного мінуса',
     N'(m)', 1, 3, NULL, NULL, 1, 1),
    (N'TC-13-02', 13, N'Закриваюча дужка після унарного плюса',
     N'(p)+1', 1, 3, NULL, NULL, 1, 1),
    (N'TC-13-03', 13, N'Два унарні мінуси підряд',
     N'mm3', 1, 3, NULL, NULL, 1, 1),
    (N'TC-13-04', 13, N'Унарний мінус, за ним унарний плюс',
     N'mp3', 1, 3, NULL, NULL, 1, 1),
    (N'TC-13-05', 13, N'Унарний плюс, за ним унарний мінус',
     N'pm3', 1, 3, NULL, NULL, 1, 1),
    (N'TC-13-06', 13, N'Два унарні плюси підряд',
     N'pp3', 1, 3, NULL, NULL, 1, 1),
    (N'TC-13-07', 13, N'Бінарний ''+'' після унарного мінуса',
     N'm+3', 1, 3, NULL, NULL, 1, 1),
    (N'TC-13-08', 13, N'Бінарний ''*'' після унарного мінуса',
     N'm*3', 1, 3, NULL, NULL, 1, 1),
    (N'TC-13-09', 13, N'Бінарний ''/'' після унарного плюса',
     N'p/3', 1, 3, NULL, NULL, 1, 1),
    (N'TC-13-10', 13, N'Бінарний оператор після унарного всередині виразу',
     N'2+m*3', 1, 3, NULL, NULL, 1, 1);
GO

/*---- Категорія 14: CheckPrecedence ----*/
INSERT INTO dbo.FormatTestCase
    (CaseCode, TestCategoryId, Description, InputPattern, InputRepeat,
     ExpectedErrorCodeId, ExpectedPosition,
     ExpectedOutputPattern, ExpectedOutputRepeat, IsActive)
VALUES
    (N'TC-14-01', 14, N'Невідомий символ виявляється раніше за невірний початок виразу',
     N'+3&', 1, 2, 2, NULL, 1, 1),
    (N'TC-14-02', 14, N'Невірний початок виразу виявляється раніше за невірне закінчення',
     N'+3+', 1, 3, NULL, NULL, 1, 1),
    (N'TC-14-03', 14, N'Невірне закінчення виявляється раніше за два оператори підряд',
     N'3++', 1, 5, NULL, NULL, 1, 1),
    (N'TC-14-04', 14, N'Невідомий символ виявляється раніше за помилку ''цифра + дужка''',
     N'3(4&', 1, 2, 3, NULL, 1, 1),
    (N'TC-14-05', 14, N'Невірне закінчення виявляється раніше за помилку після '')''',
     N'(3)4+', 1, 5, NULL, NULL, 1, 1),
    (N'TC-14-06', 14, N'Перша за позицією помилка в циклі: ''цифра+m'' раніше за ''+*''',
     N'3m4+*5', 1, 1, 1, NULL, 1, 1);
GO
