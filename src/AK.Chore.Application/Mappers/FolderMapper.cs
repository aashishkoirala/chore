/*******************************************************************************************************************************
 * AK.Chore.Application.Mappers.FolderMapper
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

using AK.Chore.Contracts.FolderAccess;
using AK.Commons.DomainDriven;
using System;
using System.Linq;

#endregion

namespace AK.Chore.Application.Mappers
{
    /// <summary>
    /// Maps between Folder data contracts and domain objects.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class FolderMapper
    {
        public static Folder Map(this Domain.Folders.Folder folder)
        {
            if (folder == null) return null;

            return new Folder
                {
                    Id = folder.Id,
                    UserId = folder.User.Id,
                    Name = folder.Name,
                    ParentFolderId = folder.Parent == null ? (int?) null : folder.Parent.Id,
                    FullPath = folder.FullPath,
                    Folders = folder.Folders.Select(Map).ToArray()
                };
        }

        public static Domain.Folders.Folder Map(
            this Folder folder,
            IEntityIdGenerator<int> idGenerator,
            Func<int, Domain.Folders.Folder> folderRetriever,
            Func<int, Domain.Users.User> userRetriever)
        {
            if (folder == null) return null;

            if (folder.Id > 0)
            {
                var mapped = folderRetriever(folder.Id);
                if (mapped == null) return null;

                mapped.Name = folder.Name;
                return mapped;
            }

            if (folder.ParentFolderId.HasValue)
            {
                var parent = folderRetriever(folder.ParentFolderId.Value);
                return parent == null
                           ? null
                           : new Domain.Folders.Folder(idGenerator, folder.Name, parent);
            }

            var user = userRetriever(folder.UserId);

            return user == null
                       ? null
                       : new Domain.Folders.Folder(idGenerator, folder.Name, user);
        }
    }
}