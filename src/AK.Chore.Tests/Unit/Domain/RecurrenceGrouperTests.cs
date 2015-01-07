/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Domain.RecurrenceGrouperTests
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

using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Tasks;
using AK.Chore.Domain.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

#endregion

namespace AK.Chore.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests for RecurrenceGrouper.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class RecurrenceGrouperTests : TestBase
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
        public void RecurrenceGrouper_BuildPredicate_Equal_Works()
        {
            var expression = this.recurrenceGrouper.BuildPredicate(DateTime.Today);
            expression.Compile();
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void RecurrenceGrouper_BuildPredicate_In_Works()
        {
            var expression = this.recurrenceGrouper.BuildPredicate(
                new[]
                    {
                        DateTime.Today,
                        DateTime.Today.AddDays(7),
                        DateTime.Today.AddDays(14)
                    });
            expression.Compile();
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void RecurrenceGrouper_BuildPredicate_Before_After_Works()
        {
            var expression = this.recurrenceGrouper.BuildPredicate(DateTime.Today, DateTime.Today.AddDays(-7));
            expression.Compile();
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void RecurrenceGrouper_TaskSatisfiesRecurrence_Equal_Works_Positive()
        {
            var recurrenceDateEqualTo = DateTime.Today.AddDays(2);
            var task = new Task(this.idGenerator, "Test", this.folder, Recurrence.Daily(2));
            var result = this.recurrenceGrouper.TaskSatisfiesRecurrence(task, recurrenceDateEqualTo);
            Assert.IsTrue(result);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void RecurrenceGrouper_TaskSatisfiesRecurrence_Equal_Works_Negative()
        {
            var time = DateTime.Today.AddDays(3);
            var task = new Task(this.idGenerator, "Test", this.folder, Recurrence.Daily(2));
            var result = this.recurrenceGrouper.TaskSatisfiesRecurrence(task, time);
            Assert.IsFalse(result);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void RecurrenceGrouper_TaskSatisfiesRecurrence_In_Works_Positive()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, Recurrence.Daily(2));
            var result = this.recurrenceGrouper.TaskSatisfiesRecurrence(
                task,
                new[]
                    {
                        DateTime.Today,
                        DateTime.Today.AddDays(2),
                        DateTime.Today.AddDays(4)
                    });
            Assert.IsTrue(result);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void RecurrenceGrouper_TaskSatisfiesRecurrence_In_Works_Negative()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, Recurrence.Daily(5));
            var result = this.recurrenceGrouper.TaskSatisfiesRecurrence(
                task,
                new[]
                    {
                        DateTime.Today.AddDays(2),
                        DateTime.Today.AddDays(4),
                        DateTime.Today.AddDays(6)
                    });
            Assert.IsFalse(result);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void RecurrenceGrouper_TaskSatisfiesRecurrence_Before_After_Works_Positive()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, Recurrence.Daily(15));
            var result = this.recurrenceGrouper.TaskSatisfiesRecurrence(
                task, DateTime.Today.AddDays(1), DateTime.Today.AddDays(20));

            Assert.IsTrue(result);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void RecurrenceGrouper_TaskSatisfiesRecurrence_Before_After_Works_Negative()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, Recurrence.Daily(15));
            var result = this.recurrenceGrouper.TaskSatisfiesRecurrence(
                task, DateTime.Today.AddDays(10), DateTime.Today.AddDays(1));

            Assert.IsFalse(result);
        }
    }
}