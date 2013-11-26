/*******************************************************************************************************************************
 * AK.To|Do.Contracts.Services.Data.Operation.IItemImportService
 * Copyright © 2013 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of To Do.
 *  
 * To Do is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * To Do is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with To Do.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

#region Namespace Imports

using AK.ToDo.Contracts.Services.Data.ItemContracts;
using System;
using System.Collections.Generic;

#endregion

namespace AK.ToDo.Contracts.Services.Operation
{
    /// <summary>
    /// Operations related to import of To-Do items.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IItemImportService
    {
        /// <summary>
        /// Imports the given TAB-delimited lines of text as a list of To-Do items.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <param name="importData">TAB-delimited lines of text to import.</param>
        /// <returns>List of created ToDoItem objects as a result of the import.</returns>
        IList<ToDoItem> Import(Guid userId, string importData);
    }
}