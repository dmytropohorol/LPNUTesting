using System.Collections.Generic;
using RestSharp;

namespace ApiTests.Support
{
    /// <summary>
    /// Стан, який кроки одного сценарію передають один одному: остання
    /// відповідь, маркер доступу, номер створеного бронювання.
    /// </summary>
    /// <remarks>
    /// Екземпляр створює контейнер SpecFlow, по одному на сценарій. Статичних
    /// полів тут немає навмисно: вони зробили б сценарії залежними від порядку
    /// запуску і закрили б шлях до паралельного прогону.
    /// </remarks>
    public class ApiContext
    {
        /// <summary>Відповідь на останній надісланий запит.</summary>
        public RestResponse Response { get; set; }

        /// <summary>Маркер доступу, отриманий кроком авторизації.</summary>
        public string Token { get; set; }

        /// <summary>Номер бронювання, з яким працює сценарій.</summary>
        public int BookingId { get; set; }

        /// <summary>
        /// Бронювання, яке сценарій надіслав на сервер. Потрібне, щоб
        /// порівняти прочитане з тим, що записувалося.
        /// </summary>
        public Models.Booking SentBooking { get; set; }

        /// <summary>
        /// Номери бронювань, створених сценарієм. Хук прибирання видаляє їх
        /// після сценарію, щоб тести не лишали сміття на спільному сервісі.
        /// </summary>
        public List<int> CreatedBookingIds { get; } = new List<int>();
    }
}
