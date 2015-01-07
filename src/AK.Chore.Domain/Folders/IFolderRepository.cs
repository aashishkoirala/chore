/*******************************************************************************************************************************
 * AK.Chore.Domain.Folders.IFolderRepository
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

using AK.Chore.Domain.Tasks;
using AK.Chore.Domain.Users;
using AK.Commons.DomainDriven;
using System.Collections.Generic;

#endregion

namespace AK.Chore.Domain.Folders
{
    /// <summary>
    /// Persistence interface for Folder domain objects.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IFolderRepository : IDomainRepository
    {
        IReadOnlyCollection<Folder> ListForUser(User user, IUserRepository userRepository);
        IReadOnlyCollection<Folder> ListForFolder(Folder folder, IUserRepository userRepository);
        IReadOnlyCollection<Folder> List(int[] ids, IUserRepository userRepository);
        Folder Get(int id, IUserRepository userRepository);
        void Save(Folder folder);
        void Delete(Folder folder, IUserRepository userRepository, ITaskRepository taskRepository);
        int GetRootCountForUser(int userId);
        bool ExistsByNameForUser(int userId, string name, int idToExclude);
        bool ExistsByNameForParent(int parentId, string name, int idToExclude);
    }
}