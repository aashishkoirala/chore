/*******************************************************************************************************************************
 * AK.Chore.Tests.Integration.Infrastructure.TaskRepositoryTests
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
using AK.Commons.DomainDriven;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace AK.Chore.Tests.Integration.Infrastructure
{
    /// <summary>
    /// Integration tests for TaskRepository.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass, Ignore]
    public class TaskRepositoryTests : TestBase
    {
        private User user;
        private Folder folder;
        private static readonly object userAndFolderLock = new object();

        [ClassInitialize]
        public static void FixtureInitialize(TestContext testContext)
        {
            Initializer.Initialize();
        }

        [ClassCleanup]
        public static void FixtureCleanup()
        {
            Initializer.ShutDown();
        }

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            if (this.user != null && this.folder != null) return;

            lock (userAndFolderLock)
            {
                if (this.user != null && this.folder != null) return;

                var newUser = new User(this.IdGenerator, "UserName", "Nickname", this.DomainServices.UserKeyGenerator,
                                       this.DomainServices.BuiltInCriterionProvider);
                var newFolder = new Folder(this.IdGenerator, "Folder", newUser);

                this.Db
                    .With<IUserRepository>()
                    .With<IFolderRepository>()
                    .Execute(map =>
                        {
                            map.Get<IUserRepository>().Save(newUser);
                            map.Get<IFolderRepository>().Save(newFolder);
                        });

                this.user = newUser;
                this.folder = newFolder;
            }
        }

        private void Execute(Action<IUserRepository, IFolderRepository, ITaskRepository> action)
        {
            this.Db
                .With<IUserRepository>()
                .With<IFolderRepository>()
                .With<ITaskRepository>()
                .Execute(map => action(
                    map.Get<IUserRepository>(),
                    map.Get<IFolderRepository>(),
                    map.Get<ITaskRepository>()));
        }

        [TestMethod, TestCategory("Integration.Infrastructure")]
        public void TaskRepository_Sanity_Check()
        {
            // ReSharper disable ImplicitlyCapturedClosure

            var newTask = new Task(this.IdGenerator, "Description", this.folder, DateTime.Today);

            Task task = null;
            this.Execute((u, f, t) => task = t.Get(newTask.Id, u, f));
            Assert.IsNull(task);

            this.Execute((u, f, t) => t.Save(newTask));
            this.Execute((u, f, t) => task = t.Get(newTask.Id, u, f));
            Assert.IsNotNull(task);
            Assert.AreEqual(task, newTask);

            newTask.Recurrence = Recurrence.Hourly(5, new TimeSpan(2, 0, 0));
            task.Recurrence = Recurrence.Hourly(5, new TimeSpan(2, 0, 0));
            this.Execute((u, f, t) => t.Save(newTask));
            this.Execute((u, f, t) => task = t.Get(newTask.Id, u, f));
            Assert.IsNotNull(task);
            Assert.AreEqual(task.Recurrence, newTask.Recurrence);

            this.Execute((u, f, t) => t.Delete(task));
            this.Execute((u, f, t) => task = t.Get(task.Id, u, f));
            Assert.IsNull(task);

            // ReSharper restore ImplicitlyCapturedClosure
        }

        [TestMethod, TestCategory("Integration.Infrastructure")]
        public void TaskRepository_ListForPredicate_Works()
        {
            // This test WILL fail locally - see comments in TaskRepository.Map().

            var allTasks = new[]
                {
                    new Task(this.IdGenerator, "First Task", this.folder, DateTime.Today),
                    new Task(this.IdGenerator, "Second Task", this.folder, DateTime.Today, TimeSpan.FromHours(5)),
                    new Task(this.IdGenerator, "Third Task", this.folder, DateTime.Today, DateTime.Today.AddDays(-4)),
                    new Task(this.IdGenerator, "Fourth Task", this.folder, DateTime.Today, DateTime.Today.AddDays(-4)),
                    new Task(this.IdGenerator, "Fifth Task", this.folder, Recurrence.Daily(2))
                };
            allTasks[3].Start();

            this.Execute((u, f, t) =>
                {
                    foreach (var task in allTasks) t.Save(task);
                });

            var recurrenceGrouper = this.DomainServices.RecurrenceGrouper;

            Expression<Func<Task, bool>> predicate1 =
                x => x.EndDate == DateTime.Today && x.Description.StartsWith("First");
            Expression<Func<Task, bool>> predicate2 =
                x => x.EndDate == DateTime.Today && x.EndTime == TimeSpan.FromHours(5);
            Expression<Func<Task, bool>> predicate3 =
                x => x.EndDate == DateTime.Today && x.StartDate == DateTime.Today.AddDays(-4);
            Expression<Func<Task, bool>> predicate4 =
                x => x.EndDate == DateTime.Today && x.State == TaskState.InProgress;
            Expression<Func<Task, bool>> predicate5 =
                x => recurrenceGrouper.TaskSatisfiesRecurrence(x, DateTime.Today);

            IReadOnlyCollection<Task> tasks1 = null, tasks2 = null, tasks3 = null, tasks4 = null, tasks5 = null;

            this.Execute((u, f, t) =>
                {
                    tasks1 = t.ListForPredicate(predicate1, u, f);
                    tasks2 = t.ListForPredicate(predicate2, u, f);
                    tasks3 = t.ListForPredicate(predicate3, u, f);
                    tasks4 = t.ListForPredicate(predicate4, u, f);
                    tasks5 = t.ListForPredicate(predicate5, u, f);
                });

            Assert.AreEqual(tasks1.Single(), allTasks[0]);
            Assert.AreEqual(tasks2.Single(), allTasks[1]);
            Assert.AreEqual(tasks3.First(), allTasks[2]);
            Assert.AreEqual(tasks3.Last(), allTasks[3]);
            Assert.AreEqual(tasks4.Single(), allTasks[3]);
            Assert.AreEqual(tasks5.Single(), allTasks[4]);
        }
    }
}