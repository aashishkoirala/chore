/*******************************************************************************************************************************
 * AK.Chore.Application.Services.FolderAccessService
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
using AK.Chore.Application.Mappers;
using AK.Chore.Contracts;
using AK.Chore.Contracts.FolderAccess;
using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Tasks;
using AK.Chore.Domain.Users;
using AK.Commons;
using AK.Commons.Configuration;
using AK.Commons.DataAccess;
using AK.Commons.DomainDriven;
using AK.Commons.Logging;
using AK.Commons.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Folder = AK.Chore.Contracts.FolderAccess.Folder;

#endregion

namespace AK.Chore.Application.Services
{
    /// <summary>
    /// Service implementation - IFolderAccessService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (IFolderAccessService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class FolderAccessService : ServiceBase, IFolderAccessService
    {
        [ImportingConstructor]
        public FolderAccessService(
            [Import] IAppDataAccess appDataAccess,
            [Import] IAppConfig appConfig,
            [Import] IAppLogger logger,
            [Import] IProviderSource<IEntityIdGeneratorProvider> entityIdGeneratorProvider)
            : base(appDataAccess, appConfig, logger, entityIdGeneratorProvider)
        {
        }

        [CatchToReturn("We had a problem getting the folders for that user.")]
        public OperationResult<IReadOnlyCollection<Folder>> GetFoldersForUser(int userId)
        {
            OperationResult<IReadOnlyCollection<Folder>> result = null;

            this.Execute((folderRepository, userRepository) =>
                {
                    var user = userRepository.Get(userId);
                    if (user == null)
                    {
                        result = new OperationResult<IReadOnlyCollection<Folder>>(
                            FolderAccessResult.FolderUserOrParentDoesNotExist, userId);
                        return;
                    }

                    user.LoadFolders(userRepository, folderRepository);
                    result = new OperationResult<IReadOnlyCollection<Folder>>(
                        user.Folders.Select(x => x.Map()).ToArray());
                });

            return result;
        }

        [CatchToReturn("We had a problem getting that folder.")]
        public OperationResult<Folder> GetFolder(int id, int userId)
        {
            OperationResult<Folder> result = null;

            this.Execute((folderRepository, userRepository) =>
                {
                    var folder = folderRepository.Get(id, userRepository);
                    if (folder == null)
                    {
                        result = new OperationResult<Folder>(FolderAccessResult.FolderDoesNotExist, id);
                        return;
                    }

                    result = folder.User.Id != userId
                                 ? new OperationResult<Folder>(GeneralResult.NotAuthorized, id)
                                 : new OperationResult<Folder>(folder.Map());
                });

            return result;
        }

        [CatchToReturn("We had a problem saving that folder.")]
        public OperationResult<Folder> SaveFolder(Folder folder, int userId)
        {
            if (folder == null) throw new ArgumentNullException("folder");

            OperationResult<Folder> result = null;

            this.Execute((folderRepository, userRepository) =>
                {
                    result = this.MapAndSave(folder, userId, folderRepository, userRepository);
                });

            return result;
        }

        [CatchToReturn("We had a problem saving those folders.")]
        public OperationResults<Folder> SaveFolders(IReadOnlyCollection<Folder> folders, int userId)
        {
            if (folders == null) throw new ArgumentNullException("folders");

            var results = new Collection<OperationResult<Folder>>();

            this.Execute((folderRepository, userRepository) =>
                {
                    foreach (var folder in folders)
                        results.Add(this.MapAndSave(folder, userId, folderRepository, userRepository));
                });

            return new OperationResults<Folder>(results);
        }

        [CatchToReturn("We had a problem moving that folder.")]
        public OperationResult MoveFolder(int id, int? targetParentFolderId, int userId)
        {
            OperationResult result = null;

            this.Execute((folderRepository, userRepository) =>
                {
                    result = Move(id, userId, targetParentFolderId, folderRepository, userRepository);
                });

            return result;
        }

        [CatchToReturn("We had a problem moving those folders.")]
        public OperationResults MoveFolders(IReadOnlyCollection<int> ids, int? targetParentFolderId, int userId)
        {
            var results = new Collection<OperationResult>();

            this.Execute((folderRepository, userRepository) =>
                {
                    foreach (var id in ids)
                        results.Add(Move(id, userId, targetParentFolderId, folderRepository, userRepository));
                });

            return new OperationResults(results);
        }

        [CatchToReturn("We had a problem deleting that folder.")]
        public OperationResult DeleteFolder(int id, int userId)
        {
            OperationResult result = null;

            this.Execute((folderRepository, userRepository, taskRepository) =>
                {
                    var rootCount = folderRepository.GetRootCountForUser(userId);
                    result = Delete(id, userId, rootCount, folderRepository, userRepository, taskRepository);
                });

            return result;
        }

        [CatchToReturn("We had a problem deleting those folders.")]
        public OperationResults DeleteFolders(IReadOnlyCollection<int> ids, int userId)
        {
            if (ids == null) throw new ArgumentNullException("ids");

            var results = new Collection<OperationResult>();

            this.Execute((folderRepository, userRepository, taskRepository) =>
                {
                    var rootCount = folderRepository.GetRootCountForUser(userId);

                    foreach (var id in ids)
                        results.Add(Delete(id, userId, rootCount, folderRepository, userRepository, taskRepository));
                });

            return new OperationResults(results);
        }

        private OperationResult<Folder> MapAndSave(
            Folder folder, int userId,
            IFolderRepository folderRepository, IUserRepository userRepository)
        {
            if (folder.UserId != userId)
                return new OperationResult<Folder>(GeneralResult.NotAuthorized, folder.Id);

            if (folder.ParentFolderId == null && folderRepository.ExistsByNameForUser(userId, folder.Name, folder.Id))
                return new OperationResult<Folder>(FolderAccessResult.FolderAlreadyExists, folder.Id);

            if (folder.ParentFolderId != null &&
                folderRepository.ExistsByNameForParent(folder.ParentFolderId.Value, folder.Name, folder.Id))
                return new OperationResult<Folder>(FolderAccessResult.FolderAlreadyExists, folder.Id);

            var entity = folder.Map(
                this.IdGenerator,
                id => folderRepository.Get(id, userRepository),
                userRepository.Get);

            if (entity == null)
            {
                return folder.Id > 0
                           ? new OperationResult<Folder>(FolderAccessResult.FolderDoesNotExist, folder.Id)
                           : new OperationResult<Folder>(FolderAccessResult.FolderUserOrParentDoesNotExist,
                                                         folder.ParentFolderId ?? folder.UserId);
            }

            folderRepository.Save(entity);

            folder.Id = entity.Id;
            return new OperationResult<Folder>(folder);
        }

        private static OperationResult Delete(
            int id, int userId,
            int rootCount,
            IFolderRepository folderRepository,
            IUserRepository userRepository,
            ITaskRepository taskRepository)
        {
            var folder = folderRepository.Get(id, userRepository);
            if (folder == null)
                return new OperationResult(FolderAccessResult.FolderDoesNotExist, id);

            if (folder.User.Id != userId)
                return new OperationResult(GeneralResult.NotAuthorized, id);

            if (rootCount == 1 && folder.Parent == null)
                return new OperationResult(FolderAccessResult.CannotDeleteOnlyRootFolder, id);

            folderRepository.Delete(folder, userRepository, taskRepository);
            return new OperationResult();
        }

        private static OperationResult Move(
            int id, int userId, int? targetParentFolderId,
            IFolderRepository folderRepository, IUserRepository userRepository)
        {
            var folder = folderRepository.Get(id, userRepository);

            if (folder == null)
                return new OperationResult(FolderAccessResult.FolderDoesNotExist, id);

            var newParent = targetParentFolderId.HasValue
                                ? folderRepository.Get(targetParentFolderId.Value, userRepository)
                                : null;

            if (newParent != null && newParent.FullPath.StartsWith(folder.FullPath) &&
                newParent.FullPath.Length > folder.FullPath.Length)
            {
                return new OperationResult(FolderAccessResult.CannotMoveToChildFolder, id);
            }

            if (folder.User.Id != userId)
                return new OperationResult(GeneralResult.NotAuthorized, id);

            if (newParent != null && newParent.User.Id != userId)
                return new OperationResult(GeneralResult.NotAuthorized, newParent.Id);

            if (targetParentFolderId.HasValue && newParent == null)
                return new OperationResult(FolderAccessResult.FolderDoesNotExist, targetParentFolderId.Value);

            if (newParent == null && folderRepository.ExistsByNameForUser(userId, folder.Name, folder.Id))
                return new OperationResult<Folder>(FolderAccessResult.FolderAlreadyExists, folder.Id);

            if (newParent != null && folderRepository.ExistsByNameForParent(newParent.Id, folder.Name, folder.Id))
                return new OperationResult<Folder>(FolderAccessResult.FolderAlreadyExists, folder.Id);

            folder.MoveTo(newParent);
            folderRepository.Save(folder);

            return new OperationResult();
        }

        private void Execute(Action<IFolderRepository, IUserRepository> action)
        {
            this.Db.Execute(action);
        }

        private void Execute(Action<IFolderRepository, IUserRepository, ITaskRepository> action)
        {
            this.Db.Execute(action);
        }
    }
}