/*******************************************************************************************************************************
 * AK.To|Do.Contracts.Services.Data.Operation.IAuthorizationService
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

using System;

namespace AK.ToDo.Contracts.Services.Operation
{
    /// <summary>
    /// Operations related to authorization/user access.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Checks to see if the given user has access rights on the given To-Do item,
        /// and throws an exception if it does not.
        /// </summary>
        /// <param name="userId">Id of user.</param>
        /// <param name="itemId">Id of To-Do item.</param>
        void AuthorizeItemOperation(Guid userId, Guid itemId);

        /// <summary>
        /// Checks to see if the given user has access rights on the given To-Do item category,
        /// and throws an exception if it does not.
        /// </summary>
        /// <param name="userId">Id of user.</param>
        /// <param name="categoryId">Id of To-Do item category.</param>
        void AuthorizeCategoryOperation(Guid userId, Guid categoryId);

        /// <summary>
        /// Looks up the given user-name, and creates a new user record if one does not exist.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <returns>Id of the user that was just looked up or created.</returns>
        Guid CreateOrLoadUserIdByName(string userName);
    }
}