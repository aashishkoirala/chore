/*******************************************************************************************************************************
 * AK.Chore.Contracts.FolderAccess.FolderAccessResult
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

using AK.Commons;

namespace AK.Chore.Contracts.FolderAccess
{
    /// <summary>
    /// Result codes for IFolderAccess operations.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public enum FolderAccessResult
    {
        [EnumDescription("The folder does not exist. Refer to Key for Id of specified folder.")]
        FolderDoesNotExist,

        [EnumDescription("The folder already exists. Refer to Key for Id of specified folder.")]
        FolderAlreadyExists,

        [EnumDescription("Either the user or parent specified on this folder does not exist. Refer to Key for Id of specified user or parent.")]
        FolderUserOrParentDoesNotExist,

        [EnumDescription("You cannot move a folder to one of its children.")]
        CannotMoveToChildFolder,

        [EnumDescription("You cannot delete the only root folder you have left.")]
        CannotDeleteOnlyRootFolder
    }
}