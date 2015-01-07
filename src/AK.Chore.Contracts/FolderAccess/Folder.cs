/*******************************************************************************************************************************
 * AK.Chore.Contracts.FolderAccess.Folder
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

using System.Collections.Generic;

namespace AK.Chore.Contracts.FolderAccess
{
    /// <summary>
    /// Represents a folder that can nest other folders.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Folder
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }
        public int? ParentFolderId { get; set; }
        public IReadOnlyCollection<Folder> Folders { get; set; }
    }
}