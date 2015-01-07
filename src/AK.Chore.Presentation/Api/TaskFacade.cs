/*******************************************************************************************************************************
 * AK.Chore.Presentation.Api.TaskFacade
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

using AK.Chore.Contracts.TaskAccess;
using AK.Chore.Contracts.TaskFilter;
using AK.Chore.Contracts.TaskFlow;
using AK.Commons.Services;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web.Http.Controllers;

#endregion

namespace AK.Chore.Presentation.Api
{
    /// <summary>
    /// Groups task-related operations.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ITaskFacade
    {
        ITaskAccessService Access { get; }
        ITaskFilterService Filter { get; }
        ITaskFlowService Flow { get; }

        OperationResult<IReadOnlyCollection<Task>> GetTasks(TaskQuery query, int userId, HttpControllerContext context);

        OperationResult<TaskSatisfiesFilterCommand> TaskSatisfiesFilter(
            TaskSatisfiesFilterCommand request, int userId, HttpControllerContext context);

        OperationResult<Task> AddTask(Task task, int userId);
        OperationResults<Task> AddTasks(IReadOnlyCollection<Task> tasks, int userId);

        OperationResult<TaskMoveCommand> MoveTask(TaskMoveCommand command, int userId);
        OperationResult<TaskMoveCommand> MoveTasks(TaskMoveCommand command, int userId);
    }

    [Export(typeof (ITaskFacade))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class TaskFacade : ITaskFacade
    {
        public ITaskAccessService Access { get; private set; }
        public ITaskFilterService Filter { get; private set; }
        public ITaskFlowService Flow { get; private set; }

        public TaskFacade()
        {
            this.Access = ServiceFactory.GetService<ITaskAccessService>();
            this.Filter = ServiceFactory.GetService<ITaskFilterService>();
            this.Flow = ServiceFactory.GetService<ITaskFlowService>();
        }

        public OperationResult<IReadOnlyCollection<Task>> GetTasks(
            TaskQuery query, int userId, HttpControllerContext context)
        {
            var folderIds = context.GetFolderIds();
            var now = context.GetNow();

            return !query.UseUnsavedFilter
                       ? this.Filter.GetTasksForSavedFilter(query.FilterId, folderIds, userId, now)
                       : this.Filter.GetTasksForUnsavedFilter(context.GetFilter(), folderIds, userId, now);
        }

        public OperationResult<TaskSatisfiesFilterCommand> TaskSatisfiesFilter(
            TaskSatisfiesFilterCommand request, int userId, HttpControllerContext context)
        {
            var folderIds = context.GetFolderIds();
            var now = context.GetNow();

            var satisfiesResult =
                !request.UseUnsavedFilter
                    ? this.Filter.TaskSatisfiesSavedFilter(request.TaskId, request.FilterId, folderIds, userId, now)
                    : this.Filter.TaskSatisfiesUnsavedFilter(request.TaskId, context.GetFilter(), folderIds, userId, now);

            if (!satisfiesResult.IsSuccess)
                return new OperationResult<TaskSatisfiesFilterCommand>(satisfiesResult);

            request.Satisfies = satisfiesResult.Result;
            return new OperationResult<TaskSatisfiesFilterCommand>(request);
        }

        public OperationResult<Task> AddTask(Task task, int userId)
        {
            task.Id = 0;
            task.UserId = userId;
            return this.Access.SaveTask(task, userId);
        }

        public OperationResults<Task> AddTasks(IReadOnlyCollection<Task> tasks, int userId)
        {
            foreach (var task in tasks)
            {
                task.Id = 0;
                task.UserId = userId;
            }

            return this.Access.SaveTasks(tasks, userId);
        }

        public OperationResult<TaskMoveCommand> MoveTask(TaskMoveCommand command, int userId)
        {
            var taskId = command.TaskIds.FirstOrDefault();
            var result = this.Access.MoveTask(taskId, command.FolderId, userId);

            command.MoveResults = new OperationResults(new[] {result});

            return !result.IsSuccess
                       ? new OperationResult<TaskMoveCommand>(result)
                       : new OperationResult<TaskMoveCommand>(command);
        }

        public OperationResult<TaskMoveCommand> MoveTasks(TaskMoveCommand command, int userId)
        {
            var results = this.Access.MoveTasks(command.TaskIds, command.FolderId, userId);

            command.MoveResults = results;

            var result = new OperationResult<TaskMoveCommand>(command) {IsSuccess = results.IsSuccess};

            return result;
        }
    }
}