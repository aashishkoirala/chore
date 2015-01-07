/*******************************************************************************************************************************
 * AK.Chore.Domain.Users.IUserRepository
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

using AK.Chore.Domain.Filters;
using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Tasks;
using AK.Commons.DomainDriven;

#endregion

namespace AK.Chore.Domain.Users
{
    /// <summary>
    /// Persistence interface for the User domain object.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IUserRepository : IDomainRepository
    {
        User Get(int id);
        User GetByKey(string key);

        User GetWithChildren(
            int id,
            IFilterRepository filterRepository, IFolderRepository folderRepository, ITaskRepository taskRepository,
            ITaskGrouper taskGrouper);

        void Save(User user);
        void Delete(User user);
    }
}