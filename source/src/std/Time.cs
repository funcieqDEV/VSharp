namespace VSharpLib
{
    using System;
    using System.Globalization;
    using VSharp;

    [Module]
    class Time
    {
        /// <summary>
        /// Returns the current date and time.
        /// </summary>
        public DateTime Now()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// Returns today's date without the time component.
        /// </summary>
        public DateTime Date()
        {
            return DateTime.Today;
        }

        /// <summary>
        /// Returns the current date and time with millisecond precision.
        /// </summary>
        public DateTime NowWithMilliseconds()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// Calculates the difference between two dates.
        /// </summary>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <returns>A TimeSpan representing the time difference between the two dates.</returns>
        public TimeSpan Difference(DateTime start, DateTime end)
        {
            return end - start;
        }

        /// <summary>
        /// Converts the given date and time to an ISO 8601 formatted string.
        /// </summary>
        /// <param name="dateTime">The date and time to convert.</param>
        /// <returns>A string representing the date and time in ISO 8601 format.</returns>
        public string ToIso8601(DateTime dateTime)
        {
            return dateTime.ToString("o", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses a date and time string in ISO 8601 format.
        /// </summary>
        /// <param name="iso8601String">The ISO 8601 formatted date string to parse.</param>
        /// <returns>The parsed DateTime object.</returns>
        public DateTime ParseIso8601(string iso8601String)
        {
            return DateTime.Parse(iso8601String, null, DateTimeStyles.RoundtripKind);
        }

        /// <summary>
        /// Adds the specified number of days to the given date.
        /// </summary>
        /// <param name="date">The starting date.</param>
        /// <param name="days">The number of days to add.</param>
        /// <returns>A DateTime representing the date plus the specified number of days.</returns>
        public DateTime AddDays(DateTime date, int days)
        {
            return date.AddDays(days);
        }

        /// <summary>
        /// Adds the specified number of hours to the given date.
        /// </summary>
        /// <param name="date">The starting date.</param>
        /// <param name="hours">The number of hours to add.</param>
        /// <returns>A DateTime representing the date plus the specified number of hours.</returns>
        public DateTime AddHours(DateTime date, int hours)
        {
            return date.AddHours(hours);
        }

        /// <summary>
        /// Adds the specified number of minutes to the given date.
        /// </summary>
        /// <param name="date">The starting date.</param>
        /// <param name="minutes">The number of minutes to add.</param>
        /// <returns>A DateTime representing the date plus the specified number of minutes.</returns>
        public DateTime AddMinutes(DateTime date, int minutes)
        {
            return date.AddMinutes(minutes);
        }
    }
}
