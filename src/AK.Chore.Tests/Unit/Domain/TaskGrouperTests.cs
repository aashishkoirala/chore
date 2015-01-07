/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Domain.TaskGrouperTests
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
using AK.Chore.Domain.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace AK.Chore.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests for TaskGrouper.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class TaskGrouperTests : TestBase
    {
        private User user;
        private Folder folder;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            this.user = new User(this.idGenerator, "Test", "Test", this.userKeyGenerator, this.builtInCriterionProvider);
            this.folder = new Folder(this.idGenerator, "Test", this.user);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void TaskGrouper_LoadForCriterion_Works()
        {
            Expression<Func<Task, bool>> predicate = null;
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.Get(It.IsAny<int>()))
                .Returns(this.user);

            var folderRepositoryMock = new Mock<IFolderRepository>();
            folderRepositoryMock
                .Setup(x => x.Get(It.IsAny<int>(), It.IsAny<IUserRepository>()))
                .Returns(this.folder);

            var taskRepositoryMock = new Mock<ITaskRepository>();
            taskRepositoryMock
                .Setup(x => x.ListForPredicate(
                    It.Is<Expression<Func<Task, bool>>>(e => e == predicate),
                    It.IsAny<IUserRepository>(),
                    It.IsAny<IFolderRepository>()))
                .Returns(new[] {task})
                .Verifiable();

            var taskGrouper = new TaskGrouper(this.recurrencePredicateRewriter);
            taskGrouper.PredicateBuilt += e => predicate = e;

            var tasks = taskGrouper.LoadForCriterion(
                Criterion.True, this.user, DateTime.Today,
                userRepositoryMock.Object, folderRepositoryMock.Object, taskRepositoryMock.Object);

            Assert.AreEqual(tasks.Single(), task);
            taskRepositoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void TaskGrouper_TaskSatisfiesCriterion_Works()
        {
            // ReSharper disable RedundantLogicalConditionalExpressionOperand

            var userId = this.user.Id;

            var criterion1 = Criterion.True;
            var criterion2 = new SimpleCriterion(Field.StartDate, Operator.Equals, FieldValue.TodaysDate);
            var criterion3 = new SimpleCriterion(Field.State, Operator.In, "NotStarted|InProgress");
            var criterion4 = new ComplexCriterion(criterion2, Conjunction.Or, criterion3);

            Expression<Func<Task, bool>> expected1 = x => (true && x.User.Id == userId) && true;
            Expression<Func<Task, bool>> expected2 =
                x => (x.StartDate == DateTime.Today && x.User.Id == userId) && true;

            Expression<Func<Task, bool>> expected3 =
                x => (new[] {TaskState.NotStarted, TaskState.InProgress}
                          .Contains(x.State) && x.User.Id == userId) && true;

            Expression<Func<Task, bool>> expected4 =
                x => (((x.StartDate == DateTime.Today ||
                        new[] {TaskState.NotStarted, TaskState.InProgress}
                            .Contains(x.State)) && x.User.Id == userId) && true);

            this.TestPredicate(criterion1, expected1);
            this.TestPredicate(criterion2, expected2);
            this.TestPredicate(criterion3, expected3);
            this.TestPredicate(criterion4, expected4);

            // ReSharper restore RedundantLogicalConditionalExpressionOperand
        }

        private void TestPredicate(Criterion criterion, Expression<Func<Task, bool>> expected)
        {
            var task = new Task(this.idGenerator, Guid.NewGuid().ToString(), this.folder, DateTime.Today);
            var taskGrouper = new TaskGrouper(this.recurrencePredicateRewriter);

            Expression<Func<Task, bool>> actual = null;
            taskGrouper.PredicateBuilt += x => actual = x;

            taskGrouper.TaskSatisfiesCriterion(task, criterion, this.user, DateTime.Today);

            var expectedBody = expected.Body;
            var actualBody = actual.Body;

            Assert.IsTrue(expectedBody.IsPracticallyEqualTo(actualBody));
        }
    }
}