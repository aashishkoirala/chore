/*******************************************************************************************************************************
 * AK.Chore.Infrastructure.Persistence.FolderRepository
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
using System.ComponentModel.Composition;
using System.Linq;

#endregion

namespace AK.Chore.Infrastructure.Persistence
{
    /// <summary>
    /// Implementation of IFolderRepository that uses MongoDB.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (Domain.Folders.IFolderRepository)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class FolderRepository : DomainRepositoryBase, Domain.Folders.IFolderRepository
    {
        public IReadOnlyCollection<Domain.Folders.Folder> ListForUser(
            Domain.Users.User user, IUserRepository userRepository)
        {
            var allIdList = this.UnitOfWork.Repository<Folder>().Query
                                .Where(x => x.UserId == user.Id)
                                .Select(x => x.Id)
                                .ToArray();

            return !allIdList.Any()
                       ? new Domain.Folders.Folder[0]
                       : this.List(allIdList, userRepository)
                             .Where(x => x.Parent == null)
                             .ToArray();
        }

        public IReadOnlyCollection<Domain.Folders.Folder> ListForFolder(
            Domain.Folders.Folder folder, IUserRepository userRepository)
        {
            var rootIdList = this.UnitOfWork.Repository<Folder>()
                                 .Query.Where(x => x.ParentFolderId == folder.Id)
                                 .Select(x => x.Id)
                                 .ToArray();
            if (!rootIdList.Any()) return new Domain.Folders.Folder[0];

            var allIdList = rootIdList.ToList();
            this.PopulateAllChildrenIds(rootIdList, allIdList);

            return this.List(allIdList.ToArray(), userRepository)
                       .Where(x => folder.Equals(x.Parent))
                       .ToArray();
        }

        public IReadOnlyCollection<Domain.Folders.Folder> List(int[] ids, IUserRepository userRepository)
        {
            var folders = this.UnitOfWork.Repository<Folder>().Query.Where(x => ids.Contains(x.Id)).ToArray();
            if (!folders.Any()) return new Domain.Folders.Folder[0];

            var unmappedLineage = new Dictionary<int, Folder>();
            var mappedUsers = new Dictionary<int, Domain.Users.User>();
            var mappedLineage = new Dictionary<int, Domain.Folders.Folder>();

            foreach (var folder in folders)
                this.BuildLineageAndUsers(folder, unmappedLineage, mappedUsers, userRepository);

            return folders
                .Select(x => Map(x, unmappedLineage, mappedLineage, mappedUsers))
                .ToArray();
        }

        public Domain.Folders.Folder Get(int id, IUserRepository userRepository)
        {
            var folder = this.UnitOfWork.Repository<Folder>().Query.SingleOrDefault(x => x.Id == id);
            if (folder == null) return null;

            var unmappedLineage = new Dictionary<int, Folder>();
            var mappedLineage = new Dictionary<int, Domain.Folders.Folder>();
            var users = new Dictionary<int, Domain.Users.User>();

            this.BuildLineageAndUsers(folder, unmappedLineage, users, userRepository);

            return Map(folder, unmappedLineage, mappedLineage, users);
        }

        public void Save(Domain.Folders.Folder folder)
        {
            var list = GetFlatListOfChildren(folder);

            foreach (var item in list.Select(Map))
                this.UnitOfWork.Repository<Folder>().Save(item);
        }

        public void Delete(
            Domain.Folders.Folder folder,
            IUserRepository userRepository,
            ITaskRepository taskRepository)
        {
            folder.LoadFolders(userRepository, this);

            var list = GetFlatListOfChildren(folder).Reverse().ToArray();

            var folderIdList = list.Select(x => x.Id).ToArray();
            var tasks = taskRepository
                .ListForPredicate(x => folderIdList.Contains(x.Folder.Id), userRepository, this);

            foreach (var task in tasks) taskRepository.Delete(task);

            foreach (var item in list.Select(Map))
                this.UnitOfWork.Repository<Folder>().Delete(item);
        }

        public int GetRootCountForUser(int userId)
        {
            return this.UnitOfWork
                       .Repository<Folder>()
                       .Query
                       .Count(x => x.UserId == userId && x.ParentFolderId == null);
        }

        public bool ExistsByNameForUser(int userId, string name, int idToExclude)
        {
            return this.UnitOfWork
                       .Repository<Folder>()
                       .Query
                       .Any(x => x.UserId == userId && x.Name == name && x.Id != idToExclude && x.ParentFolderId == null);
        }

        public bool ExistsByNameForParent(int parentId, string name, int idToExclude)
        {
            return this.UnitOfWork
                       .Repository<Folder>()
                       .Query
                       .Any(x => x.ParentFolderId == parentId && x.Name == name && x.Id != idToExclude);
        }

        private static Folder Map(Domain.Folders.Folder folder)
        {
            return new Folder
                {
                    Id = folder.Id,
                    Name = folder.Name,
                    ParentFolderId = folder.Parent != null ? folder.Parent.Id : (int?) null,
                    UserId = folder.User.Id
                };
        }

        private static Domain.Folders.Folder Map(
            Folder folder,
            IDictionary<int, Folder> unmappedLineage,
            IDictionary<int, Domain.Folders.Folder> mappedLineage,
            IDictionary<int, Domain.Users.User> mappedUsers)
        {
            if (folder == null) return null;

            Domain.Folders.Folder mappedFolder;
            if (mappedLineage.TryGetValue(folder.Id, out mappedFolder)) return mappedFolder;

            Domain.Folders.Folder mappedParent = null;
            Domain.Users.User mappedUser = null;

            if (folder.ParentFolderId.HasValue)
            {
                var parentId = folder.ParentFolderId.Value;
                if (!mappedLineage.TryGetValue(parentId, out mappedParent))
                {
                    var unmappedParent = unmappedLineage[parentId];
                    mappedParent = Map(unmappedParent, unmappedLineage, mappedLineage, mappedUsers);
                }
            }
            else mappedUser = mappedUsers[folder.UserId];

            mappedFolder = new FolderImpl(folder.Id, folder.Name, mappedParent, mappedUser);
            mappedLineage[folder.Id] = mappedFolder;

            return mappedFolder;
        }

        private void PopulateAllChildrenIds(IEnumerable<int> rootIdList, ICollection<int> idListToPopulate)
        {
            var nullableRootIdList = rootIdList.Cast<int?>();

            var childIdList =
                this.UnitOfWork.Repository<Folder>()
                    .Query.Where(x => x.ParentFolderId != null && nullableRootIdList.Contains(x.ParentFolderId))
                    .Select(x => x.Id)
                    .ToArray();

            if (!childIdList.Any()) return;

            foreach (var id in childIdList) idListToPopulate.Add(id);

            this.PopulateAllChildrenIds(childIdList, idListToPopulate);
        }

        private static ICollection<Domain.Folders.Folder> GetFlatListOfChildren(Domain.Folders.Folder root)
        {
            var list = new List<Domain.Folders.Folder> {root};

            foreach (var childList in root.Folders.Select(GetFlatListOfChildren))
                list.AddRange(childList);

            return list;
        }

        private void BuildLineageAndUsers(
            Folder folder,
            IDictionary<int, Folder> lineage,
            IDictionary<int, Domain.Users.User> users,
            IUserRepository userRepository)
        {
            if (folder.ParentFolderId.HasValue)
            {
                Folder parent;
                if (!lineage.TryGetValue(folder.ParentFolderId.Value, out parent))
                {
                    parent = this.UnitOfWork.Repository<Folder>().Query
                                 .SingleOrDefault(x => x.Id == folder.ParentFolderId.Value);
                }
                if (parent != null) this.BuildLineageAndUsers(parent, lineage, users, userRepository);
            }

            Domain.Users.User user;
            if (!users.TryGetValue(folder.UserId, out user))
            {
                user = userRepository.Get(folder.UserId);
                users[folder.UserId] = user;
            }

            lineage[folder.Id] = folder;
        }

        private class FolderImpl : Domain.Folders.Folder
        {
            public FolderImpl(int id, string name, Domain.Folders.Folder parent, Domain.Users.User user)
                : base(id, name, parent, user)
            {
            }
        }
    }
}