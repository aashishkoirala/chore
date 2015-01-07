/*******************************************************************************************************************************
 * AK.Chore.Application.Services.TaskFilterService
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
using AK.Chore.Application.Mappers;
using AK.Chore.Contracts;
using AK.Chore.Contracts.FilterAccess;
using AK.Chore.Contracts.TaskAccess;
using AK.Chore.Contracts.TaskFilter;
using AK.Chore.Domain.Filters;
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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Criterion = AK.Chore.Contracts.FilterAccess.Criterion;
using Filter = AK.Chore.Contracts.FilterAccess.Filter;
using Task = AK.Chore.Contracts.TaskAccess.Task;

#endregion

namespace AK.Chore.Application.Services
{
    /// <summary>
    /// Service implementation - ITaskFilterService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof(ITaskFilterService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class TaskFilterService : ServiceBase, ITaskFilterService
    {
        private readonly IFilterAccessService filterAccessService;
        private readonly ITaskGrouper taskGrouper;

        [ImportingConstructor]
        public TaskFilterService(
            [Import] IAppDataAccess appDataAccess,
            [Import] IAppConfig appConfig,
            [Import] IAppLogger logger,
            [Import] IProviderSource<IEntityIdGeneratorProvider> entityIdGeneratorProvider,
            [Import] IFilterAccessService filterAccessService,
            [Import] ITaskGrouper taskGrouper)
            : base(appDataAccess, appConfig, logger, entityIdGeneratorProvider)
        {
            this.filterAccessService = filterAccessService;
            this.taskGrouper = taskGrouper;
        }

        [CatchToReturn("We had a problem getting tasks matching those criteria.")]
        public OperationResult<IReadOnlyCollection<Task>> GetTasksForSavedFilter(
            int filterId, IReadOnlyCollection<int> folderIds, int userId, DateTime now)
        {
            var filterResult = this.filterAccessService.GetFilter(filterId, userId);

            return !filterResult.IsSuccess
                       ? new OperationResult<IReadOnlyCollection<Task>>(filterResult)
                       : this.GetTasksForCriterion(filterResult.Result.Criterion, folderIds, userId, now);
        }

        [CatchToReturn("We had a problem getting tasks matching those criteria.")]
        public OperationResult<IReadOnlyCollection<Task>> GetTasksForUnsavedFilter(
            Filter filter, IReadOnlyCollection<int> folderIds, int userId, DateTime now)
        {
            if (filter == null || filter.Criterion == null)
                return new OperationResult<IReadOnlyCollection<Task>>(GeneralResult.InvalidRequest);

            return filter.UserId != userId
                       ? new OperationResult<IReadOnlyCollection<Task>>(GeneralResult.NotAuthorized, filter.Id)
                       : this.GetTasksForCriterion(filter.Criterion, folderIds, userId, now);
        }

        [CatchToReturn("We had a problem figuring out if that task matches that criteria.")]
        public OperationResult<bool> TaskSatisfiesSavedFilter(
            int id, int filterId, IReadOnlyCollection<int> folderIds, int userId, DateTime now)
        {
            var filterResult = this.filterAccessService.GetFilter(filterId, userId);

            return !filterResult.IsSuccess
                       ? new OperationResult<bool>(filterResult)
                       : this.TaskSatisfiesCriterion(id, filterResult.Result.Criterion, folderIds, userId, now);
        }

        [CatchToReturn("We had a problem figuring out if that task matches that criteria.")]
        public OperationResult<bool> TaskSatisfiesUnsavedFilter(
            int id, Filter filter, IReadOnlyCollection<int> folderIds, int userId, DateTime now)
        {
            if (filter == null || filter.Criterion == null)
                return new OperationResult<bool>(GeneralResult.InvalidRequest);

            return filter.UserId != userId
                       ? new OperationResult<bool>(GeneralResult.NotAuthorized, filter.Id)
                       : this.TaskSatisfiesCriterion(id, filter.Criterion, folderIds, userId, now);
        }

        private OperationResult<IReadOnlyCollection<Task>> GetTasksForCriterion(
            Criterion criterion, IReadOnlyCollection<int> folderIds, int userId, DateTime now)
        {
            if (folderIds == null) folderIds = new int[0];

            OperationResult<IReadOnlyCollection<Task>> result = null;

            this.Execute((userRepository, filterRepository, folderRepository, taskRepository) =>
                {
                    var user = userRepository.Get(userId);
                    if (user == null)
                    {
                        result = new OperationResult<IReadOnlyCollection<Task>>(
                            FilterAccessResult.FilterUserDoesNotExist, userId);
                        return;
                    }

                    IReadOnlyCollection<Folder> folders = null;
                    if (folderIds.Any())
                        folders = folderRepository.List(folderIds.ToArray(), userRepository);

                    var today = now.Date;

                    var entities = this.taskGrouper.LoadForCriterion(
                        criterion.Map(), user, today, userRepository, folderRepository, taskRepository, folders);

                    var tasks = entities
                        .Where(x => !x.IsMundane)
                        .Select(x => x.Map(now))
                        .OrderBy(x => x.IsLate ? 1 : 2)
                        .ThenByDescending(x => x.IsRecurring ? 1 : 2)
                        .ThenBy(GetTaskOrderDate)
                        .ToArray();

                    result = new OperationResult<IReadOnlyCollection<Task>>(tasks);
                });

            return result;
        }

        private OperationResult<bool> TaskSatisfiesCriterion(
            int id, Criterion criterion, IReadOnlyCollection<int> folderIds, int userId, DateTime now)
        {
            if (criterion == null)
                return new OperationResult<bool>(GeneralResult.InvalidRequest);

            if (folderIds == null) folderIds = new int[0];

            OperationResult<bool> result = null;

            this.Execute((userRepository, folderRepository, taskRepository) =>
                {
                    var user = userRepository.Get(userId);
                    if (user == null)
                    {
                        result = new OperationResult<bool>(FilterAccessResult.FilterUserDoesNotExist, userId);
                        return;
                    }

                    var task = taskRepository.Get(id, userRepository, folderRepository);
                    if (task == null)
                    {
                        result = new OperationResult<bool>(TaskAccessResult.TaskDoesNotExist, id);
                        return;
                    }

                    if (task.User.Id != userId)
                    {
                        result = new OperationResult<bool>(GeneralResult.NotAuthorized, task.Id);
                        return;
                    }

                    IReadOnlyCollection<Folder> folders = null;
                    if (folderIds.Any())
                        folders = folderRepository.List(folderIds.ToArray(), userRepository);

                    var today = now.Date;

                    var satisfies = this.taskGrouper.TaskSatisfiesCriterion(task, criterion.Map(), user, today, folders);

                    result = new OperationResult<bool>(satisfies);
                });

            return result;
        }

        private static DateTime GetTaskOrderDate(Task task)
        {
            var endDate = task.EndDate ?? DateTime.Now;
            endDate = endDate.Add(task.EndTime ?? TimeSpan.Zero);

            if (!task.StartDate.HasValue) return endDate;

            var startDate = task.StartDate.Value;
            startDate = startDate.Add(task.StartTime ?? TimeSpan.Zero);

            return startDate < endDate ? startDate : endDate;
        }

        private void Execute(Action<IUserRepository, IFilterRepository, IFolderRepository, ITaskRepository> action)
        {
            this.Db.Execute(action);
        }

        private void Execute(Action<IUserRepository, IFolderRepository, ITaskRepository> action)
        {
            this.Db.Execute(action);
        }
    }
}
