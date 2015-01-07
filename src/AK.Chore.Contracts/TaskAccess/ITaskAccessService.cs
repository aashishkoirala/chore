/*******************************************************************************************************************************
 * AK.Chore.Contracts.TaskAccess.ITaskAccessService
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

using AK.Commons.Services;
using System;
using System.Collections.Generic;

#endregion

namespace AK.Chore.Contracts.TaskAccess
{
    /// <summary>
    /// CRUD-type operations for tasks.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ITaskAccessService
    {
        /// <summary>
        /// Gets the given task.
        /// </summary>
        /// <param name="id">ID of task to get.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date to use as right now.</param>
        /// <returns>Result with task.</returns>
        OperationResult<Task> GetTask(int id, int userId, DateTime now);

        /// <summary>
        /// Saves the given task.
        /// </summary>
        /// <param name="task">Task to save.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Result with saved task.</returns>
        OperationResult<Task> SaveTask(Task task, int userId);

        /// <summary>
        /// Saves the given tasks.
        /// </summary>
        /// <param name="tasks">List of tasks to save.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Results with saved tasks.</returns>
        OperationResults<Task> SaveTasks(IReadOnlyCollection<Task> tasks, int userId);

        /// <summary>
        /// Deletes the given task.
        /// </summary>
        /// <param name="id">ID of task to delete.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Result with status of operation.</returns>
        OperationResult DeleteTask(int id, int userId);

        /// <summary>
        /// Deletes the given tasks.
        /// </summary>
        /// <param name="ids">List of IDs of tasks to delete.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Results with statuses of operations.</returns>
        OperationResults DeleteTasks(IReadOnlyCollection<int> ids, int userId);

        /// <summary>
        /// Moves the given task.
        /// </summary>
        /// <param name="id">ID of task to move.</param>
        /// <param name="targetFolderId">ID of folder to move task to.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Result with status of operation.</returns>
        OperationResult MoveTask(int id, int targetFolderId, int userId);

        /// <summary>
        /// Moves the given tasks.
        /// </summary>
        /// <param name="ids">List of IDs of tasks to move.</param>
        /// <param name="targetFolderId">ID of folder to move tasks to.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Results with statuses of operations.</returns>
        OperationResults MoveTasks(IReadOnlyCollection<int> ids, int targetFolderId, int userId);
    }
}