/*******************************************************************************************************************************
 * AK.To|Do.Contracts.Services.Data.Operation.IItemCommandService
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

using System;
using System.Collections.Generic;

#endregion

namespace AK.ToDo.Contracts.Services.Operation
{
    /// <summary>
    /// Operations related to commands performed on To-Do items.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IItemCommandService
    {
        /// <summary>
        /// Starts the given To-Do item.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <param name="id">Id of the ToDoItem object to start.</param>
        void Start(Guid userId, Guid id);

        /// <summary>
        /// Pauses the given To-Do item.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <param name="id">Id of the ToDoItem object to pause.</param>
        void Pause(Guid userId, Guid id);

        /// <summary>
        /// Completes the given To-Do item.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <param name="id">Id of the ToDoItem object to complete.</param>
        void Complete(Guid userId, Guid id);

        /// <summary>
        /// Starts the given To-Do items.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <param name="idList">List of Ids of the ToDoItem objects to start.</param>
        void StartList(Guid userId, IList<Guid> idList);

        /// <summary>
        /// Pauses the given To-Do items.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <param name="idList">List of Ids of the ToDoItem objects to pause.</param>
        void PauseList(Guid userId, IList<Guid> idList);

        /// <summary>
        /// Completes the given To-Do items.
        /// </summary>
        /// <param name="userId">Id of user making the request.</param>
        /// <param name="idList">List of Ids of the ToDoItem objects to complete.</param>
        void CompleteList(Guid userId, IList<Guid> idList);
    }
}