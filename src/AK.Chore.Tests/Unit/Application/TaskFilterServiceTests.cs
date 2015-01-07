/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.TaskFilterServiceTests
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
using AK.Chore.Contracts.FilterAccess;
using AK.Chore.Contracts.TaskFilter;
using AK.Chore.Domain.Tasks;
using AK.Commons.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using Task = AK.Chore.Domain.Tasks.Task;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for TaskFilterService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class TaskFilterServiceTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFilterService_GetTasksForSavedFilter_Interaction_Works()
        {
            var filterAccessServiceMock = new Mock<IFilterAccessService>();
            filterAccessServiceMock
                .Setup(x => x.GetFilter(1, UserId))
                .Returns(
                    new OperationResult<Filter>(new Filter {Criterion = new Criterion {Type = CriterionType.True}}))
                .Verifiable();

            var taskGrouperMock = new Mock<ITaskGrouper>();
            taskGrouperMock
                .Setup(x => x.LoadForCriterion(Chore.Domain.Filters.Criterion.True, this.user, It.IsAny<DateTime>(),
                                               this.userRepositoryMock.Object, this.folderRepositoryMock.Object,
                                               this.taskRepositoryMock.Object,
                                               It.IsAny<IReadOnlyCollection<Chore.Domain.Folders.Folder>>()))
                .Returns(new Task[0])
                .Verifiable();

            var taskFilterService = this.GetTarget(filterAccessServiceMock, taskGrouperMock);

            var result = taskFilterService.GetTasksForSavedFilter(1, new[] {1, 2}, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);

            filterAccessServiceMock.Verify();
            taskGrouperMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFilterService_GetTasksForUnsavedFilter_Interaction_Works()
        {
            var filterAccessServiceMock = new Mock<IFilterAccessService>();
            var taskGrouperMock = new Mock<ITaskGrouper>();
            taskGrouperMock
                .Setup(x => x.LoadForCriterion(Chore.Domain.Filters.Criterion.True, this.user, It.IsAny<DateTime>(),
                                               this.userRepositoryMock.Object, this.folderRepositoryMock.Object,
                                               this.taskRepositoryMock.Object,
                                               It.IsAny<IReadOnlyCollection<Chore.Domain.Folders.Folder>>()))
                .Returns(new Task[0])
                .Verifiable();

            var taskFilterService = this.GetTarget(filterAccessServiceMock, taskGrouperMock);

            var result = taskFilterService
                .GetTasksForUnsavedFilter(
                    new Filter {Criterion = new Criterion {Type = CriterionType.True}, UserId = UserId},
                    new[] {1, 2}, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);

            taskGrouperMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFilterService_TaskSatisfiesSavedFilter_Interaction_Works()
        {
            var filterAccessServiceMock = new Mock<IFilterAccessService>();
            filterAccessServiceMock
                .Setup(x => x.GetFilter(1, UserId))
                .Returns(
                    new OperationResult<Filter>(new Filter {Criterion = new Criterion {Type = CriterionType.True}}))
                .Verifiable();

            var taskGrouperMock = new Mock<ITaskGrouper>();
            taskGrouperMock
                .Setup(x =>
                       x.TaskSatisfiesCriterion(It.IsAny<Task>(), Chore.Domain.Filters.Criterion.True, this.user,
                                                It.IsAny<DateTime>(),
                                                It.IsAny<IReadOnlyCollection<Chore.Domain.Folders.Folder>>()))
                .Returns(true)
                .Verifiable();

            var taskFilterService = this.GetTarget(filterAccessServiceMock, taskGrouperMock);

            var result = taskFilterService.TaskSatisfiesSavedFilter(1, 1, new[] {1, 2}, UserId, DateTime.Now);

            Assert.IsTrue(result.IsSuccess);

            filterAccessServiceMock.Verify();
            taskGrouperMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskFilterService_TaskSatisfiesUnsavedFilter_Interaction_Works()
        {
            var filterAccessServiceMock = new Mock<IFilterAccessService>();
            var taskGrouperMock = new Mock<ITaskGrouper>();
            taskGrouperMock
                .Setup(x =>
                       x.TaskSatisfiesCriterion(It.IsAny<Task>(), Chore.Domain.Filters.Criterion.True, this.user,
                                                It.IsAny<DateTime>(),
                                                It.IsAny<IReadOnlyCollection<Chore.Domain.Folders.Folder>>()))
                .Returns(true)
                .Verifiable();

            var taskFilterService = this.GetTarget(filterAccessServiceMock, taskGrouperMock);

            var result = taskFilterService.TaskSatisfiesUnsavedFilter(
                1,
                new Filter
                    {
                        Criterion = new Criterion {Type = CriterionType.True},
                        UserId = UserId
                    }, new[] {1, 2}, UserId,
                DateTime.Now);

            Assert.IsTrue(result.IsSuccess);

            taskGrouperMock.Verify();
        }

        private ITaskFilterService GetTarget(
            IMock<IFilterAccessService> filterAccessServiceMock, IMock<ITaskGrouper> taskGrouperMock)
        {
            return new TaskFilterService(
                this.dataAccessMock.Object,
                this.configMock.Object,
                this.logger,
                this.entityIdGeneratorProviderSourceMock.Object,
                filterAccessServiceMock.Object,
                taskGrouperMock.Object);
        }
    }
}