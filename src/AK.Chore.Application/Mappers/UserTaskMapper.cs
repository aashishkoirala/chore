/*******************************************************************************************************************************
 * AK.Chore.Application.Mappers.UserTaskMapper
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

using AK.Chore.Contracts.UserDataImportExport;
using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Tasks;
using AK.Commons;
using AK.Commons.DomainDriven;
using AK.Commons.Exceptions;
using Task = AK.Chore.Domain.Tasks.Task;

#endregion

namespace AK.Chore.Application.Mappers
{
    /// <summary>
    /// Maps between UserTask data contract and Task domain object.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class UserTaskMapper
    {
        public static UserTask Map(this Task task)
        {
            return new UserTask
                {
                    Description = task.Description,
                    StartDate = task.StartDate,
                    StartTime = task.StartTime,
                    EndDate = task.EndDate,
                    EndTime = task.EndTime,
                    State = task.State.ToString(),
                    Recurrence = task.Recurrence.Map(),
                    IsMundane = task.IsMundane,
                    IsRecurring = task.IsRecurring
                };
        }

        public static Task Map(this UserTask task, Folder folder, IEntityIdGenerator<int> idGenerator)
        {
            if (task.IsRecurring)
            {
                var recurrence = task.Recurrence.Map();
                return new Task(idGenerator, task.Description, folder, recurrence);
            }

            var targetState = task.State.ParseEnum<TaskState>();
            if (!task.EndDate.HasValue) throw new GeneralException("End date not set on task.");

            var taskEntity = task.StartDate.HasValue
                                 ? new Task(idGenerator, task.Description, folder, task.EndDate.Value,
                                            task.StartDate.Value, task.EndTime, task.StartTime)
                                 : new Task(idGenerator, task.Description, folder, task.EndDate.Value,
                                            task.EndTime);

            taskEntity.TransitionTo(targetState);
            taskEntity.IsMundane = task.IsMundane;

            return taskEntity;
        }
    }
}