/*******************************************************************************************************************************
 * AK.Chore.Infrastructure.Persistence.UserRepository
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
using System.ComponentModel.Composition;
using System.Linq;

#endregion

namespace AK.Chore.Infrastructure.Persistence
{
    /// <summary>
    /// Implementation of IUserRepository that uses MongoDB.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (Domain.Users.IUserRepository)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class UserRepository : DomainRepositoryBase, Domain.Users.IUserRepository
    {
        public Domain.Users.User Get(int id)
        {
            return Map(this.UnitOfWork.Repository<User>().Query.SingleOrDefault(x => x.Id == id));
        }

        public Domain.Users.User GetByKey(string key)
        {
            return Map(this.UnitOfWork.Repository<User>().Query.SingleOrDefault(x => x.Key == key));
        }

        public Domain.Users.User GetWithChildren(int id, IFilterRepository filterRepository,
                                                 IFolderRepository folderRepository, ITaskRepository taskRepository,
                                                 ITaskGrouper taskGrouper)
        {
            var user = this.Get(id);
            user.LoadFilters(filterRepository);
            user.LoadFolders(this, folderRepository);

            foreach (var folder in user.Folders)
            {
                this.LoadFolderChildren(folder, folderRepository, taskRepository, taskGrouper);
            }

            return user;
        }

        private void LoadFolderChildren(Domain.Folders.Folder folder, IFolderRepository folderRepository,
                                        ITaskRepository taskRepository, ITaskGrouper taskGrouper)
        {
            folder.LoadFolders(this, folderRepository);
            folder.LoadTasks(taskGrouper, this, folderRepository, taskRepository);

            foreach (var child in folder.Folders)
                this.LoadFolderChildren(child, folderRepository, taskRepository, taskGrouper);
        }

        public void Save(Domain.Users.User user)
        {
            this.UnitOfWork.Repository<User>().Save(Map(user));
        }

        public void Delete(Domain.Users.User user)
        {
            this.UnitOfWork.Repository<User>().Delete(Map(user));
        }

        private static User Map(Domain.Users.User user)
        {
            return new User
                {
                    Id = user.Id,
                    Key = user.Key,
                    Nickname = user.Nickname
                };
        }

        private static Domain.Users.User Map(User user)
        {
            return user == null ? null : new UserImpl(user.Id, user.Key, user.Nickname);
        }

        private class UserImpl : Domain.Users.User
        {
            public UserImpl(int id, string key, string nickname)
            {
                this.Id = id;
                this.Key = key;
                this.Nickname = nickname;
            }
        }
    }
}