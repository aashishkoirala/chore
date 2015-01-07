/*******************************************************************************************************************************
 * AK.Chore.Application.Helpers.CalendarRecurrenceCalculator
 * Copyright © 2014-2015 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of CHORE.
 *  
 * CHORE is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * CHORE is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with CHORE.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

#region Namespace Imports

using AK.Chore.Domain.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Task = AK.Chore.Domain.Tasks.Task;

#endregion

namespace AK.Chore.Application.Helpers
{
    /// <summary>
    /// Given a list of tasks and start/end dates, returns a map of each task ID and what date/times they recur on.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ICalendarRecurrenceCalculator
    {
        IDictionary<int, DateTime[]> GetCalendarTaskRecurrenceMap(
            IEnumerable<Task> tasks, DateTime weekStart, DateTime weekEnd);
    }

    [Export(typeof (ICalendarRecurrenceCalculator)), PartCreationPolicy(CreationPolicy.Shared)]
    public class CalendarRecurrenceCalculator : ICalendarRecurrenceCalculator
    {
        public IDictionary<int, DateTime[]> GetCalendarTaskRecurrenceMap(
            IEnumerable<Task> tasks, DateTime weekStart, DateTime weekEnd)
        {
            return tasks
                .Where(x => x.IsRecurring && x.Recurrence.IsEnabled)
                .ToDictionary(x => x.Id, x => GetTaskRecurrences(x.Recurrence, weekStart, weekEnd));
        }

        private static DateTime[] GetTaskRecurrences(Recurrence recurrence, DateTime weekStart, DateTime weekEnd)
        {
            var month = recurrence.MonthOfYear == 0 ? weekStart.Month : recurrence.MonthOfYear;
            var day = recurrence.DayOfMonth == 0 ? weekStart.Day : recurrence.DayOfMonth;

            var start = new DateTime(weekStart.Year, month, day);
            if (start < weekStart && recurrence.Type != RecurrenceType.Monthly &&
                recurrence.Type != RecurrenceType.Yearly) start = weekStart;

            var timeOfDay = TimeSpan.FromHours(recurrence.TimeOfDay.Hours);
            start = start.Add(timeOfDay);

            var duration = recurrence.Duration;
            duration = duration.Add(TimeSpan.FromMinutes(recurrence.TimeOfDay.Minutes));
            if (duration == TimeSpan.Zero) duration = TimeSpan.FromHours(1);

            return GetRecurrences(recurrence, start, weekEnd, duration).ToArray();
        }

        private static IEnumerable<DateTime> GetRecurrences(
            Recurrence recurrence, DateTime start, DateTime end, TimeSpan duration)
        {
            switch (recurrence.Type)
            {
                case RecurrenceType.Hourly:
                    return GetRecurrences(recurrence, start, end, duration, (x, y) => x.AddHours(y));
                case RecurrenceType.Daily:
                    return GetRecurrences(recurrence, start, end, duration, (x, y) => x.AddDays(y));
                case RecurrenceType.Weekly:
                    return GetWeeklyRecurrences(recurrence, start, end, duration);
                case RecurrenceType.Monthly:
                    return GetRecurrences(recurrence, start, end, duration, (x, y) => x.AddMonths(y));
                case RecurrenceType.Yearly:
                    return GetRecurrences(recurrence, start, end, duration, (x, y) => x.AddYears(y));
            }

            return Enumerable.Empty<DateTime>();
        }

        private static IEnumerable<DateTime> GetRecurrences(
            Recurrence recurrence, DateTime start, DateTime end, TimeSpan duration,
            Func<DateTime, int, DateTime> adder)
        {
            var time = start;
            while (time <= end)
            {
                var localStart = time;
                var localEnd = localStart.Add(duration);

                while (time < localEnd)
                {
                    yield return time;
                    time = time.AddHours(1);
                }

                time = adder(localStart, recurrence.Interval);
            }
        }

        private static IEnumerable<DateTime> GetWeeklyRecurrences(
            Recurrence recurrence, DateTime start, DateTime end, TimeSpan duration)
        {
            var time = start;
            while (time <= end)
            {
                var weekLocalStart = time;
                var weekLocalEnd = weekLocalStart.AddDays(6);

                while (time <= weekLocalEnd)
                {
                    var localStart = time;
                    var localEnd = localStart.Add(duration);

                    while (time < localEnd)
                    {
                        if (recurrence.DaysOfWeek.Contains(time.DayOfWeek)) yield return time;
                        time = time.AddHours(1);
                    }

                    time = localStart.AddDays(1);
                }

                time = weekLocalStart.AddDays(recurrence.Interval*7);
            }
        }
    }
}