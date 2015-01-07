/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Domain.TaskTests
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
using AK.Chore.Domain;
using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Tasks;
using AK.Chore.Domain.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace AK.Chore.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests for Task.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class TaskTests : TestBase
    {
        private User user;
        private Folder folder;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            this.user = new User(this.idGenerator, "akoirala", "test", this.userKeyGenerator,
                                 this.builtInCriterionProvider);
            this.folder = new Folder(this.idGenerator, "My Folder", this.user);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Creation_Valid_EndDate_Works()
        {
            var task = new Task(this.idGenerator, "Test 1", this.folder, DateTime.Today);
            Assert.IsNotNull(task);
            Assert.AreEqual(task.State, TaskState.NotStarted);

            task = new Task(this.idGenerator, "Test 2", this.folder, DateTime.Today, TimeSpan.FromMinutes(5));
            Assert.IsNotNull(task);
            Assert.AreEqual(task.State, TaskState.NotStarted);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Creation_Valid_EndDate_StartDate_Works()
        {
            var task = new Task(this.idGenerator, "Test 1", this.folder, DateTime.Today, DateTime.Today.AddDays(-1));
            Assert.IsNotNull(task);
            Assert.AreEqual(task.State, TaskState.NotStarted);

            task = new Task(this.idGenerator, "Test 2", this.folder, DateTime.Today, DateTime.Today.AddDays(-1),
                            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10));
            Assert.IsNotNull(task);
            Assert.AreEqual(task.State, TaskState.NotStarted);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Creation_Valid_Recurring_Works()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, Recurrence.Hourly(1));
            Assert.IsNotNull(task);
            Assert.AreEqual(task.State, TaskState.Recurring);
            Assert.IsTrue(task.IsRecurring);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_IsLate_Valid_Works()
        {
            var onTimeTasks = new[]
                {
                    new Task(this.idGenerator, "On Time 1", this.folder, DateTime.Today.AddDays(1)),
                    new Task(this.idGenerator, "On Time 2", this.folder, DateTime.Today.AddDays(2),
                             DateTime.Today.AddDays(1)),
                    new Task(this.idGenerator, "On Time 3", this.folder, Recurrence.Hourly(1)),
                    new Task(this.idGenerator, "On Time 4", this.folder, DateTime.Today.AddDays(1),
                             DateTime.Today.AddDays(-1)),
                    new Task(this.idGenerator, "On Time 5", this.folder, DateTime.Today.AddDays(-1))
                };

            onTimeTasks[3].Start();
            onTimeTasks[4].Complete();

            foreach (var task in onTimeTasks) Assert.IsFalse(task.IsLate(DateTime.Now));

            var lateTasks = new[]
                {
                    new Task(this.idGenerator, "Late 1", this.folder, DateTime.Today.AddDays(1),
                             DateTime.Today.AddDays(-1)),
                    new Task(this.idGenerator, "Late 2", this.folder, DateTime.Today.AddDays(-1)),
                    new Task(this.idGenerator, "Late 3", this.folder, DateTime.Today.AddDays(-1),
                             DateTime.Today.AddDays(-1))
                };

            lateTasks[2].Start();

            foreach (var task in lateTasks) Assert.IsTrue(task.IsLate(DateTime.Now));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Start_Valid_Works()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today.AddDays(1), DateTime.Today);
            task.Start();
            Assert.AreEqual(task.State, TaskState.InProgress);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Pause_Valid_Works()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today.AddDays(1), DateTime.Today);
            task.Start();
            task.Pause();
            Assert.AreEqual(task.State, TaskState.Paused);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Resume_Valid_Works()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today.AddDays(1), DateTime.Today);
            task.Start();
            task.Pause();
            task.Resume();
            Assert.AreEqual(task.State, TaskState.InProgress);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Complete_Valid_Works()
        {
            var task = new Task(this.idGenerator, "Test 1", this.folder, DateTime.Today.AddDays(1), DateTime.Today);
            task.Start();
            task.Complete();
            Assert.AreEqual(task.State, TaskState.Completed);

            task = new Task(this.idGenerator, "Test 2", this.folder, DateTime.Today.AddDays(1));
            task.Complete();
            Assert.AreEqual(task.State, TaskState.Completed);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_EnableRecurrence_Valid_Works()
        {
            var enabledRecurrence = Recurrence.Hourly(1);
            var disabledRecurrence = Recurrence.Disabled(Recurrence.Hourly(1));

            var task = new Task(this.idGenerator, "Test", this.folder, disabledRecurrence);
            task.EnableRecurrence();

            Assert.AreEqual(task.Recurrence, enabledRecurrence);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_DisableRecurrence_Valid_Works()
        {
            var enabledRecurrence = Recurrence.Hourly(1);
            var disabledRecurrence = Recurrence.Disabled(Recurrence.Hourly(1));

            var task = new Task(this.idGenerator, "Test", this.folder, enabledRecurrence);
            task.DisableRecurrence();

            Assert.AreEqual(task.Recurrence, disabledRecurrence);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_MoveTo_Valid_Works()
        {
            var anotherFolder = new Folder(this.idGenerator, "New Folder", this.user);
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today);

            task.MoveTo(anotherFolder);
            Assert.AreEqual(task.Folder, anotherFolder);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Creation_Invalid_IdGenerator_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.TaskIdGeneratorNotSet,
                () => new Task(null, "test", this.folder, DateTime.Today));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Creation_Invalid_Folder_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.TaskFolderNotSet,
                () => new Task(this.idGenerator, "test", null, DateTime.Today));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Creation_Invalid_Description_Not_Set_Or_Whitespace_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.TaskDescriptionEmpty,
                () => new Task(this.idGenerator, null, this.folder, DateTime.Today));

            this.TestInvalid(
                DomainValidationErrorCode.TaskDescriptionEmpty,
                () => new Task(this.idGenerator, "    ", this.folder, DateTime.Today));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Creation_Invalid_Start_Date_Later_Than_End_Date_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.TaskStartDateLaterThanEndDate,
                () => new Task(this.idGenerator, "Test1", this.folder, DateTime.Today, DateTime.Today.AddDays(1)));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Description_Invalid_Not_Set_Or_Whitespace_Throws()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today);

            this.TestInvalid(
                DomainValidationErrorCode.TaskDescriptionEmpty,
                () => task.Description = null);

            this.TestInvalid(
                DomainValidationErrorCode.TaskDescriptionEmpty,
                () => task.Description = "  ");
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Dates_Invalid_Bad_Combination_Throws()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today);

            this.TestInvalid(
                DomainValidationErrorCode.TaskStartDateLaterThanEndDate,
                () => task.StartDate = DateTime.Today.AddDays(1));

            task.StartDate = null;

            this.TestInvalid(
                DomainValidationErrorCode.TaskStartTimeSetWithoutStartDate,
                () => task.StartTime = TimeSpan.FromHours(4));

            this.TestInvalid(
                DomainValidationErrorCode.TaskNonRecurringWithoutEndDate,
                () => task.EndDate = null);

            task.Recurrence = Recurrence.Hourly(1);
            task.EndDate = null;

            this.TestInvalid(
                DomainValidationErrorCode.TaskEndTimeSetWithoutEndDate,
                () => task.EndTime = TimeSpan.FromHours(4));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_State_Recurrence_Bad_Combination_Is_Prevented()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today);
            Assert.AreNotEqual(task.State, TaskState.Recurring);

            task.Recurrence = Recurrence.Hourly(1);
            Assert.AreEqual(task.State, TaskState.Recurring);
            Assert.IsTrue(task.IsRecurring);

            task.Recurrence = Recurrence.NonRecurring();
            Assert.AreNotEqual(task.State, TaskState.Recurring);
            Assert.IsFalse(task.IsRecurring);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_IsMundane_Recurrence_Invalid_Bad_Combination_Throws()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today);

            this.TestInvalid(
                DomainValidationErrorCode.TaskNonRecurringCannotBeMundane,
                () => task.IsMundane = true);

            Assert.IsFalse(task.IsMundane);

            task.Recurrence = Recurrence.Hourly(1);
            task.IsMundane = true;
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Start_Invalid_Throws()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today);
            Assert.IsFalse(task.CanStart);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Start);

            task.Recurrence = Recurrence.Hourly(1);
            Assert.IsFalse(task.CanStart);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Start);

            task.Recurrence = Recurrence.NonRecurring();
            task.StartDate = DateTime.Today.AddDays(-1);
            task.Start();
            Assert.IsFalse(task.CanStart);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Start);

            task.Pause();
            Assert.IsFalse(task.CanStart);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Start);

            task.Resume();
            task.Complete();
            Assert.IsFalse(task.CanStart);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Start);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Pause_Invalid_Throws()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today);
            Assert.IsFalse(task.CanPause);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Pause);

            task.Recurrence = Recurrence.Hourly(1);
            Assert.IsFalse(task.CanPause);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Pause);

            task.Recurrence = Recurrence.NonRecurring();
            task.StartDate = DateTime.Today.AddDays(-1);
            Assert.IsFalse(task.CanPause);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Pause);

            task.Start();
            task.Complete();
            Assert.IsFalse(task.CanPause);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Pause);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Resume_Invalid_Throws()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today);
            Assert.IsFalse(task.CanResume);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Resume);

            task.Recurrence = Recurrence.Hourly(1);
            Assert.IsFalse(task.CanResume);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Resume);

            task.Recurrence = Recurrence.NonRecurring();
            task.StartDate = DateTime.Today.AddDays(-1);
            Assert.IsFalse(task.CanResume);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Resume);

            task.Start();
            Assert.IsFalse(task.CanResume);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Resume);

            task.Complete();
            Assert.IsFalse(task.CanResume);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Resume);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_Complete_Invalid_Throws()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, Recurrence.Hourly(1));
            Assert.IsFalse(task.CanComplete);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Complete);

            task.Recurrence = Recurrence.NonRecurring();
            task.EndDate = DateTime.Today;
            task.StartDate = DateTime.Today.AddDays(-1);
            Assert.IsFalse(task.CanComplete);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Complete);

            task.Start();
            Assert.IsTrue(task.CanComplete);

            task.Pause();
            Assert.IsFalse(task.CanComplete);
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Complete);

            task.Resume();
            Assert.IsTrue(task.CanComplete);

            task.Complete();
            this.TestInvalid(DomainValidationErrorCode.TaskInvalidStateForOperation, task.Complete);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_EnableRecurrence_Invalid_Throws()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today);
            this.TestInvalid(DomainValidationErrorCode.TaskRecurringOperationOnNonRecurringTask, task.EnableRecurrence);

            task.Recurrence = Recurrence.Hourly(1);
            this.TestInvalid(DomainValidationErrorCode.TaskRecurringOperationOnNonRecurringTask, task.EnableRecurrence);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_DisableRecurrence_Invalid_Throws()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today);
            this.TestInvalid(DomainValidationErrorCode.TaskRecurringOperationOnNonRecurringTask, task.DisableRecurrence);

            task.Recurrence = Recurrence.Disabled(Recurrence.Hourly(1));
            this.TestInvalid(DomainValidationErrorCode.TaskRecurringOperationOnNonRecurringTask, task.DisableRecurrence);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Task_MoveTo_Invalid_Someone_Elses_Folder_Throws()
        {
            var task = new Task(this.idGenerator, "Test", this.folder, DateTime.Today);

            var anotherUser = new User(this.idGenerator, "Test", "Test", this.userKeyGenerator,
                                       this.builtInCriterionProvider);
            var anotherFolder = new Folder(this.idGenerator, "Another Folder", anotherUser);

            this.TestInvalid(
                DomainValidationErrorCode.TaskAttemptToMoveToAnotherUsersFolder,
                () => task.MoveTo(anotherFolder));
        }
    }
}