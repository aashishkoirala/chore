/*******************************************************************************************************************************
 * AK.Chore.Application.Services.TaskFlowService
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

using AK.Chore.Application.Mappers;
using AK.Chore.Contracts;
using AK.Chore.Contracts.TaskAccess;
using AK.Chore.Contracts.TaskFlow;
using AK.Chore.Domain;
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
using Task = AK.Chore.Contracts.TaskAccess.Task;

#endregion

namespace AK.Chore.Application.Services
{
    /// <summary>
    /// Service implementation - ITaskFlowService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ITaskFlowService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class TaskFlowService : ServiceBase, ITaskFlowService
    {
        [ImportingConstructor]
        public TaskFlowService(
            [Import] IAppDataAccess appDataAccess,
            [Import] IAppConfig appConfig,
            [Import] IAppLogger logger,
            [Import] IProviderSource<IEntityIdGeneratorProvider> entityIdGeneratorProvider)
            : base(appDataAccess, appConfig, logger, entityIdGeneratorProvider)
        {
        }

        public OperationResult<Task> Start(int id, int userId, DateTime now)
        {
            return this.ExecuteFlow("starting", id, userId, now, x => x.CanStart, x => x.Start());
        }

        public OperationResults<Task> StartAll(IReadOnlyCollection<int> ids, int userId, DateTime now)
        {
            return this.ExecuteFlows("starting", ids, userId, now, x => x.CanStart, x => x.Start());
        }

        public OperationResult<Task> Pause(int id, int userId, DateTime now)
        {
            return this.ExecuteFlow("pausing", id, userId, now, x => x.CanPause, x => x.Pause());
        }

        public OperationResults<Task> PauseAll(IReadOnlyCollection<int> ids, int userId, DateTime now)
        {
            return this.ExecuteFlows("pausing", ids, userId, now, x => x.CanPause, x => x.Pause());
        }

        public OperationResult<Task> Resume(int id, int userId, DateTime now)
        {
            return this.ExecuteFlow("resuming", id, userId, now, x => x.CanResume, x => x.Resume());
        }

        public OperationResults<Task> ResumeAll(IReadOnlyCollection<int> ids, int userId, DateTime now)
        {
            return this.ExecuteFlows("resuming", ids, userId, now, x => x.CanResume, x => x.Resume());
        }

        public OperationResult<Task> Complete(int id, int userId, DateTime now)
        {
            return this.ExecuteFlow("completing", id, userId, now, x => x.CanComplete, x => x.Complete());
        }

        public OperationResults<Task> CompleteAll(IReadOnlyCollection<int> ids, int userId, DateTime now)
        {
            return this.ExecuteFlows("completing", ids, userId, now, x => x.CanComplete, x => x.Complete());
        }

        public OperationResult<Task> EnableRecurrence(int id, int userId, DateTime now)
        {
            return this.ExecuteFlow("enabling recurrence", id, userId, now, x => true, x => x.EnableRecurrence());
        }

        public OperationResults<Task> EnableRecurrenceForAll(IReadOnlyCollection<int> ids, int userId, DateTime now)
        {
            return this.ExecuteFlows("enabling recurrence", ids, userId, now, x => true, x => x.EnableRecurrence());
        }

        public OperationResult<Task> DisableRecurrence(int id, int userId, DateTime now)
        {
            return this.ExecuteFlow("disabling recurrence", id, userId, now, x => true, x => x.DisableRecurrence());
        }

        public OperationResults<Task> DisableRecurrenceForAll(IReadOnlyCollection<int> ids, int userId, DateTime now)
        {
            return this.ExecuteFlows("disabling recurrence", ids, userId, now, x => true, x => x.DisableRecurrence());
        }

        private OperationResult<Task> ExecuteFlow(
            string doingWhat, int id, int userId, DateTime now,
            Func<Domain.Tasks.Task, bool> isAllowedAction,
            Action<Domain.Tasks.Task> flowAction)
        {
            OperationResult<Task> result = null;

            try
            {
                this.Execute((taskRepository, userRepository, folderRepository) =>
                    {
                        var task = taskRepository.Get(id, userRepository, folderRepository);
                        if (task == null)
                        {
                            result = new OperationResult<Task>(TaskAccessResult.TaskDoesNotExist, id);
                            return;
                        }

                        if (task.User.Id != userId)
                        {
                            result = new OperationResult<Task>(GeneralResult.NotAuthorized, id);
                            return;
                        }

                        if (!isAllowedAction(task))
                        {
                            result = new OperationResult<Task>(GeneralResult.InvalidRequest, id,
                                                               "Cannot do that to this task in this state.");
                            return;
                        }

                        flowAction(task);
                        taskRepository.Save(task);

                        result = new OperationResult<Task>(task.Map(now));
                    });
            }
            catch (DomainValidationException ex)
            {
                this.Logger.Error(ex);
                var message = string.Format("We had a problem {0} this task. Perhaps it is not " +
                                            "in the right state for that operation.", doingWhat);

                result = new OperationResult<Task>(GeneralResult.InvalidRequest, id, message);
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex);
                var message = string.Format("We had a problem {0} this task.", doingWhat);

                result = new OperationResult<Task>(GeneralResult.Error, id, message);
            }

            return result;
        }

        private OperationResults<Task> ExecuteFlows(
            string doingWhat, IEnumerable<int> ids, int userId, DateTime now,
            Func<Domain.Tasks.Task, bool> isAllowedAction,
            Action<Domain.Tasks.Task> flowAction)
        {
            var results = new Collection<OperationResult<Task>>();

            try
            {
                this.Execute((taskRepository, userRepository, folderRepository) =>
                    {
                        var tasks = taskRepository.ListForPredicate(
                            x => ids.Contains(x.Id), userRepository, folderRepository);

                        foreach (var result in ids
                            .Except(tasks.Select(x => x.Id))
                            .Select(x => new OperationResult<Task>(TaskAccessResult.TaskDoesNotExist, x)))
                        {
                            results.Add(result);
                        }

                        foreach (var task in tasks)
                        {
                            try
                            {
                                if (task.User.Id != userId)
                                {
                                    var result = new OperationResult<Task>(GeneralResult.NotAuthorized, task.Id);
                                    results.Add(result);
                                    continue;
                                }

                                if (!isAllowedAction(task))
                                {
                                    var result = new OperationResult<Task>(
                                        GeneralResult.InvalidRequest,
                                        task.Id, "Cannot do that to this task in this state.");
                                    results.Add(result);
                                    continue;
                                }

                                flowAction(task);
                                taskRepository.Save(task);

                                results.Add(new OperationResult<Task>(task.Map(now)));
                            }
                            catch (DomainValidationException ex)
                            {
                                this.Logger.Error(ex);
                                var message = string.Format("We had a problem {0} this task. Perhaps it is not " +
                                                            "in the right state for that operation.", doingWhat);

                                var result = new OperationResult<Task>(GeneralResult.InvalidRequest, task.Id, message);
                                results.Add(result);
                            }
                            catch (Exception ex)
                            {
                                this.Logger.Error(ex);
                                var message = string.Format("We had a problem {0} this task.", doingWhat);

                                var result = new OperationResult<Task>(GeneralResult.Error, message);
                                results.Add(result);
                            }
                        }
                    });
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex);
                var message = string.Format("We had a problem {0} these tasks.", doingWhat);

                results.Add(new OperationResult<Task>(GeneralResult.Error, message));
            }

            return new OperationResults<Task>(results);
        }

        private void Execute(Action<ITaskRepository, IUserRepository, IFolderRepository> action)
        {
            this.Db.Execute(action);
        }
    }
}