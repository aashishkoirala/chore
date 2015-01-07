/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.TaskFlowServiceTests
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

using AK.Chore.Application.Services;
using AK.Chore.Contracts.TaskFlow;
using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Users;
using AK.Commons.DomainDriven;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for TaskFlowService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class TaskFlowServiceTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFlowService_Start_Interaction_Works()
        {
            var taskFlowService = this.GetTarget();

            var result = taskFlowService.Start(1, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("InProgress", result.Result.State);
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFlowService_StartAll_Interaction_Works()
        {
            var taskFlowService = this.GetTarget();

            var result = taskFlowService.StartAll(new[] {1, 2}, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);
            foreach (var item in result.Results) Assert.AreEqual("InProgress", item.Result.State);
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFlowService_Pause_Interaction_Works()
        {
            var taskFlowService = this.GetTarget();

            var result = taskFlowService.Pause(3, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Paused", result.Result.State);
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFlowService_PauseAll_Interaction_Works()
        {
            var taskFlowService = this.GetTarget();

            var result = taskFlowService.PauseAll(new[] {3, 4}, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);
            foreach (var item in result.Results) Assert.AreEqual("Paused", item.Result.State);
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFlowService_Resume_Interaction_Works()
        {
            var taskFlowService = this.GetTarget();

            var result = taskFlowService.Resume(5, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("InProgress", result.Result.State);
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFlowService_ResumeAll_Interaction_Works()
        {
            var taskFlowService = this.GetTarget();

            var result = taskFlowService.ResumeAll(new[] {5, 6}, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);
            foreach (var item in result.Results) Assert.AreEqual("InProgress", item.Result.State);
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFlowService_Complete_Interaction_Works()
        {
            var taskFlowService = this.GetTarget();

            var result = taskFlowService.Complete(7, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Completed", result.Result.State);
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFlowService_CompleteAll_Interaction_Works()
        {
            var taskFlowService = this.GetTarget();

            var result = taskFlowService.CompleteAll(new[] {7, 8}, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);
            foreach (var item in result.Results) Assert.AreEqual("Completed", item.Result.State);
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFlowService_EnableRecurrence_Interaction_Works()
        {
            var taskFlowService = this.GetTarget();

            var result = taskFlowService.EnableRecurrence(9, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Result.Recurrence.IsEnabled);
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFlowService_EnableRecurrenceForAll_Interaction_Works()
        {
            var taskFlowService = this.GetTarget();

            var result = taskFlowService.EnableRecurrenceForAll(new[] {9, 10}, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);
            foreach (var item in result.Results) Assert.IsTrue(item.Result.Recurrence.IsEnabled);
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFlowService_DisableRecurrence_Interaction_Works()
        {
            var taskFlowService = this.GetTarget();

            var result = taskFlowService.DisableRecurrence(11, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Result.Recurrence.IsEnabled);
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFlowService_DisableRecurrenceForAll_Interaction_Works()
        {
            var taskFlowService = this.GetTarget();

            var result = taskFlowService.DisableRecurrenceForAll(new[] {11, 12}, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);
            foreach (var item in result.Results) Assert.IsFalse(item.Result.Recurrence.IsEnabled);
        }

        private ITaskFlowService GetTarget()
        {
            this.taskRepositoryMock
                .Setup(x =>
                       x.ListForPredicate(It.IsAny<Expression<Func<Chore.Domain.Tasks.Task, bool>>>(),
                                          this.userRepositoryMock.Object, this.folderRepositoryMock.Object))
                .Returns<Expression<Func<Chore.Domain.Tasks.Task, bool>>, IUserRepository, IFolderRepository>(
                    (predicate, u, f) => this.GetTasks(predicate).ToArray());

            this.taskRepositoryMock
                .Setup(x => x.Get(It.IsAny<int>(), this.userRepositoryMock.Object, this.folderRepositoryMock.Object))
                .Returns<int, IUserRepository, IFolderRepository>((id, u, f) => this.GetTask(id));

            return new TaskFlowService(
                this.dataAccessMock.Object,
                this.configMock.Object,
                this.logger,
                this.entityIdGeneratorProviderSourceMock.Object);
        }

        private IEnumerable<Chore.Domain.Tasks.Task> GetTasks(Expression<Func<Chore.Domain.Tasks.Task, bool>> predicate)
        {
            var call = (MethodCallExpression) predicate.Body;
            var memberExpression = ((MemberExpression) call.Arguments[0]);
            var member = (FieldInfo) memberExpression.Member;
            var target = ((ConstantExpression) memberExpression.Expression).Value;

            var value = (int[]) member.GetValue(target);

            return value.Select(this.GetTask);
        }

        private Chore.Domain.Tasks.Task GetTask(int id)
        {
            var folder = new Folder(this.IdGenerator, string.Format("TaskFlowFolder{0}", Guid.NewGuid()), this.user);
            var description = string.Format("Test{0}", id);

            Chore.Domain.Tasks.Task task = null;

            var idGenerator = new StaticIdGenerator(id);

            switch (id)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    task = new Chore.Domain.Tasks.Task(
                        idGenerator, description, folder, DateTime.Today, DateTime.Today.AddDays(-1));
                    break;

                case 7:
                case 8:
                    task = new Chore.Domain.Tasks.Task(idGenerator, description, folder, DateTime.Today);
                    break;

                case 9:
                case 10:
                    task = new Chore.Domain.Tasks.Task(
                        idGenerator, description, folder,
                        Chore.Domain.Tasks.Recurrence.Disabled(Chore.Domain.Tasks.Recurrence.Hourly(5)));
                    break;

                case 11:
                case 12:
                    task = new Chore.Domain.Tasks.Task(
                        idGenerator, description, folder, Chore.Domain.Tasks.Recurrence.Hourly(5));
                    break;
            }

            if (task == null) return null;

            switch (id)
            {
                case 3:
                case 4:
                    task.Start();
                    break;
                case 5:
                case 6:
                    task.Start();
                    task.Pause();
                    break;
            }

            return task;
        }

        private class StaticIdGenerator : IEntityIdGenerator<int>
        {
            private readonly int id;

            public StaticIdGenerator(int id)
            {
                this.id = id;
            }

            public int Next<TEntity>() where TEntity : IEntity<TEntity, int>
            {
                return this.id;
            }
        }
    }
}