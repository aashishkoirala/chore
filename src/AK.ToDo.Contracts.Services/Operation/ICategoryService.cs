/*******************************************************************************************************************************
 * AK.To|Do.Contracts.Services.Data.Operation.ICategoryService
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

using AK.ToDo.Contracts.Services.Data.CategoryContracts;
using System;
using System.Collections.Generic;

#endregion

namespace AK.ToDo.Contracts.Services.Operation
{
    /// <summary>
    /// Operations related to storage and retrieval of To-Do item categories.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ICategoryService
    {
        /// <summary>
        /// Retrieves the requested To-Do item category.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <param name="id">Id of the category to retrieve.</param>
        /// <returns>ToDoCategory object.</returns>
        ToDoCategory Get(Guid userId, Guid id);

        /// <summary>
        /// Retrieves the list of To-Do item categories for the given user.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <returns>List of ToDoCategory objects.</returns>
        IList<ToDoCategory> GetList(Guid userId);

        /// <summary>
        /// Creates or updates the given To-Do item category.
        /// </summary>
        /// <param name="item">ToDoCategory object to create/update.</param>
        /// <returns>Created/updated ToDoCategory object.</returns>
        ToDoCategory Update(ToDoCategory item);

        /// <summary>
        /// Creates or updates the given To-Do item categories.
        /// </summary>
        /// <param name="itemList">ToDoCategory objects to create/update.</param>
        /// <returns>List of created/updated ToDoCategory objects.</returns>
        IList<ToDoCategory> UpdateList(IList<ToDoCategory> itemList);

        /// <summary>
        /// Deletes the given To-Do item category.
        /// </summary>
        /// <param name="item">ToDoCategory object to delete.</param>
        void Delete(ToDoCategory item);

        /// <summary>
        /// Deletes the given To-Do item categories.
        /// </summary>
        /// <param name="itemList">List of ToDoCategory objects to delete.</param>
        void DeleteList(IList<ToDoCategory> itemList);

        /// <summary>
        /// Deletes the To-Do item category with the given Id.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <param name="id">Id of ToDoCategory object to delete.</param>
        void DeleteById(Guid userId, Guid id);

        /// <summary>
        /// Deletes the To-Do item categories with the given Ids.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <param name="idList">List of Ids of ToDoCategory objects to delete.</param>
        void DeleteListByIdList(Guid userId, IList<Guid> idList);
    }
}