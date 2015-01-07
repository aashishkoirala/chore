/*******************************************************************************************************************************
 * AK.Chore.Application.Services.TaskAccessService
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
using AK.Chore.Contracts.TaskAccess;
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
using Task = AK.Chore.Contracts.TaskAccess.Task;

#endregion

namespace AK.Chore.Application.Services
{
    /// <summary>
    /// Service implementation - ITaskAccessService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ITaskAccessService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class TaskAccessService : ServiceBase, ITaskAccessService
    {
        [ImportingConstructor]
        public TaskAccessService(
            [Import] IAppDataAccess appDataAccess,
            [Import] IAppConfig appConfig,
            [Import] IAppLogger logger,
            [Import] IProviderSource<IEntityIdGeneratorProvider> entityIdGeneratorProvider)
            : base(appDataAccess, appConfig, logger, entityIdGeneratorProvider)
        {
        }

        [CatchToReturn("We had a problem getting that task.")]
        public OperationResult<Task> GetTask(int id, int userId, DateTime now)
        {
            OperationResult<Task> result = null;

            this.Execute((taskRepository, userRepository, folderRepository) =>
                {
                    var task = taskRepository.Get(id, userRepository, folderRepository);
                    if (task == null)
                    {
                        result = new OperationResult<Task>(TaskAccessResult.TaskDoesNotExist, id);
                        return;
                    }

                    result = task.User.Id != userId
                                 ? new OperationResult<Task>(GeneralResult.NotAuthorized, id)
                                 : new OperationResult<Task>(task.Map(now));
                });

            return result;
        }

        [CatchToReturn("We had a problem saving that task.")]
        public OperationResult<Task> SaveTask(Task task, int userId)
        {
            if (task == null) throw new ArgumentNullException("task");

            OperationResult<Task> result = null;

            this.Execute((taskRepository, userRepository, folderRepository) =>
                {
                    result = this.MapAndSave(task, userId, taskRepository, userRepository, folderRepository);
                });

            return result;
        }

        [CatchToReturn("We had a problem saving those Tasks.")]
        public OperationResults<Task> SaveTasks(IReadOnlyCollection<Task> tasks, int userId)
        {
            if (tasks == null) throw new ArgumentNullException("tasks");

            var results = new Collection<OperationResult<Task>>();

            this.Execute((taskRepository, userRepository, folderRepository) =>
                {
                    foreach (var task in tasks)
                        results.Add(this.MapAndSave(task, userId, taskRepository, userRepository, folderRepository));
                });

            return new OperationResults<Task>(results);
        }

        [CatchToReturn("We had a problem moving that task.")]
        public OperationResult MoveTask(int id, int targetFolderId, int userId)
        {
            OperationResult result = null;

            this.Execute((taskRepository, userRepository, folderRepository) =>
                {
                    result = Move(id, targetFolderId, userId, taskRepository, userRepository, folderRepository);
                });

            return result;
        }

        [CatchToReturn("We had a problem moving those tasks.")]
        public OperationResults MoveTasks(IReadOnlyCollection<int> ids, int targetFolderId, int userId)
        {
            var results = new Collection<OperationResult>();

            this.Execute((taskRepository, userRepository, folderRepository) =>
                {
                    foreach (var id in ids)
                        results.Add(Move(id, targetFolderId, userId, taskRepository, userRepository, folderRepository));
                });

            return new OperationResults(results);
        }

        [CatchToReturn("We had a problem deleting that task.")]
        public OperationResult DeleteTask(int id, int userId)
        {
            OperationResult result = null;

            this.Execute((taskRepository, userRepository, folderRepository) =>
                {
                    result = Delete(id, userId, taskRepository, userRepository, folderRepository);
                });

            return result;
        }

        [CatchToReturn("We had a problem deleting those tasks.")]
        public OperationResults DeleteTasks(IReadOnlyCollection<int> ids, int userId)
        {
            if (ids == null) throw new ArgumentNullException("ids");

            var results = new Collection<OperationResult>();

            this.Execute((taskRepository, userRepository, folderRepository) =>
                {
                    foreach (var id in ids)
                        results.Add(Delete(id, userId, taskRepository, userRepository, folderRepository));
                });

            return new OperationResults(results);
        }

        private OperationResult<Task> MapAndSave(
            Task task, int userId,
            ITaskRepository taskRepository,
            IUserRepository userRepository,
            IFolderRepository folderRepository)
        {
            if (task.UserId != userId) return new OperationResult<Task>(GeneralResult.NotAuthorized, task.Id);

            if (taskRepository.ExistsForFolder(task.FolderId, task.Description, task.Id))
                return new OperationResult<Task>(TaskAccessResult.TaskAlreadyExists);

            // ReSharper disable ImplicitlyCapturedClosure

            var entity = task.Map(
                this.IdGenerator,
                id => taskRepository.Get(id, userRepository, folderRepository),
                id => folderRepository.Get(id, userRepository));

            // ReSharper restore ImplicitlyCapturedClosure

            if (entity == null)
            {
                return task.Id > 0
                           ? new OperationResult<Task>(TaskAccessResult.TaskDoesNotExist, task.Id)
                           : new OperationResult<Task>(TaskAccessResult.TaskCouldNotBeMapped, task.Id);
            }

            if (entity.Folder.User.Id != userId)
                return new OperationResult<Task>(GeneralResult.NotAuthorized, task.Id);

            taskRepository.Save(entity);
            task = entity.Map(task.Now);

            return new OperationResult<Task>(task);
        }

        private static OperationResult Delete(
            int id, int userId,
            ITaskRepository taskRepository,
            IUserRepository userRepository,
            IFolderRepository folderRepository)
        {
            var task = taskRepository.Get(id, userRepository, folderRepository);
            if (task == null) return new OperationResult<Task>(TaskAccessResult.TaskDoesNotExist, id);
            if (task.User.Id != userId) return new OperationResult<Task>(GeneralResult.NotAuthorized, id);

            taskRepository.Delete(task);

            return new OperationResult();
        }

        private static OperationResult Move(
            int id, int targetFolderId, int userId,
            ITaskRepository taskRepository,
            IUserRepository userRepository,
            IFolderRepository folderRepository)
        {
            var task = taskRepository.Get(id, userRepository, folderRepository);
            if (task == null) return new OperationResult<Task>(TaskAccessResult.TaskDoesNotExist, id);
            if (task.User.Id != userId) return new OperationResult<Task>(GeneralResult.NotAuthorized, id);

            var folder = folderRepository.Get(targetFolderId, userRepository);
            if (folder == null) return new OperationResult<Task>(FolderAccessResult.FolderDoesNotExist, targetFolderId);
            if (folder.User.Id != userId) return new OperationResult<Task>(GeneralResult.NotAuthorized, targetFolderId);

            if (taskRepository.ExistsForFolder(folder.Id, task.Description, task.Id))
                return new OperationResult<Task>(TaskAccessResult.TaskAlreadyExists, task.Id);

            task.MoveTo(folder);
            taskRepository.Save(task);

            return new OperationResult();
        }

        private void Execute(Action<ITaskRepository, IUserRepository, IFolderRepository> action)
        {
            this.Db.Execute(action);
        }
    }
}