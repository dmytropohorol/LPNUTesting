# Лабораторна робота №3 — Тестування Web API з використанням RestSharp

**Курс:** Автоматизовані системи тестування програмного продукту

**Завдання 1:** операції типу CRUD сервісу
[restful-booker](https://restful-booker.herokuapp.com/)

**Завдання 2:** категорія **Weather** зі
[списку відкритих API](https://github.com/public-api-lists/public-api-lists),
сервіс [Open-Meteo](https://open-meteo.com/)

Автоматизовані тести Web API з використанням **RestSharp**, методології **BDD**
та інструмента **SpecFlow** зі сценаріями мовою **Gherkin**.

---

## Структура проєкту

```
WebApiTesting.RestSharp/
├── WebApiTesting.RestSharp.sln
├── Run-Tests.ps1                      запуск однією командою
├── Run-Tests.bat                      те саме, вікно не закривається
└── ApiTests/
    ├── Features/                       сценарії мовою Gherkin
    │   ├── BookingCrud.feature          завдання 1: операції CRUD
    │   ├── BookingAuth.feature          завдання 1: авторизація
    │   └── WeatherForecast.feature      завдання 2: прогноз погоди
    ├── StepDefinitions/                реалізація кроків
    │   ├── BookingCrudSteps.cs
    │   ├── BookingAuthSteps.cs
    │   ├── WeatherForecastSteps.cs
    │   └── CommonSteps.cs               кроки, спільні для обох служб
    ├── Clients/                        доступ до API
    │   ├── ApiClientBase.cs             налаштування RestSharp, журнал обміну
    │   ├── BookerClient.cs              restful-booker
    │   └── WeatherClient.cs             Open-Meteo
    ├── Models/                         тіла запитів і відповідей
    ├── Support/
    │   ├── ApiContext.cs                стан між кроками сценарію
    │   └── TestData.cs                  готові набори даних
    ├── Hooks/
    │   └── ApiHooks.cs                  прибирання даних, журнал при падінні
    └── specflow.json                   налаштування SpecFlow
```

Три шари не перетинаються: `.feature` описує поведінку природною мовою, кроки
перекладають її у дії предметної області, клієнти знають адреси ресурсів і
формат тіла. Роль клієнта тут та сама, що в Page Object для Web тестування:
зміна контракту API зачіпає один файл, а не кожен сценарій.

---

## Вимоги

* Visual Studio 2019 або новіша, .NET Framework 4.7.2 Developer Pack
* або .NET SDK для запуску з командного рядка
* доступ до мережі: обидві служби зовнішні

Розширення SpecFlow для Visual Studio **не потрібне**: пакет
`SpecFlow.Tools.MsBuild.Generation` генерує код кроків із `.feature` під час
збирання.

---

## Запуск

```bash
powershell -ExecutionPolicy Bypass -File Run-Tests.ps1
```

Окрема група сценаріїв:

```bash
powershell -ExecutionPolicy Bypass -File Run-Tests.ps1 -Filter "TestCategory=weather"
```

З Visual Studio: **Test → Run All Tests** (`Ctrl+R, A`).

### Очікуваний результат

```
Total tests: 29
     Passed: 29
```

---

## Сценарії

| Тег | Сценарій | Що перевіряє |
|---|---|---|
| `@smoke @crud` | Booking passes the full life cycle | створення, читання, заміна, видалення одним ланцюжком |
| `@crud` | Created booking appears in the list of all bookings | новий запис є у списку `GET /booking` |
| `@crud` | Created booking is found by the customer name | фільтр за іменем знаходить саме цей запис |
| `@crud` | Partial update changes only the given field | `PATCH` змінює суму і не зачіпає решти полів |
| `@crud @negative` | Reading a booking that does not exist returns Not Found | читання відсутнього ресурсу дає 404 |
| `@crud @negative` | Creating a booking without the required fields is rejected | неповне тіло запиту не проходить |
| `@crud @negative` | Writing to a booking that does not exist is rejected | заміна та видалення відсутнього ресурсу не проходять |
| `@auth` | Correct credentials return an access token | маркер доступу має 15 символів |
| `@auth @negative` | Wrong password does not return a token | маркера немає, причина `Bad credentials` |
| `@auth @negative` | Unknown user does not return a token | те саме для невідомого користувача |
| `@auth @negative` | Write operation without a token is forbidden | заміна, зміна та видалення без маркера дають 403, запис не змінюється |
| `@weather @smoke` | Current weather is returned for the given coordinates | є температура, одиниці та координати |
| `@weather` | Daily forecast returns as many days as requested | 1, 3 та 16 днів |
| `@weather` | Forecast with the automatic time zone returns the local zone | `timezone=auto` дає `Europe/Kyiv` |
| `@weather` | Temperature is returned in the requested unit | градуси Цельсія та Фаренгейта |
| `@weather` | Coordinates on the edge of the allowed range are accepted | широта 90 і -90, довгота 180 |
| `@weather @negative` | Coordinates outside the allowed range are rejected | широта 91 і -91, довгота 181 дають 400 |
| `@weather @negative` | Request without the longitude is rejected | неповний набір обов'язкових параметрів |
| `@weather @negative` | Forecast longer than the allowed range is rejected | 17 днів виходить за межу 16 |

Разом **29 тестів**: 14 для завдання 1 і 15 для завдання 2. Частину породжують
таблиці `Examples` у `Scenario Outline`.

У кожному сценарії з таблицею змінюється лише один параметр, решта умов стала.
Так падіння одразу вказує на причину, а набір не містить повторів тих самих
перевірок.

---

## Відхилення сервісу restful-booker від специфікації HTTP

Знайдені тестуванням. Сервіс навмисно містить дефекти: він створений як
майданчик для навчання тестуванню API.

| Запит | Код сервісу | Код за специфікацією |
|---|---|---|
| `GET /ping` | 201 Created | 200 OK, ресурс лише читається |
| `POST /auth` з неправильним паролем | 200 OK, причина в тілі | 401 Unauthorized |
| `POST /booking` без обов'язкових полів | 500 Internal Server Error | 400 Bad Request |
| `PUT` та `DELETE` без маркера | 403 Forbidden | 401 Unauthorized, бо даних для входу немає |
| `PUT` та `DELETE` відсутнього ресурсу | 405 Method Not Allowed | 404 Not Found |
| `DELETE` наявного ресурсу | 201 Created | 200 OK або 204 No Content |

Через це негативні сценарії перевіряють сам факт відмови, а не конкретний код
там, де код сервісу суперечить специфікації. Сценарії з кодом 403 і 404
перевіряють код точно, бо на цих шляхах сервіс сталий.

Для порівняння Open-Meteo дотримується специфікації: на будь-який некоректний
параметр відповідає кодом 400 і пояснює причину в полі `reason`.

---

## Особливості реалізації

**Маркер доступу у заголовку `Cookie`.** Сервіс restful-booker чекає маркер як
`Cookie: token=...`, а не у звичному для REST заголовку `Authorization`. Через
це вбудований аутентифікатор RestSharp не підходить і заголовок додається
вручну в `BookerClient`.

**Дати рядками, а не `DateTime`.** Сервіс використовує формат `yyyy-MM-dd`.
Якби модель мала `DateTime`, серіалізатор перевів би його у повну позначку
часу з часовим поясом, і результат порівняння залежав би від налаштувань
машини.

**Числа у рядку запиту з явною культурою.** За української культури
роздільником дробової частини є кома, і сервіс отримав би широту `49,8397`
замість `49.8397`. Тому `WeatherClient` форматує числа через
`CultureInfo.InvariantCulture`.

**Статус-код перевіряє тест, а не бібліотека.** Налаштування
`ThrowOnAnyError = false` лишає відповідь із кодом поза 2xx звичайним
результатом. Інакше негативні сценарії не змогли б перевірити ні код, ні тіло.

**Повтор лише безпечних запитів.** Обидві служби зовнішні та спільні, тому
поодинокий обрив зв'язку або код 5xx можливий без будь-якої помилки у тому,
що перевіряється. Запит надсилається до трьох разів, але лише там, де повтор
дає той самий результат: `GET`, `PUT`, `DELETE` і запит маркера. Створення
не повторюється, бо другий `POST /booking` створив би ще один запис. Коди 4xx
тимчасовими не вважаються: для негативних сценаріїв це очікуваний результат.

**Прибирання створених записів.** Хук `AfterScenario` видаляє бронювання, які
створив сценарій. Сервіс спільний для всіх, хто його використовує, тому
лишати по собі записи не варто.

**Журнал обміну при падінні.** У звіті прогону лишається лише назва кроку,
тому клієнт накопичує запити та відповіді, а хук друкує їх, коли сценарій
упав. Той самий журнал додається до повідомлення кожного `Assert`.

**Номер створеного запису запамʼятовується у кроці `When`.** Саме цей крок
створює ресурс, а кроки `Then` лише перевіряють результат і стану сценарію не
змінюють. Інакше сценарій без кроку перевірки створення лишився б без номера
і працював би з ресурсом за номером нуль.
