/*******************************************************************************************************************************
 * AK.Chore.Contracts.TaskFlow.ITaskFlowService
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
using AK.Commons.Services;
using System;
using System.Collections.Generic;

#endregion

namespace AK.Chore.Contracts.TaskFlow
{
    /// <summary>
    /// Operations related to task workflow.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ITaskFlowService
    {
        /// <summary>
        /// Starts the given task.
        /// </summary>
        /// <param name="id">ID of task to start.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date/time to use as right now.</param>
        /// <returns>Result with task that was operated upon.</returns>
        OperationResult<Task> Start(int id, int userId, DateTime now);

        /// <summary>
        /// Starts the given tasks.
        /// </summary>
        /// <param name="ids">List of IDs of task to start.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date/time to use as right now.</param>
        /// <returns>Results with tasks that were operated upon.</returns>
        OperationResults<Task> StartAll(IReadOnlyCollection<int> ids, int userId, DateTime now);

        /// <summary>
        /// Pauses the given task.
        /// </summary>
        /// <param name="id">ID of task to pause.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date/time to use as right now.</param>
        /// <returns>Result with task that was operated upon.</returns>
        OperationResult<Task> Pause(int id, int userId, DateTime now);

        /// <summary>
        /// Pauses the given tasks.
        /// </summary>
        /// <param name="ids">List of IDs of task to pause.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date/time to use as right now.</param>
        /// <returns>Results with tasks that were operated upon.</returns>
        OperationResults<Task> PauseAll(IReadOnlyCollection<int> ids, int userId, DateTime now);

        /// <summary>
        /// Resumes the given task.
        /// </summary>
        /// <param name="id">ID of task to resume.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date/time to use as right now.</param>
        /// <returns>Result with task that was operated upon.</returns>
        OperationResult<Task> Resume(int id, int userId, DateTime now);

        /// <summary>
        /// Resumes the given tasks.
        /// </summary>
        /// <param name="ids">List of IDs of task to resume.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date/time to use as right now.</param>
        /// <returns>Results with tasks that were operated upon.</returns>
        OperationResults<Task> ResumeAll(IReadOnlyCollection<int> ids, int userId, DateTime now);

        /// <summary>
        /// Completes the given task.
        /// </summary>
        /// <param name="id">ID of task to complete.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date/time to use as right now.</param>
        /// <returns>Result with task that was operated upon.</returns>
        OperationResult<Task> Complete(int id, int userId, DateTime now);

        /// <summary>
        /// Completes the given tasks.
        /// </summary>
        /// <param name="ids">List of IDs of task to complete.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date/time to use as right now.</param>
        /// <returns>Results with tasks that were operated upon.</returns>
        OperationResults<Task> CompleteAll(IReadOnlyCollection<int> ids, int userId, DateTime now);

        /// <summary>
        /// Enables recurrence on the given task.
        /// </summary>
        /// <param name="id">ID of task to enable recurrence for.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date/time to use as right now.</param>
        /// <returns>Result with task that was operated upon.</returns>
        OperationResult<Task> EnableRecurrence(int id, int userId, DateTime now);

        /// <summary>
        /// Enables recurrence for the given tasks.
        /// </summary>
        /// <param name="ids">List of IDs of task to enable recurrence for.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date/time to use as right now.</param>
        /// <returns>Results with tasks that were operated upon.</returns>
        OperationResults<Task> EnableRecurrenceForAll(IReadOnlyCollection<int> ids, int userId, DateTime now);

        /// <summary>
        /// Disables recurrence on the given task.
        /// </summary>
        /// <param name="id">ID of task to disable recurrence for.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date/time to use as right now.</param>
        /// <returns>Result with task that was operated upon.</returns>
        OperationResult<Task> DisableRecurrence(int id, int userId, DateTime now);

        /// <summary>
        /// Disables recurrence for the given tasks.
        /// </summary>
        /// <param name="ids">List of IDs of task to disable recurrence for.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date/time to use as right now.</param>
        /// <returns>Results with tasks that were operated upon.</returns>
        OperationResults<Task> DisableRecurrenceForAll(IReadOnlyCollection<int> ids, int userId, DateTime now);
    }
}