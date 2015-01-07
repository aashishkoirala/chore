/*******************************************************************************************************************************
 * AK.Chore.Application.Helpers.TaskExportFormatter
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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

#endregion

namespace AK.Chore.Application.Helpers
{
    /// <summary>
    /// Formats the given task as a string.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ITaskExportFormatter
    {
        /// <summary>
        /// Formats the given task as a string.
        /// </summary>
        /// <param name="task">Task to format.</param>
        /// <returns>Task formatted as string.</returns>
        string FormatTask(Domain.Tasks.Task task);
    }

    [Export(typeof (ITaskExportFormatter)), PartCreationPolicy(CreationPolicy.Shared)]
    public class TaskExportFormatter : ITaskExportFormatter
    {
        private static readonly string[] monthNames = new[]
            {
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"
            };

        public string FormatTask(Domain.Tasks.Task task)
        {
            var builder = new StringBuilder();

            builder.AppendFormat("{0}\t", task.Description);
            builder.AppendFormat("{0}\t", task.Folder.FullPath);
            builder.AppendFormat("{0}\t", GetDateString(task.EndDate, task.EndTime));
            builder.AppendFormat("{0}\t", GetDateString(task.StartDate, task.EndTime));
            builder.AppendFormat("{0}\t", task.IsRecurring ? "Yes" : "No");
            builder.AppendFormat("{0}\t", task.IsRecurring ? task.Recurrence.Type.ToString() : string.Empty);
            builder.AppendFormat("{0}\t", task.IsRecurring ? task.Recurrence.Interval.ToString() : string.Empty);
            builder.AppendFormat("{0}\t", task.IsRecurring ? task.Recurrence.Duration.ToString(@"hh\:mm") : string.Empty);
            builder.AppendFormat("{0}\t", task.IsRecurring
                                              ? task.Recurrence.TimeOfDay.ToString(@"hh\:mm")
                                              : string.Empty);
            builder.AppendFormat("{0}\t", task.IsRecurring ? task.Recurrence.DayOfMonth.ToString() : string.Empty);
            builder.AppendFormat("{0}\t", task.IsRecurring
                                              ? GetDaysOfWeekString(task.Recurrence.DaysOfWeek)
                                              : string.Empty);
            builder.AppendFormat("{0}\t", task.IsRecurring ? GetMonthString(task.Recurrence.MonthOfYear) : string.Empty);

            return builder.ToString();
        }

        private static string GetDateString(DateTime? date, TimeSpan? time)
        {
            if (!date.HasValue) return string.Empty;

            var builder = new StringBuilder();
            builder.AppendFormat("{0}", date.Value.ToString("yyyy-MM-dd"));

            if (time.HasValue) builder.AppendFormat("{0}", time.Value.ToString(@"hh\:mm"));

            return builder.ToString();
        }

        private static string GetDaysOfWeekString(IEnumerable<DayOfWeek> days)
        {
            if (days == null) return string.Empty;

            var dayNames = days.Select(x => x.ToString()).ToArray();
            return !dayNames.Any() ? string.Empty : dayNames.Aggregate((x1, x2) => x1 + "," + x2);
        }

        private static string GetMonthString(int month)
        {
            return month < 1 || month > 12 ? string.Empty : monthNames[month - 1];
        }
    }
}