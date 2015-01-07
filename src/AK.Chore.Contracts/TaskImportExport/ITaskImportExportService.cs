/*******************************************************************************************************************************
 * AK.Chore.Contracts.TaskImportExport.ITaskImportExportService
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
using System.Collections.Generic;

#endregion

namespace AK.Chore.Contracts.TaskImportExport
{
    /// <summary>
    /// Bulk task import/export operations.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ITaskImportExportService
    {
        /// <summary>
        /// Imports tasks from the given formatted text.
        /// </summary>
        /// <param name="importData">Formatted text that represents list of tasks.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Results with tasks that are imported.</returns>
        OperationResults<Task> Import(string importData, int userId);

        /// <summary>
        /// Exports the given list of tasks as formatted text.
        /// </summary>
        /// <param name="ids">List of IDs of tasks to export.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Result with formatted text representing the list of tasks.</returns>
        OperationResult<string> Export(IReadOnlyCollection<int> ids, int userId);
    }
}