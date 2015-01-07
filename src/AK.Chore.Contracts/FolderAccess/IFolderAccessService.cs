/*******************************************************************************************************************************
 * AK.Chore.Contracts.FolderAccess.IFolderAccessService
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

namespace AK.Chore.Contracts.FolderAccess
{
    /// <summary>
    /// CRUD-type operations for folders.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IFolderAccessService
    {
        /// <summary>
        /// Gets folders for the given user.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <returns>Result with list of folders.</returns>
        OperationResult<IReadOnlyCollection<Folder>> GetFoldersForUser(int userId);

        /// <summary>
        /// Gets the given folder.
        /// </summary>
        /// <param name="id">ID of folder to get.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Result with folder.</returns>
        OperationResult<Folder> GetFolder(int id, int userId);

        /// <summary>
        /// Saves the given folder.
        /// </summary>
        /// <param name="folder">Folder to save.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Result with saved folder.</returns>
        OperationResult<Folder> SaveFolder(Folder folder, int userId);

        /// <summary>
        /// Saves the given folders.
        /// </summary>
        /// <param name="folders">List of folders to save.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Results with saved folders.</returns>
        OperationResults<Folder> SaveFolders(IReadOnlyCollection<Folder> folders, int userId);

        /// <summary>
        /// Moves the given folder.
        /// </summary>
        /// <param name="id">ID of folder to move.</param>
        /// <param name="targetParentFolderId">
        /// ID of folder to make the new parent, set to NULL to make this a root folder for the user.
        /// </param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Result with status of operation.</returns>
        OperationResult MoveFolder(int id, int? targetParentFolderId, int userId);

        /// <summary>
        /// Moves the given folders.
        /// </summary>
        /// <param name="ids">List of IDs of folders to move.</param>
        /// <param name="targetParentFolderId">
        /// ID of folder to make the new parent, set to NULL to make this a root folder for the user.
        /// </param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Results with statuses of operations.</returns>
        OperationResults MoveFolders(IReadOnlyCollection<int> ids, int? targetParentFolderId, int userId);

        /// <summary>
        /// Deletes the given folder.
        /// </summary>
        /// <param name="id">ID of folder to delete.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Result with status of operation.</returns>
        OperationResult DeleteFolder(int id, int userId);

        /// <summary>
        /// Deletes the given folders.
        /// </summary>
        /// <param name="ids">List of IDs of folders to delete.</param>
        /// <param name="userId">User ID (for authorization).</param>
        /// <returns>Results with statuses of operations.</returns>
        OperationResults DeleteFolders(IReadOnlyCollection<int> ids, int userId);
    }
}