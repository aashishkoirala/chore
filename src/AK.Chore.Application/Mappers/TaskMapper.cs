/*******************************************************************************************************************************
 * AK.Chore.Application.Mappers.TaskMapper
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
using AK.Commons.DomainDriven;
using System;
using Recurrence = AK.Chore.Contracts.TaskAccess.Recurrence;
using Task = AK.Chore.Contracts.TaskAccess.Task;

#endregion

namespace AK.Chore.Application.Mappers
{
    /// <summary>
    /// Maps between Task/Recurrence data contracts and domain objects.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class TaskMapper
    {
        public static Task Map(this Domain.Tasks.Task task, DateTime now)
        {
            if (task == null) return null;

            return new Task
                {
                    Id = task.Id,
                    UserId = task.User.Id,
                    FolderId = task.Folder.Id,
                    FolderPath = task.Folder.FullPath,
                    Description = task.Description,
                    StartDate = task.StartDate,
                    StartTime = task.StartTime,
                    EndDate = task.EndDate,
                    EndTime = task.EndTime,
                    State = task.State.ToString(),
                    Recurrence = task.Recurrence.Map(),
                    IsMundane = task.IsMundane,
                    IsRecurring = task.IsRecurring,
                    IsLate = task.IsLate(now),
                    CanStart = task.CanStart,
                    CanPause = task.CanPause,
                    CanResume = task.CanResume,
                    CanComplete = task.CanComplete,
                    DateOrRecurrenceSummary = task.DateOrRecurrenceSummary
                };
        }

        public static Domain.Tasks.Task Map(
            this Task task, IEntityIdGenerator<int> idGenerator,
            Func<int, Domain.Tasks.Task> taskRetriever,
            Func<int, Domain.Folders.Folder> folderRetriever)
        {
            if (task == null) return null;

            if (task.Id > 0)
            {
                var mapped = taskRetriever(task.Id);
                if (mapped == null) return null;

                mapped.Description = task.Description;
                mapped.Recurrence = task.Recurrence.Map();
                mapped.IsMundane = task.IsMundane;
                mapped.SetDatesAndTimes(task.EndDate, task.EndTime, task.StartDate, task.StartTime);

                return mapped;
            }

            var folder = folderRetriever(task.FolderId);
            if (folder == null) return null;

            if (task.EndDate.HasValue && !task.StartDate.HasValue)
                return new Domain.Tasks.Task(idGenerator, task.Description, folder, task.EndDate.Value, task.EndTime);

            if (task.EndDate.HasValue && task.StartDate.HasValue)
                return new Domain.Tasks.Task(idGenerator, task.Description, folder, task.EndDate.Value,
                                             task.StartDate.Value, task.EndTime, task.StartTime);

            return task.IsRecurring
                       ? new Domain.Tasks.Task(idGenerator, task.Description, folder, task.Recurrence.Map())
                       : null;
        }

        public static Recurrence Map(this Domain.Tasks.Recurrence recurrence)
        {
            if (recurrence == null) return null;

            return new Recurrence
                {
                    Type = recurrence.Type.ToString(),
                    IsEnabled = recurrence.IsEnabled,
                    MonthOfYear = recurrence.MonthOfYear,
                    DayOfMonth = recurrence.DayOfMonth,
                    DaysOfWeek = recurrence.DaysOfWeek,
                    TimeOfDay = recurrence.TimeOfDay,
                    Duration = recurrence.Duration,
                    Interval = recurrence.Interval
                };
        }

        public static Domain.Tasks.Recurrence Map(this Recurrence recurrence)
        {
            if (recurrence == null) return null;

            var type = ParseEnum<RecurrenceType>(recurrence.Type);

            Domain.Tasks.Recurrence mapped = null;
            switch (type)
            {
                case RecurrenceType.NonRecurring:
                    mapped = Domain.Tasks.Recurrence.NonRecurring();
                    break;

                case RecurrenceType.Hourly:
                    mapped = Domain.Tasks.Recurrence.Hourly(recurrence.Interval, recurrence.Duration);
                    break;

                case RecurrenceType.Daily:
                    mapped = Domain.Tasks.Recurrence.Daily(recurrence.Interval, recurrence.TimeOfDay,
                                                           recurrence.Duration,
                                                           recurrence.DayOfMonth, recurrence.MonthOfYear);
                    break;

                case RecurrenceType.Weekly:
                    mapped = Domain.Tasks.Recurrence.Weekly(recurrence.Interval, recurrence.DaysOfWeek,
                                                            recurrence.TimeOfDay, recurrence.Duration,
                                                            recurrence.DayOfMonth, recurrence.MonthOfYear);
                    break;

                case RecurrenceType.Monthly:
                    mapped = Domain.Tasks.Recurrence.Monthly(recurrence.Interval, recurrence.DayOfMonth,
                                                             recurrence.TimeOfDay, recurrence.Duration,
                                                             recurrence.MonthOfYear);
                    break;

                case RecurrenceType.Yearly:
                    mapped = Domain.Tasks.Recurrence.Yearly(recurrence.Interval, recurrence.MonthOfYear,
                                                            recurrence.DayOfMonth, recurrence.TimeOfDay,
                                                            recurrence.Duration);
                    break;
            }
            if (mapped == null) return null;

            if (!recurrence.IsEnabled) mapped = Domain.Tasks.Recurrence.Disabled(mapped);

            return mapped;
        }

        private static TEnum ParseEnum<TEnum>(string value)
        {
            return (TEnum) Enum.Parse(typeof (TEnum), value);
        }
    }
}