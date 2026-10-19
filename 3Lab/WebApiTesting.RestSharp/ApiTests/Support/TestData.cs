using System;
using System.Globalization;
using ApiTests.Models;

namespace ApiTests.Support
{
    public static class TestData
    {
        public const int MissingBookingId = 99999999;

        // Сервіс спільний і записи з такими іменами там уже є, тому до прізвища
        // додається час, щоб пошук за іменем знаходив саме цей запис.
        public static Booking ValidBooking()
        {
            DateTime checkIn = DateTime.Today.AddDays(7);

            return new Booking
            {
                FirstName = "Dmytro",
                LastName = "Test" + DateTime.Now.ToString("HHmmssfff", CultureInfo.InvariantCulture),
                TotalPrice = 1200,
                DepositPaid = true,
                BookingDates = new BookingDates { CheckIn = Iso(checkIn), CheckOut = Iso(checkIn.AddDays(6)) },
                AdditionalNeeds = "Breakfast"
            };
        }

        // Усі поля відмінні від ValidBooking, щоб заміна через PUT перевірялась на кожному.
        public static Booking ReplacementBooking()
        {
            DateTime checkIn = DateTime.Today.AddDays(30);

            return new Booking
            {
                FirstName = "Ivan",
                LastName = "Shevchenko",
                TotalPrice = 999,
                DepositPaid = false,
                BookingDates = new BookingDates { CheckIn = Iso(checkIn), CheckOut = Iso(checkIn.AddDays(3)) },
                AdditionalNeeds = "Dinner"
            };
        }

        private static string Iso(DateTime value)
        {
            return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
