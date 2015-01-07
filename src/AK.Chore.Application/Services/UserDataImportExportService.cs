/*******************************************************************************************************************************
 * AK.Chore.Application.Services.UserDataImportExportService
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

using AK.Chore.Application.Aspects;
using AK.Chore.Application.Helpers;
using AK.Chore.Application.Mappers;
using AK.Chore.Contracts.UserDataImportExport;
using AK.Chore.Domain.Filters;
using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Tasks;
using AK.Chore.Domain.Users;
using AK.Commons;
using AK.Commons.Configuration;
using AK.Commons.DataAccess;
using AK.Commons.DomainDriven;
using AK.Commons.Logging;
using AK.Commons.Services;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

#endregion

namespace AK.Chore.Application.Services
{
    /// <summary>
    /// Service implementation - IUserDataImportExportService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (IUserDataImportExportService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class UserDataImportExportService : ServiceBase, IUserDataImportExportService
    {
        private readonly ITaskGrouper taskGrouper;
        private readonly IUserDataImportValidator userDataImportValidator;

        [ImportingConstructor]
        public UserDataImportExportService(
            [Import] IAppDataAccess appDataAccess,
            [Import] IAppConfig appConfig,
            [Import] IAppLogger logger,
            [Import] IProviderSource<IEntityIdGeneratorProvider> entityIdGeneratorProvider,
            [Import] ITaskGrouper taskGrouper,
            [Import] IUserDataImportValidator userDataImportValidator)
            : base(appDataAccess, appConfig, logger, entityIdGeneratorProvider)
        {
            this.taskGrouper = taskGrouper;
            this.userDataImportValidator = userDataImportValidator;
        }

        [CatchToReturnMany("There was a problem importing user data.")]
        public OperationResults Import(UserData userData, int userId)
        {
            userData.Filters = userData.Filters ?? new UserFilter[0];
            userData.Folders = userData.Folders ?? new UserFolder[0];

            var user = this.GetUserWithChildren(userId);

            var results = this.userDataImportValidator.Validate(userData, user);
            if (!results.IsSuccess) return results;

            user = this.Db.Execute<IUserRepository, User>(x => x.Get(userId));

            user.Nickname = userData.Nickname;

            Iterate(from filter in userData.Filters
                    select new Filter(this.IdGenerator, filter.Name, user, filter.Criterion.Map()));

            foreach (var folder in userData.Folders) this.AddFolder(folder, user, null);

            this.Db.Execute<IUserRepository, IFolderRepository, IFilterRepository, ITaskRepository>(
                (userRepository, folderRepository, filterRepository, taskRepository) =>
                    {
                        userRepository.Save(user);
                        foreach (var filter in user.Filters) filterRepository.Save(filter);
                        foreach (var folder in user.Folders)
                            this.SaveFolder(folder, folderRepository, taskRepository);
                    });

            return results;
        }

        [CatchToReturn("There was a problem exporting user data.")]
        public OperationResult<UserData> Export(int userId)
        {
            var user = this.GetUserWithChildren(userId);

            var filters = user
                .Filters
                .Select(x => new UserFilter {Name = x.Name, Criterion = x.Criterion.Map()})
                .ToArray();

            var userData = new UserData
                {
                    Nickname = user.Nickname,
                    Filters = filters,
                    Folders = user.Folders.Select(Map).ToArray()
                };

            return new OperationResult<UserData>(userData);
        }

        private static UserFolder Map(Folder folder)
        {
            var userFolder = new UserFolder
                {
                    Name = folder.Name,
                    Folders = new UserFolder[0],
                    Tasks = new UserTask[0]
                };

            if (folder.Folders.Any()) userFolder.Folders = folder.Folders.Select(Map).ToArray();
            if (folder.Tasks.Any()) userFolder.Tasks = folder.Tasks.Select(x => x.Map()).ToArray();

            return userFolder;
        }

        private User GetUserWithChildren(int userId)
        {
            return this.Db.Execute<IUserRepository, IFolderRepository, IFilterRepository, ITaskRepository, User>(
                (userRepository, folderRepository, filterRepository, taskRepository) =>
                    {
                        var user = userRepository.Get(userId);
                        user.LoadFilters(filterRepository);
                        user.LoadFolders(userRepository, folderRepository);

                        foreach (var folder in user.Folders)
                            this.LoadChildrenForFolder(folder, userRepository, folderRepository, taskRepository);

                        return user;
                    });
        }

        private void LoadChildrenForFolder(
            Folder folder,
            IUserRepository userRepository,
            IFolderRepository folderRepository,
            ITaskRepository taskRepository)
        {
            folder.LoadFolders(userRepository, folderRepository);
            folder.LoadTasks(this.taskGrouper, userRepository, folderRepository, taskRepository);

            foreach (var child in folder.Folders)
                LoadChildrenForFolder(child, userRepository, folderRepository, taskRepository);
        }

        private void AddFolder(UserFolder userFolder, User user, Folder parent)
        {
            var folder = parent != null
                             ? new Folder(this.IdGenerator, userFolder.Name, parent)
                             : new Folder(this.IdGenerator, userFolder.Name, user);

            Iterate(userFolder.Tasks.Select(x => x.Map(folder, this.IdGenerator)));

            foreach (var child in userFolder.Folders) this.AddFolder(child, user, folder);
        }

        private void SaveFolder(Folder folder, IFolderRepository folderRepository, ITaskRepository taskRepository)
        {
            folderRepository.Save(folder);
            foreach (var task in folder.Tasks) taskRepository.Save(task);

            foreach (var child in folder.Folders) this.SaveFolder(child, folderRepository, taskRepository);
        }

        private static void Iterate<T>(IEnumerable<T> list)
        {
            foreach (var item in list)
            {
                // ReSharper disable UnusedVariable

                var unused = item;

                // ReSharper restore UnusedVariable
            }
        }
    }
}