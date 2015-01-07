/*******************************************************************************************************************************
 * AK.Chore.Contracts.FilterAccess.IFilterAccessService
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
using System.Collections.Generic;

#endregion

namespace AK.Chore.Contracts.FilterAccess
{
    /// <summary>
    /// CRUD-type operations for filters.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IFilterAccessService
    {
        /// <summary>
        /// Gets all filters for the given users.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <returns>Result with list of filters.</returns>
        OperationResult<IReadOnlyCollection<Filter>> GetFiltersForUser(int userId);

        /// <summary>
        /// Gets the given filter.
        /// </summary>
        /// <param name="id">ID of filter.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Result with filter.</returns>
        OperationResult<Filter> GetFilter(int id, int userId);

        /// <summary>
        /// Saves the given filter.
        /// </summary>
        /// <param name="filter">Filter to save.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Result with saved filter.</returns>
        OperationResult<Filter> SaveFilter(Filter filter, int userId);

        /// <summary>
        /// Saves the given filters.
        /// </summary>
        /// <param name="filters">List of filters to save.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Results with saved filters.</returns>
        OperationResults<Filter> SaveFilters(IReadOnlyCollection<Filter> filters, int userId);

        /// <summary>
        /// Deletes the filter with the given ID.
        /// </summary>
        /// <param name="id">ID of filter to delete.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Result with status of operation.</returns>
        OperationResult DeleteFilter(int id, int userId);

        /// <summary>
        /// Deletes the filters with the given IDs.
        /// </summary>
        /// <param name="ids">List of ids of filters to delete.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Results with statuses of operations.</returns>
        OperationResults DeleteFilters(IReadOnlyCollection<int> ids, int userId);
    }
}