# Лабораторна робота №2 — Web тестування з використанням Selenium

**Курс:** Автоматизовані системи тестування програмного продукту
**Варіант 4:** сайт [XYZ Bank](https://www.globalsqa.com/angularJs-protractor/BankingProject/),
вхід **Bank Manager Login**, операція **Open Account**

Автоматизовані Web тести з використанням **Selenium WebDriver**,
**Page Objects Pattern**, методології **BDD** та інструмента **SpecFlow**
зі сценаріями мовою **Gherkin**.

---

## Структура проєкту

```
BankingProject.Selenium/
├── BankingProject.Selenium.sln
├── Run-Tests.ps1                       запуск однією командою
└── BankingTests/
    ├── Features/
    │   └── OpenAccount.feature          сценарії мовою Gherkin
    ├── StepDefinitions/
    │   └── OpenAccountSteps.cs          реалізація кроків
    ├── Pages/                           Page Objects Pattern
    │   ├── BasePage.cs                  спільні дії з елементами
    │   ├── LoginPage.cs                 вибір ролі на стартовій сторінці
    │   ├── ManagerPage.cs               вкладки менеджера банку
    │   └── OpenAccountPage.cs           форма відкриття рахунку
    ├── Drivers/
    │   └── BrowserDriver.cs             життєвий цикл браузера
    ├── Hooks/
    │   └── ScenarioHooks.cs             знімок екрана при падінні, закриття браузера
    └── specflow.json                    налаштування SpecFlow
```

Три шари не перетинаються: `.feature` описує поведінку природною мовою,
кроки перекладають її у дії предметної області, сторінки знають про
селектори. Селектор змінюється лише в одному місці.

---

## Вимоги

* Visual Studio 2019 або новіша, .NET Framework 4.7.2 Developer Pack
* Google Chrome (перевірено на версії 154)

`chromedriver.exe` завантажувати **не потрібно**: Selenium 4.6 і новіші
мають вбудований Selenium Manager, який сам підбирає драйвер під
встановлену версію браузера.

---

## Запуск

```bash
powershell -ExecutionPolicy Bypass -File Run-Tests.ps1
```

Без вікна браузера:

```bash
powershell -ExecutionPolicy Bypass -File Run-Tests.ps1 -Headless
```

Окрема група сценаріїв:

```bash
powershell -ExecutionPolicy Bypass -File Run-Tests.ps1 -Filter "TestCategory=smoke"
```

З Visual Studio: **Test → Run All Tests** (`Ctrl+R, A`).

### Очікуваний результат

```
Total tests: 9
     Passed: 9
```

---

## Сценарії

| Тег | Сценарій | Що перевіряє |
|---|---|---|
| `@smoke` | Account is opened for an existing customer | основний шлях: рахунок створюється, номер не порожній |
| `@regression` | Account is opened in every supported currency | 5 клієнтів × валюти через `Scenario Outline` |
| `@negative` | Account is not opened when customer is not selected | без клієнта рахунок не створюється |
| `@negative` | Account is not opened when currency is not selected | без валюти рахунок не створюється |
| `@ui` | Customer list contains all bank customers | у списку всі п'ять клієнтів банку |

Разом 9 тестів: 5 з них породжує `Scenario Outline` з таблиці `Examples`.

---

## Особливості реалізації

**Селектори за `ng-click`.** Застосунок побудований на AngularJS, тому
кнопки адресуємо за атрибутом `ng-click`, наприклад
`button[ng-click='manager()']`. Це стабільніше за текст кнопки та за класи
оформлення Bootstrap.

**Кнопка Process шукається всередині форми.** У шапці сторінки є інші
кнопки з `type='submit'`, тому локатор звужено до
`form[name='myForm'] button[type='submit']`.

**Результат приходить через `alert`.** Застосунок повідомляє про
створення рахунку діалогом браузера з текстом
`Account created successfully with account Number :1016`. Номер рахунку
виймається з цього тексту і перевіряється окремо.

**Тільки явні очікування.** Неявне очікування вимкнене
(`ImplicitWait = TimeSpan.Zero`): змішувати його з явним не варто, бо час
очікування стає непередбачуваним.

**Один браузер на сценарій.** Екземпляр `BrowserDriver` створює контейнер
SpecFlow, вікно відкривається лише при першому зверненні, а хук
`AfterScenario` його закриває. Якщо сценарій упав, перед закриттям
зберігається знімок екрана у теці `Screenshots`.
