using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverApp.Shared.Utils
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// Gets the same day last year counting leap years
        /// </summary>
        /// <returns>DateTime</returns>
        public static DateTime GetSameDayLastYear()
        {
            var today = DateTime.Today;
            var lastYear = today.Year - 1;

            int daysInMonthLastYear = DateTime.DaysInMonth(lastYear, today.Month);
            int day = Math.Min(today.Day, daysInMonthLastYear);

            return new DateTime(lastYear, today.Month, day);
        }

        /// <summary>
        /// Get today with time 23:59:59
        /// </summary>
        /// <returns>DateTime</returns>
        public static DateTime GetTodayLastTime()
        {
            return new(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 23, 59, 59);
        }

        /// <summary>
        /// Get current's month day 1
        /// </summary>
        /// <returns>DateTime</returns>
        public static DateTime GetFirstDayThisMonth()
        {
            return new(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        public static DateTime GetLastDayThisMonth()
        {
            int year = DateTime.Today.Year;
            int month = DateTime.Today.Month;

            return GetLastDayOfMonth(year, month);
        }

        public static DateTime GetLastDayOfMonth(int year, int month)
        {
            int daysInMonth = DateTime.DaysInMonth(year, month);
            return new DateTime(year, month, daysInMonth);
        }

        public static int TotalMinutes(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return 0;

            return (int)dateTime.Value.TimeOfDay.TotalMinutes;
        }

        public static int TotalMinutes(TimeOnly? time)
        {
            if (!time.HasValue)
                return 0;

            return (int)time.Value.Hour * 60 + time.Value.Minute;
        }
    }
}
