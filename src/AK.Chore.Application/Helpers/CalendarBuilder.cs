/*******************************************************************************************************************************
 * AK.Chore.Application.Helpers.CalendarBuilder
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

using AK.Chore.Contracts.CalendarView;
using AK.Chore.Domain.Tasks;
using AK.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Task = AK.Chore.Domain.Tasks.Task;

#endregion

namespace AK.Chore.Application.Helpers
{
    /// <summary>
    /// Builds CalendarWeek data based on given tasks and recurrence map.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ICalendarBuilder
    {
        CalendarWeek BuildCalendarWeek(
            DateTime weekStart, DateTime weekEnd, IReadOnlyCollection<Task> tasks,
            IDictionary<int, DateTime[]> recurrenceMap);
    }

    [Export(typeof (ICalendarBuilder)), PartCreationPolicy(CreationPolicy.Shared)]
    public class CalendarBuilder : ICalendarBuilder
    {
        public CalendarWeek BuildCalendarWeek(
            DateTime weekStart, DateTime weekEnd, IReadOnlyCollection<Task> tasks,
            IDictionary<int, DateTime[]> recurrenceMap)
        {
            var calendarDays = new List<CalendarDay>();

            var date = weekStart;
            while (date <= weekEnd)
            {
                var calendarDay = BuildCalendarDay(date, tasks, recurrenceMap);
                calendarDays.Add(calendarDay);
                date = date.AddDays(1);
            }

            return new CalendarWeek {StartDate = weekStart, EndDate = weekEnd, Days = calendarDays.ToArray()};
        }

        private static CalendarDay BuildCalendarDay(
            DateTime day, IReadOnlyCollection<Task> tasks,
            IDictionary<int, DateTime[]> recurrenceMap)
        {
            return new CalendarDay
                {
                    Date = day,
                    DayOfWeek = day.DayOfWeek,
                    Hours =
                        Enumerable.Range(0, 24).Select(x => BuildCalendarHour(day, x, tasks, recurrenceMap)).ToArray(),
                    DayItems = BuildDayCalendarItems(day, tasks, recurrenceMap).ToArray()
                };
        }

        private static CalendarHour BuildCalendarHour(
            DateTime day, int hour,
            IReadOnlyCollection<Task> tasks, IDictionary<int, DateTime[]> recurrenceMap)
        {
            return new CalendarHour
                {
                    Hour = hour,
                    Items = BuildHourCalendarItems(day.AddHours(hour), tasks, recurrenceMap).ToArray()
                };
        }

        private static IEnumerable<CalendarItem> BuildDayCalendarItems(
            DateTime day, IReadOnlyCollection<Task> tasks, IDictionary<int, DateTime[]> recurrenceMap)
        {
            foreach (var task in tasks.Where(x => !x.IsRecurring))
            {
                if (task.EndDate == day && !task.EndTime.HasValue && !task.StartDate.HasValue)
                    yield return BuildCalendarItem(task, task.EndDate.Value, task.EndDate.Value);

                if (task.StartDate.HasValue && task.EndDate.HasValue && !task.EndTime.HasValue && task.StartDate >= day &&
                    task.EndDate <= day)
                    yield return BuildCalendarItem(task, task.StartDate.Value, task.EndDate.Value);
            }

            var recurringTasks = from task in tasks.Where(x => x.IsRecurring && x.Recurrence.IsEnabled &&
                                                               x.Recurrence.Type != RecurrenceType.Hourly &&
                                                               (int) x.Recurrence.TimeOfDay.TotalMilliseconds == 0)
                                 let dateTimes = recurrenceMap.LookFor(task.Id)
                                 where dateTimes.IsThere && dateTimes.ValueOrDefault.Contains(day)
                                 select task;

            foreach (var task in recurringTasks) yield return BuildCalendarItem(task, day, day);
        }

        private static IEnumerable<CalendarItem> BuildHourCalendarItems(
            DateTime hour, IReadOnlyCollection<Task> tasks, IDictionary<int, DateTime[]> recurrenceMap)
        {
            foreach (var task in tasks.Where(x => !x.IsRecurring))
            {
                if (task.EndDate.HasValue && task.EndTime.HasValue && task.StartDate.HasValue && task.StartTime.HasValue)
                {
                    var endTime = task.EndDate.Value.Add(task.EndTime.Value);
                    var startTime = task.StartDate.Value.Add(task.StartTime.Value);

                    if (hour >= startTime && hour < endTime)
                        yield return BuildCalendarItem(task, startTime, endTime);
                }

                else if (task.EndDate.HasValue && task.EndTime.HasValue)
                {
                    var nextHour = hour.AddHours(1);
                    var time = task.EndDate.Value.Add(task.EndTime.Value);

                    if (time >= hour && time <= nextHour)
                        yield return BuildCalendarItem(task, time, time);
                }
            }

            var recurringTasks = from task in tasks.Where(x => x.IsRecurring && x.Recurrence.IsEnabled &&
                                                               (x.Recurrence.Type == RecurrenceType.Hourly ||
                                                                (int) x.Recurrence.TimeOfDay.TotalMilliseconds > 0))
                                 let dateTimes = recurrenceMap.LookFor(task.Id)
                                 where dateTimes.IsThere && dateTimes.ValueOrDefault.Contains(hour)
                                 select task;

            foreach (var task in recurringTasks)
                yield return BuildCalendarItem(task, hour, hour.Add(task.Recurrence.Duration));
        }

        private static CalendarItem BuildCalendarItem(Task task, DateTime start, DateTime end)
        {
            return new CalendarItem
                {
                    TaskId = task.Id,
                    RootFolderId = GetRootFolderId(task),
                    Description = task.Description,
                    StartDateTime = start,
                    EndDateTime = end
                };
        }

        private static int GetRootFolderId(Task task)
        {
            var folder = task.Folder;
            var id = 0;
            while (folder != null)
            {
                id = folder.Id;
                folder = folder.Parent;
            }

            return id;
        }
    }
}