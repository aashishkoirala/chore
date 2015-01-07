/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.TaskAccessServiceTests
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

using System;
using AK.Chore.Application.Services;
using AK.Chore.Contracts.TaskAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for TaskAccessService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class TaskAccessServiceTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void TaskAccessService_GetTask_Interaction_Works()
        {
            var taskAccessService = this.GetTarget();

            var result = taskAccessService.GetTask(1, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);

            this.taskRepositoryMock.Verify(
                x => x.Get(1, this.userRepositoryMock.Object, this.folderRepositoryMock.Object));
            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskAccessService_SaveTask_Interaction_Works()
        {
            var taskAccessService = this.GetTarget();

            var task = new Task
                {
                    Description = "TestSave",
                    UserId = UserId,
                    FolderId = 1,
                    EndDate = DateTime.Now
                };

            var result = taskAccessService.SaveTask(task, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.taskRepositoryMock.Verify(x => x.Save(
                It.Is<Chore.Domain.Tasks.Task>(
                    y =>
                    y.User.Equals(this.user) && y.Description == task.Description)));

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskAccessService_SaveTasks_Interaction_Works()
        {
            var taskAccessService = this.GetTarget();

            var task = new Task
                {
                    Description = "TestSave",
                    UserId = UserId,
                    FolderId = 1,
                    EndDate = DateTime.Now
                };

            var result = taskAccessService.SaveTasks(new[] {task}, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.taskRepositoryMock.Verify(x => x.Save(
                It.Is<Chore.Domain.Tasks.Task>(
                    y =>
                    y.User.Equals(this.user) && y.Description == task.Description)));

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskAccessService_DeleteTask_Interaction_Works()
        {
            var taskAccessService = this.GetTarget();

            var result = taskAccessService.DeleteTask(1, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.taskRepositoryMock
                .Verify(x => x.Delete(It.Is<Chore.Domain.Tasks.Task>(y => y.User.Equals(this.user))));

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskAccessService_DeleteTasks_Interaction_Works()
        {
            var taskAccessService = this.GetTarget();

            var result = taskAccessService.DeleteTasks(new[] {1}, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.taskRepositoryMock
                .Verify(x => x.Delete(It.Is<Chore.Domain.Tasks.Task>(y => y.User.Equals(this.user))));

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskAccessService_MoveTask_Interaction_Works()
        {
            var taskAccessService = this.GetTarget();

            var result = taskAccessService.MoveTask(1, 2, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskAccessService_MoveTasks_Interaction_Works()
        {
            var taskAccessService = this.GetTarget();

            var result = taskAccessService.MoveTasks(new[] {1}, 2, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        private ITaskAccessService GetTarget()
        {
            return new TaskAccessService(
                this.dataAccessMock.Object,
                this.configMock.Object,
                this.logger,
                this.entityIdGeneratorProviderSourceMock.Object);
        }
    }
}