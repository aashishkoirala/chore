/*******************************************************************************************************************************
 * AK.Chore.Contracts.UserProfile.IUserProfileService
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

using AK.Commons.Services;

namespace AK.Chore.Contracts.UserProfile
{
    /// <summary>
    /// Operations related to user profile management.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IUserProfileService
    {
        /// <summary>
        /// Gets the user by username.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <returns>Result with user information.</returns>
        OperationResult<User> GetUserByUserName(string userName);

        /// <summary>
        /// Creates the given user.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="nickname">Nickname.</param>
        /// <returns>Result with created user.</returns>
        OperationResult<User> CreateUser(string userName, string nickname);

        /// <summary>
        /// Updates the nickname for the given user.
        /// </summary>
        /// <param name="user">User informaton with new nickname.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Result with status of operation.</returns>
        OperationResult UpdateNickname(User user, int userId);

        /// <summary>
        /// Deletes the user profile and all data for the given user.
        /// </summary>
        /// <param name="user">User to delete profile for.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Result with status of operation.</returns>
        OperationResult DeleteUserProfile(User user, int userId);
    }
}