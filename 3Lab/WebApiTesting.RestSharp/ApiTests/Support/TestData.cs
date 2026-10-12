using System;
using System.Globalization;
using ApiTests.Models;

namespace ApiTests.Support
{
    /// <summary>Готові набори даних для сценаріїв.</summary>
    public static class TestData
    {
        /// <summary>
        /// Коректне бронювання з усіма полями. Дати беруться від поточної, тож
        /// набір не псується з часом, а до імені додається позначка часу, щоб
        /// фільтр за іменем знаходив саме цей запис: сервіс спільний, і записи
        /// з іменами з документації там уже є.
        /// </summary>
        public static Booking ValidBooking()
        {
            DateTime checkIn = DateTime.Today.AddDays(7);

            return new Booking
            {
                FirstName = "Dmytro",
                LastName = "Test" + DateTime.Now.ToString("HHmmssfff", CultureInfo.InvariantCulture),
                TotalPrice = 1200,
                DepositPaid = true,
                BookingDates = new BookingDates
                {
                    CheckIn = Iso(checkIn),
                    CheckOut = Iso(checkIn.AddDays(6))
                },
                AdditionalNeeds = "Breakfast"
            };
        }

        /// <summary>
        /// Інше коректне бронювання. Усі поля відмінні від ValidBooking, тому
        /// повна заміна через PUT перевіряється на кожному полі.
        /// </summary>
        public static Booking ReplacementBooking()
        {
            DateTime checkIn = DateTime.Today.AddDays(30);

            return new Booking
            {
                FirstName = "Ivan",
                LastName = "Shevchenko",
                TotalPrice = 999,
                DepositPaid = false,
                BookingDates = new BookingDates
                {
                    CheckIn = Iso(checkIn),
                    CheckOut = Iso(checkIn.AddDays(3))
                },
                AdditionalNeeds = "Dinner"
            };
        }

        /// <summary>
        /// Номер, якого на сервісі немає. Перевірка читання відсутнього
        /// ресурсу. Значення завелике, щоб його встигли зайняти.
        /// </summary>
        public const int MissingBookingId = 99999999;

        private static string Iso(DateTime value)
        {
            return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
