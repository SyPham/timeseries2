using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Helpers
{
    /// <summary>
    ///     A class that contains extension method for the <see cref="DateTimeOffset" /> data type.
    /// </summary>
    public static class DateTimeOffsetExtensions
    {
        /// <summary>
        ///     Gets the end of the current month.
        /// </summary>
        /// <param name="date">The date to get the current month end from.</param>
        /// <returns>A new <see cref="DateTimeOffset" /> that represents the end of the current month.</returns>
        public static DateTimeOffset EndOfCurrentMonth(this DateTimeOffset date)
        {
            var lastDayInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            var newDate = new DateTime(date.Year, date.Month, lastDayInMonth, 23, 59, 59);
            var timeZoneOffset = newDate.GetTimeZoneOffset();
            return new DateTimeOffset(newDate, timeZoneOffset);
        }
    }
}
