/*******************************************************************************************************************************
 * AK.Chore.Application.Services.CalendarViewService
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

using AK.Chore.Application.Aspects;
using AK.Chore.Application.Helpers;
using AK.Chore.Contracts.CalendarView;
using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Tasks;
using AK.Chore.Domain.Users;
using AK.Commons;
using AK.Commons.Configuration;
using AK.Commons.DataAccess;
using AK.Commons.DomainDriven;
using AK.Commons.Logging;
using AK.Commons.Services;
using System;
using System.ComponentModel.Composition;
using System.Linq.Expressions;

#endregion

namespace AK.Chore.Application.Services
{
    /// <summary>
    /// Service implementation - ICalendarViewService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ICalendarViewService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class CalendarViewService : ServiceBase, ICalendarViewService
    {
        private readonly IRecurrenceGrouper recurrenceGrouper;
        private readonly ICalendarRecurrenceCalculator calendarRecurrenceCalculator;
        private readonly ICalendarBuilder calendarBuilder;

        [ImportingConstructor]
        public CalendarViewService(
            [Import] IAppDataAccess appDataAccess,
            [Import] IAppConfig appConfig,
            [Import] IAppLogger logger,
            [Import] IProviderSource<IEntityIdGeneratorProvider> entityIdGeneratorProvider,
            [Import] IRecurrenceGrouper recurrenceGrouper,
            [Import] ICalendarRecurrenceCalculator calendarRecurrenceCalculator,
            [Import] ICalendarBuilder calendarBuilder)
            : base(appDataAccess, appConfig, logger, entityIdGeneratorProvider)
        {
            this.recurrenceGrouper = recurrenceGrouper;
            this.calendarRecurrenceCalculator = calendarRecurrenceCalculator;
            this.calendarBuilder = calendarBuilder;
        }

        [CatchToReturn("There was a problem getting the calendar.")]
        public OperationResult<CalendarWeek> GetCalendarWeekForUser(DateTime dayInWeek, int userId)
        {
            var weekStart = dayInWeek.Date;
            var dayNumber = (int) weekStart.DayOfWeek;
            if (dayNumber > 0) weekStart = weekStart.AddDays(-1*dayNumber);
            var weekEnd = weekStart.AddDays(6);

            var predicate = GetPredicate(userId, weekStart, weekEnd, this.recurrenceGrouper);

            var tasks = this.Execute(
                (taskRepository, userRepository, folderRepository) =>
                taskRepository.ListForPredicate(predicate, userRepository, folderRepository));

            var recurrenceMap = this.calendarRecurrenceCalculator.GetCalendarTaskRecurrenceMap(tasks, weekStart, weekEnd);

            var calendarWeek = this.calendarBuilder.BuildCalendarWeek(weekStart, weekEnd, tasks, recurrenceMap);

            return new OperationResult<CalendarWeek>(calendarWeek);
        }

        private static Expression<Func<Task, bool>> GetPredicate(
            int userId, DateTime weekStart, DateTime weekEnd, IRecurrenceGrouper recurrenceGrouper)
        {
            return x => x.User.Id == userId &&
                        ((x.State != TaskState.Completed &&
                          x.State != TaskState.Recurring &&
                          ((x.EndDate >= weekStart && x.EndDate <= weekEnd) ||
                           (x.StartDate >= weekStart && x.StartDate <= weekEnd))) ||
                         (recurrenceGrouper.TaskSatisfiesRecurrence(x, weekEnd, weekStart)));
        }

        private TResult Execute<TResult>(Func<ITaskRepository, IUserRepository, IFolderRepository, TResult> action)
        {
            return this.Db.Execute(action);
        }
    }
}