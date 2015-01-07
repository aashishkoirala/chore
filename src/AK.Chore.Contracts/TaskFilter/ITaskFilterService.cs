/*******************************************************************************************************************************
 * AK.Chore.Contracts.TaskFilter.ITaskFilterService
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

using AK.Chore.Contracts.FilterAccess;
using AK.Chore.Contracts.TaskAccess;
using AK.Commons.Services;
using System;
using System.Collections.Generic;

#endregion

namespace AK.Chore.Contracts.TaskFilter
{
    /// <summary>
    /// Operations that list tasks based on filter criteria.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ITaskFilterService
    {
        /// <summary>
        /// Gets tasks matching given saved filter.
        /// </summary>
        /// <param name="filterId">ID of saved filter.</param>
        /// <param name="folderIds">List of IDs of folders to limit tasks to (empty means include all folders).</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date to use as now.</param>
        /// <returns>Result with list of tasks.</returns>
        OperationResult<IReadOnlyCollection<Task>> GetTasksForSavedFilter(
            int filterId, IReadOnlyCollection<int> folderIds, int userId, DateTime now);

        /// <summary>
        /// Gets tasks matching given unsaved filter.
        /// </summary>
        /// <param name="filter">Unsaved filter.</param>
        /// <param name="folderIds">List of IDs of folders to limit tasks to (empty means include all folders).</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date to use as now.</param>
        /// <returns>Result with list of tasks.</returns>
        OperationResult<IReadOnlyCollection<Task>> GetTasksForUnsavedFilter(
            Filter filter, IReadOnlyCollection<int> folderIds, int userId, DateTime now);

        /// <summary>
        /// Checks whether given task matches given saved filter.
        /// </summary>
        /// <param name="id">ID of task to check.</param>
        /// <param name="filterId">ID of saved filter.</param>
        /// <param name="folderIds">List of IDs of folders to limit tasks to (empty means include all folders).</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date to use as now.</param>
        /// <returns>Result with whether task matches filter.</returns>
        OperationResult<bool> TaskSatisfiesSavedFilter(
            int id, int filterId, IReadOnlyCollection<int> folderIds, int userId, DateTime now);

        /// <summary>
        /// Checks whether given task matches given unsaved filter.
        /// </summary>
        /// <param name="id">ID of task to check.</param>
        /// <param name="filter">Unsaved filter.</param>
        /// <param name="folderIds">List of IDs of folders to limit tasks to (empty means include all folders).</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <param name="now">Date to use as now.</param>
        /// <returns>Result with whether task matches filter.</returns>
        OperationResult<bool> TaskSatisfiesUnsavedFilter(
            int id, Filter filter, IReadOnlyCollection<int> folderIds, int userId, DateTime now);
    }
}