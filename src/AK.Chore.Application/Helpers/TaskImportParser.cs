/*******************************************************************************************************************************
 * AK.Chore.Application.Helpers.TaskImportParser
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

using AK.Chore.Contracts.TaskImportExport;
using AK.Chore.Domain.Tasks;
using AK.Commons;
using AK.Commons.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Recurrence = AK.Chore.Contracts.TaskAccess.Recurrence;
using Task = AK.Chore.Contracts.TaskAccess.Task;

#endregion

namespace AK.Chore.Application.Helpers
{
    /// <summary>
    /// Parses formatted text lines as tasks.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ITaskImportParser
    {
        /// <summary>
        /// Parses formatted text lines as tasks.
        /// </summary>
        /// <param name="lines">Lines to parse.</param>
        /// <returns>Results with parsed tasks.</returns>
        OperationResults<Task> ParseLines(string[] lines);
    }

    [Export(typeof (ITaskImportParser)), PartCreationPolicy(CreationPolicy.Shared)]
    public class TaskImportParser : ITaskImportParser
    {
        public OperationResults<Task> ParseLines(string[] lines)
        {
            if (lines.Length < 2)
            {
                var result = new OperationResult<Task>(TaskImportExportResult.NoTasksToImport);
                return new OperationResults<Task>(new[] {result});
            }

            var headerLine = lines[0];
            lines = lines.Skip(1).ToArray();

            var fieldIndexMap = GetFieldIndexMap(headerLine);
            var results = lines.Select(x => ConvertLineToTask(x, fieldIndexMap)).ToArray();

            return new OperationResults<Task>(results);
        }

        private static OperationResult<Task> ConvertLineToTask(string line, IDictionary<string, int> fieldIndexMap)
        {
            var fields = line.Split('\t')
                             .Select(x => x.Trim())
                             .ToArray();

            var folderPath = GetLineField(fields, "Folder", fieldIndexMap);
            var description = GetLineField(fields, "Description", fieldIndexMap);

            if (String.IsNullOrWhiteSpace(folderPath) || String.IsNullOrWhiteSpace(description))
                return new OperationResult<Task>(TaskImportExportResult.CannotImportTask, line);

            var isRecurringField = GetLineField(fields, "Recurring", fieldIndexMap);
            var isRecurring = !String.IsNullOrWhiteSpace(isRecurringField) && isRecurringField.ToLower() == "yes";

            var task = new Task
                {
                    FolderPath = folderPath,
                    Description = description,
                    IsRecurring = isRecurring
                };

            if (!isRecurring)
            {
                task.Recurrence = new Recurrence {Type = RecurrenceType.NonRecurring.ToString()};

                var perhapsEndDate = GetLineField(fields, "Finish By", fieldIndexMap).ParseDateTime();
                if (!perhapsEndDate.IsThere)
                    return new OperationResult<Task>(TaskImportExportResult.CannotImportTask, line);

                task.EndDate = perhapsEndDate.Value.Date;
                task.EndTime = perhapsEndDate.Value.TimeOfDay.TotalMilliseconds > 0
                                   ? perhapsEndDate.Value.TimeOfDay
                                   : (TimeSpan?) null;

                var startDateField = GetLineField(fields, "Start By", fieldIndexMap);
                if (!String.IsNullOrWhiteSpace(startDateField))
                {
                    var perhapsStartDate = startDateField.ParseDateTime();
                    if (!perhapsStartDate.IsThere)
                        return new OperationResult<Task>(TaskImportExportResult.CannotImportTask, line);

                    task.StartDate = perhapsStartDate.Value.Date;
                    task.StartTime = perhapsStartDate.Value.TimeOfDay.TotalMilliseconds > 0
                                         ? perhapsStartDate.Value.TimeOfDay
                                         : (TimeSpan?) null;
                }

                return new OperationResult<Task>(task) {Key = line};
            }

            var perhapsRecurrenceType = GetLineField(fields, "Recurrence Type", fieldIndexMap)
                .ParseEnum<RecurrenceType>();

            if (!perhapsRecurrenceType.IsThere)
                return new OperationResult<Task>(TaskImportExportResult.CannotImportTask, line);

            task.Recurrence = new Recurrence
                {
                    IsEnabled = true,
                    Type = perhapsRecurrenceType.Value.ToString(),
                    Interval =
                        GetLineField(fields, "Recurrence Interval", fieldIndexMap)
                            .ParseInteger()
                            .DoIfThere(x => x, 1),
                    Duration =
                        GetLineField(fields, "Recurrence Duration", fieldIndexMap)
                            .ParseTimeSpan()
                            .DoIfThere(x => x, TimeSpan.FromHours(1)),
                    TimeOfDay = GetLineField(fields, "Recurrence Time of Day", fieldIndexMap)
                        .ParseTimeSpan()
                        .DoIfThere(x => x, TimeSpan.Zero),
                    DayOfMonth = GetLineField(fields, "Recurrence Day of Month", fieldIndexMap)
                        .ParseInteger()
                        .DoIfThere(x => x, 0),
                    MonthOfYear = GetLineField(fields, "Recurrence Month of Year", fieldIndexMap)
                        .ParseInteger()
                        .DoIfThere(x => x, 0)
                };

            var daysOfWeekField = GetLineField(fields, "Recurrence Days of Week", fieldIndexMap);
            if (!String.IsNullOrWhiteSpace(daysOfWeekField) && perhapsRecurrenceType.Value == RecurrenceType.Weekly)
            {
                task.Recurrence.DaysOfWeek = daysOfWeekField
                    .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.ParseEnum<DayOfWeek>())
                    .Where(x => x.IsThere)
                    .Select(x => x.Value)
                    .ToArray();
            }

            return new OperationResult<Task>(task) {Key = line};
        }

        private static string GetLineField(
            IList<string> fields, string fieldName, IDictionary<string, int> fieldIndexMap)
        {
            var index = fieldIndexMap.LookFor(fieldName);
            return !index.IsThere || fields.Count - 1 < index ? String.Empty : fields[index];
        }

        private static IDictionary<string, int> GetFieldIndexMap(string headerLine)
        {
            var fields = headerLine.Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries);

            return fields
                .Select((x, y) => new {Item = x, Index = y})
                .ToDictionary(x => x.Item, x => x.Index);
        }
    }
}