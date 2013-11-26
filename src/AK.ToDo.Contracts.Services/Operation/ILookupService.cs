/*******************************************************************************************************************************
 * AK.To|Do.Contracts.Services.Data.Operation.ILookupService
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
using System.Collections.Generic;

#endregion

namespace AK.ToDo.Contracts.Services.Operation
{
    /// <summary>
    /// Operations related to lookup information.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ILookupService
    {
        /// <summary>
        /// Gets the list of possible item states.
        /// </summary>
        /// <returns>List of possible item states.</returns>
        IDictionary<ToDoItemState, string> GetItemStateList();

        /// <summary>
        /// Gets the list of possible item list types.
        /// </summary>
        /// <returns>List of possible item list types.</returns>
        IDictionary<ItemListType, string> GetItemListTypeList();
    }
}