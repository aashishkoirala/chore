/*******************************************************************************************************************************
 * AK.To|Do.Contracts.Services.Data.Operation.IItemService
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
    /// Operations related to retrieval and storage of To-Do items.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IItemService
    {
        /// <summary>
        /// Retrieves the requested To-Do item.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <param name="id">Id of the item.</param>
        /// <returns>ToDoItem object.</returns>
        ToDoItem Get(Guid userId, Guid id);

        /// <summary>
        /// Retrieves the list of To-Do items based on the given request.
        /// </summary>
        /// <param name="request">Request object with list parameters.</param>
        /// <returns>List of ToDoItem objects.</returns>
        IList<ToDoItem> GetList(ToDoItemListRequest request);

        /// <summary>
        /// Creates or updates the given To-Do item.
        /// </summary>
        /// <param name="item">ToDoItem object to create or update.</param>
        /// <returns>Created/updated ToDoItem instance.</returns>
        ToDoItem Update(ToDoItem item);

        /// <summary>
        /// Creates or updates the given To-Do items.
        /// </summary>
        /// <param name="itemList">List of ToDoItem objects to create or update.</param>
        /// <returns>List of created/updated ToDoItem instances.</returns>
        IList<ToDoItem> UpdateList(IList<ToDoItem> itemList);

        /// <summary>
        /// Deletes the given To-Do item.
        /// </summary>
        /// <param name="item">ToDoItem object to delete.</param>
        void Delete(ToDoItem item);

        /// <summary>
        /// Deletes the given To-Do items.
        /// </summary>
        /// <param name="itemList">List of ToDoItem object to delete.</param>
        void DeleteList(IList<ToDoItem> itemList);

        /// <summary>
        /// Deletes the To-Do item with the given Id.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <param name="id">Id of ToDoItem object to delete.</param>
        void DeleteById(Guid userId, Guid id);

        /// <summary>
        /// Deletes the To-Do items with the given Ids.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <param name="idList">List of Ids of ToDoItem objects to delete.</param>
        void DeleteListByIdList(Guid userId, IList<Guid> idList);
    }
}