/*******************************************************************************************************************************
 * AK.Chore.Infrastructure.Persistence.TaskRepository
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

using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Users;
using AK.Commons.DomainDriven;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace AK.Chore.Infrastructure.Persistence
{
    /// <summary>
    /// Implementation of ITaskRepository that uses MongoDB.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (Domain.Tasks.ITaskRepository)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class TaskRepository : DomainRepositoryBase, Domain.Tasks.ITaskRepository
    {
        private readonly Domain.Tasks.IRecurrencePredicateRewriter recurrencePredicateRewriter;

        private readonly PredicateMapper<Domain.Tasks.Task, Task> mapper =
            new PredicateMapper<Domain.Tasks.Task, Task>();

        private readonly PredicateTransformationExpressionVisitor<Domain.Tasks.Task, Task> mappingVisitor;

        [ImportingConstructor]
        public TaskRepository(
            [Import] Domain.Tasks.IRecurrencePredicateRewriter recurrencePredicateRewriter)
        {
            this.recurrencePredicateRewriter = recurrencePredicateRewriter;

            this.mapper.Map(x => x.User.Id, x => x.UserId);
            this.mapper.Map(x => x.Folder.Id, x => x.FolderId);
            this.mapper.Map(x => x.Recurrence, x => x.Recurrence);

            this.mappingVisitor = new PredicateTransformationExpressionVisitor<Domain.Tasks.Task, Task>(this.mapper);
        }

        public IReadOnlyCollection<Domain.Tasks.Task> ListForPredicate(
            Expression<Func<Domain.Tasks.Task, bool>> predicate,
            IUserRepository userRepository,
            IFolderRepository folderRepository)
        {
            var preQueryPredicate = this.recurrencePredicateRewriter.ReplaceRecurrenceGrouperCall(predicate);
            var postQueryPredicate = this.recurrencePredicateRewriter.ReplaceRecurrenceGrouperParameter(predicate);

            var mappedPreQueryPredicate = this.mappingVisitor.Transform(preQueryPredicate);

            var tasks = this.UnitOfWork.Repository<Task>().Query.Where(mappedPreQueryPredicate).ToArray();

            var folderIdList = tasks.Select(x => x.FolderId).ToArray();
            var folderMap = folderRepository.List(folderIdList, userRepository).ToDictionary(x => x.Id);

            return tasks
                .Select(x => Map(x, folderMap[x.FolderId]))
                .Where(postQueryPredicate.Compile())
                .ToArray();
        }

        public IReadOnlyCollection<Domain.Tasks.Task> List(
            IReadOnlyCollection<int> ids, IUserRepository userRepository, IFolderRepository folderRepository)
        {
            var tasks = this.UnitOfWork.Repository<Task>().Query.Where(x => ids.Contains(x.Id)).ToArray();

            var folderIdList = tasks.Select(x => x.FolderId).ToArray();
            var folderMap = folderRepository.List(folderIdList, userRepository).ToDictionary(x => x.Id);

            return tasks
                .Select(x => Map(x, folderMap[x.FolderId]))
                .ToArray();
        }

        public Domain.Tasks.Task Get(int id, IUserRepository userRepository, IFolderRepository folderRepository)
        {
            var task = this.UnitOfWork.Repository<Task>().Query.SingleOrDefault(x => x.Id == id);
            if (task == null) return null;

            var folder = folderRepository.Get(task.FolderId, userRepository);

            return Map(task, folder);
        }

        public void Save(Domain.Tasks.Task task)
        {
            this.UnitOfWork.Repository<Task>().Save(Map(task));
        }

        public void Delete(Domain.Tasks.Task task)
        {
            this.UnitOfWork.Repository<Task>().Delete(Map(task));
        }

        public bool ExistsForFolder(int folderId, string description, int idToExclude)
        {
            return this.UnitOfWork
                       .Repository<Task>()
                       .Query
                       .Any(x => x.FolderId == folderId && x.Description == description && x.Id != idToExclude);
        }

        private static Domain.Tasks.Recurrence Map(Recurrence recurrence)
        {
            if (recurrence == null) return null;

            var daysOfWeek = string.IsNullOrWhiteSpace(recurrence.DaysOfWeek)
                                 ? new DayOfWeek[0]
                                 : recurrence.DaysOfWeek.Split('|')
                                             .Select(x => (DayOfWeek) Enum.Parse(typeof (DayOfWeek), x))
                                             .ToArray();

            return new RecurrenceImpl(recurrence.IsEnabled, recurrence.Type, recurrence.Interval, recurrence.TimeOfDay,
                                      daysOfWeek, recurrence.DayOfMonth, recurrence.MonthOfYear, recurrence.Duration);
        }

        private static Domain.Tasks.Task Map(Task task, Domain.Folders.Folder folder)
        {
            if (task == null) return null;

            return new TaskImpl(task.Id, task.Description, task.StartDate, task.EndDate, task.StartTime, task.EndTime,
                                task.State, Map(task.Recurrence), folder, task.IsMundane);
        }

        private static Recurrence Map(Domain.Tasks.Recurrence recurrence)
        {
            return new Recurrence
                {
                    IsEnabled = recurrence.IsEnabled,
                    Type = recurrence.Type,
                    Interval = recurrence.Interval,
                    TimeOfDay = recurrence.TimeOfDay,
                    DaysOfWeek =
                        (recurrence.DaysOfWeek == null || !recurrence.DaysOfWeek.Any())
                            ? string.Empty
                            : recurrence.DaysOfWeek
                                        .Select(x => x.ToString())
                                        .Aggregate((x1, x2) => x1 + "|" + x2),
                    DayOfMonth = recurrence.DayOfMonth,
                    MonthOfYear = recurrence.MonthOfYear,
                    Duration = recurrence.Duration
                };
        }

        private static Task Map(Domain.Tasks.Task task)
        {
            var endDate = task.EndDate;
            var startDate = task.StartDate;

            // MongoDB's crazy date thing with UTC is driving me crazy.
            // The following needs to be put in for dates to make sense, but for some reason
            // it does not work locally and my tests fail. However, I need to leave this in
            // and it seems to work in the cloud.

            var offset = DateTime.UtcNow.Subtract(DateTime.Now);

            if (endDate.HasValue) endDate = endDate.Value.Date.Subtract(offset);
            if (startDate.HasValue) startDate = startDate.Value.Date.Subtract(offset);

            return new Task
                {
                    Id = task.Id,
                    Description = task.Description,
                    StartDate = startDate,
                    EndDate = endDate,
                    StartTime = task.StartTime,
                    EndTime = task.EndTime,
                    State = task.State,
                    Recurrence = Map(task.Recurrence),
                    FolderId = task.Folder.Id,
                    UserId = task.User.Id,
                    IsMundane = task.IsMundane
                };
        }

        private class TaskImpl : Domain.Tasks.Task
        {
            public TaskImpl(int id, string description, DateTime? startDate, DateTime? endDate, TimeSpan? startTime,
                            TimeSpan? endTime, Domain.Tasks.TaskState state, Domain.Tasks.Recurrence recurrence,
                            Domain.Folders.Folder folder, bool isMundane) : base(id, description, folder)
            {
                this.State = state;
                this.Recurrence = recurrence;
                this.EndDate = HandleTimeZone(endDate);
                this.EndTime = endTime;
                this.StartDate = HandleTimeZone(startDate);
                this.StartTime = startTime;
                this.IsMundane = isMundane;
            }

            private static DateTime? HandleTimeZone(DateTime? input)
            {
                if (input == null) return null;

                var offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                return input.Value.Add(offset);
            }
        }

        private class RecurrenceImpl : Domain.Tasks.Recurrence
        {
            public RecurrenceImpl(bool isEnabled, Domain.Tasks.RecurrenceType recurrenceType, int interval,
                                  TimeSpan timeOfDay, DayOfWeek[] daysOfWeek, int dayOfMonth, int monthOfYear,
                                  TimeSpan duration)
            {
                this.IsEnabled = isEnabled;
                this.Type = recurrenceType;
                this.Interval = interval;
                this.TimeOfDay = timeOfDay;
                this.DaysOfWeek = daysOfWeek;
                this.DayOfMonth = dayOfMonth;
                this.MonthOfYear = monthOfYear;
                this.Duration = duration;
            }
        }
    }
}