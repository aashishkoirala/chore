/*******************************************************************************************************************************
 * AK.Chore.Domain.Tasks.ITaskRepository
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

using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Users;
using AK.Commons.DomainDriven;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#endregion

namespace AK.Chore.Domain.Tasks
{
    /// <summary>
    /// Persistence interface for Task domain objects.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ITaskRepository : IDomainRepository
    {
        IReadOnlyCollection<Task> ListForPredicate(
            Expression<Func<Task, bool>> predicate, IUserRepository userRepository, IFolderRepository folderRepository);

        IReadOnlyCollection<Task> List(
            IReadOnlyCollection<int> ids, IUserRepository userRepository, IFolderRepository folderRepository);

        Task Get(int id, IUserRepository userRepository, IFolderRepository folderRepository);
        void Save(Task task);
        void Delete(Task task);
        bool ExistsForFolder(int folderId, string description, int idToExclude);
    }
}