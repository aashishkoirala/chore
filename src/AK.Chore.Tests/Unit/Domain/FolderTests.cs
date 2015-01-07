/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Domain.FolderTests
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
using System.Linq;
using System.Linq.Expressions;
using AK.Chore.Domain;
using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Tasks;
using AK.Chore.Domain.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

#endregion

namespace AK.Chore.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests for Folder.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class FolderTests : TestBase
    {
        private User user;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            this.user = new User(this.idGenerator, "akoirala", "test", this.userKeyGenerator,
                                 this.builtInCriterionProvider);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_Creation_Valid_With_User_Works()
        {
            var folder = new Folder(this.idGenerator, "My Folder", this.user);
            Assert.IsNotNull(folder);
            Assert.AreEqual(folder.Name, "My Folder");
            Assert.IsTrue(this.user.Folders.Contains(folder));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_Creation_Valid_With_Parent_Works()
        {
            var parent = new Folder(this.idGenerator, "My Parent Folder", this.user);
            var folder = new Folder(this.idGenerator, "My Folder", parent);
            Assert.IsNotNull(folder);
            Assert.AreEqual(folder.Name, "My Folder");
            Assert.IsTrue(parent.Folders.Contains(folder));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_Creation_Invalid_IdGenerator_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.FolderIdGeneratorNotSet,
                () => new Folder(null, "My Folder", this.user));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_Creation_Invalid_Name_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.FolderNameEmpty,
                () => new Folder(this.idGenerator, null, this.user));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_Creation_Invalid_Name_Whitespace_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.FolderNameEmpty,
                () => new Folder(this.idGenerator, "   ", this.user));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_Creation_Invalid_User_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.FolderUserNotSet,
                () => new Folder(this.idGenerator, "My Folder", (User) null));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_Creation_Invalid_Parent_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.FolderParentNotSet,
                () => new Folder(this.idGenerator, "My Folder", (Folder) null));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_Name_Invalid_Null_Or_Whitespace_Throws()
        {
            var folder = this.CreateValidFolder();

            this.TestInvalid(
                DomainValidationErrorCode.FolderNameEmpty,
                () => folder.Name = null);

            this.TestInvalid(
                DomainValidationErrorCode.FolderNameEmpty,
                () => folder.Name = "   ");
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_LoadFolders_Interaction_Works()
        {
            var folder = this.CreateValidFolder();
            var childFolder = this.CreateValidFolder("Child");

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.Get(It.IsAny<int>()))
                .Returns(this.user);

            var folderRepositoryMock = new Mock<IFolderRepository>();
            folderRepositoryMock
                .Setup(x => x.ListForFolder(It.Is<Folder>(y => y.Equals(folder)), It.IsAny<IUserRepository>()))
                .Returns(new[] {childFolder})
                .Verifiable();

            folder.LoadFolders(userRepositoryMock.Object, folderRepositoryMock.Object);

            folderRepositoryMock.Verify();
            Assert.IsTrue(folder.AreFoldersLoaded);
            Assert.AreEqual(folder.Folders.Single(), childFolder);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_LoadTasks_Interaction_Works()
        {
            var folder = this.CreateValidFolder();
            var task = new Task(this.idGenerator, "Test", folder, Recurrence.Hourly(1));
            folder.RemoveTask(task);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.Get(It.IsAny<int>()))
                .Returns(this.user);

            var folderRepositoryMock = new Mock<IFolderRepository>();
            folderRepositoryMock
                .Setup(x => x.Get(It.IsAny<int>(), It.IsAny<IUserRepository>()))
                .Returns(folder);

            var taskRepositoryMock = new Mock<ITaskRepository>();
            taskRepositoryMock
                .Setup(x => x.ListForPredicate(
                    It.IsAny<Expression<Func<Task, bool>>>(),
                    It.IsAny<IUserRepository>(),
                    It.IsAny<IFolderRepository>()))
                .Returns(new[] {task})
                .Verifiable();

            var taskGrouper = new TaskGrouper(this.recurrencePredicateRewriter);

            folder.LoadTasks(taskGrouper, userRepositoryMock.Object, folderRepositoryMock.Object,
                             taskRepositoryMock.Object);

            taskRepositoryMock.Verify();
            Assert.IsTrue(folder.AreTasksLoaded);
            Assert.AreEqual(folder.Tasks.Single(), task);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_AddTask_Valid_Works()
        {
            var folder = this.CreateValidFolder();
            var task = new Task(this.idGenerator, "Test", folder, Recurrence.Hourly(1));

            Assert.AreEqual(folder.Tasks.Count, 1);
            Assert.AreEqual(folder.Tasks.Single(), task);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_AddTask_Invalid_Task_Not_Set_Throws()
        {
            var folder = this.CreateValidFolder();

            this.TestInvalid(
                DomainValidationErrorCode.FolderTaskNotSet,
                () => folder.AddTask(null));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_AddTask_Invalid_Someone_Elses_Task_Throws()
        {
            var folder = this.CreateValidFolder();
            var anotherFolder = this.CreateValidFolder("Another");
            var task = new Task(this.idGenerator, "Test", anotherFolder, Recurrence.Hourly(1));

            this.TestInvalid(
                DomainValidationErrorCode.FolderTaskWithInvalidFolder,
                () => folder.AddTask(task));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_AddTask_Invalid_Existing_Task_Throws()
        {
            var folder = this.CreateValidFolder();
            var task = new Task(this.idGenerator, "Test", folder, Recurrence.Hourly(1));

            this.TestInvalid(
                DomainValidationErrorCode.FolderAttemptToAddExistingTask,
                () => folder.AddTask(task));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_RemoveTask_Invalid_Task_Not_Set_Throws()
        {
            var folder = this.CreateValidFolder();

            this.TestInvalid(
                DomainValidationErrorCode.FolderTaskNotSet,
                () => folder.RemoveTask(null));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_RemoveTask_Invalid_Someone_Elses_Task_Throws()
        {
            var folder = this.CreateValidFolder();
            var anotherFolder = this.CreateValidFolder("Another");
            var task = new Task(this.idGenerator, "Test", anotherFolder, Recurrence.Hourly(1));

            this.TestInvalid(
                DomainValidationErrorCode.FolderTaskWithInvalidFolder,
                () => folder.RemoveTask(task));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_RemoveTask_Invalid_Non_Existent_Task_Throws()
        {
            var folder = this.CreateValidFolder();
            var task = new Task(this.idGenerator, "Test", folder, Recurrence.Hourly(1));
            folder.RemoveTask(task);

            this.TestInvalid(
                DomainValidationErrorCode.FolderAttemptToRemoveNonExistingTask,
                () => folder.RemoveTask(task));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_AddFolder_Valid_Works()
        {
            var folder = this.CreateValidFolder();
            var child = new Folder(this.idGenerator, "Child", folder);

            Assert.AreEqual(folder.Folders.Count, 1);
            Assert.AreEqual(folder.Folders.Single(), child);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_AddFolder_Invalid_Folder_Not_Set_Throws()
        {
            var folder = this.CreateValidFolder();

            this.TestInvalid(
                DomainValidationErrorCode.FolderFolderNotSet,
                () => folder.AddFolder(null));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_AddFolder_Invalid_Someone_Elses_Folder_Throws()
        {
            var folder = this.CreateValidFolder();
            var anotherUser = new User(this.idGenerator, "Test", "Test", this.userKeyGenerator,
                                       this.builtInCriterionProvider);
            var anotherUsersFolder = new Folder(this.idGenerator, "Test", anotherUser);

            this.TestInvalid(
                DomainValidationErrorCode.FolderFolderWithInvalidParent,
                () => folder.AddFolder(anotherUsersFolder));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_AddFolder_Invalid_Existing_Folder_Throws()
        {
            var folder = this.CreateValidFolder();
            var child = new Folder(this.idGenerator, "Test", folder);

            this.TestInvalid(
                DomainValidationErrorCode.FolderAttemptToAddExistingFolder,
                () => folder.AddFolder(child));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_RemoveFolder_Invalid_Folder_Not_Set_Throws()
        {
            var folder = this.CreateValidFolder();

            this.TestInvalid(
                DomainValidationErrorCode.FolderFolderNotSet,
                () => folder.RemoveFolder(null));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_RemoveFolder_Invalid_Someone_Elses_Folder_Throws()
        {
            var folder = this.CreateValidFolder();
            var anotherUser = new User(this.idGenerator, "Test", "Test", this.userKeyGenerator,
                                       this.builtInCriterionProvider);
            var anotherUsersFolder = new Folder(this.idGenerator, "Test", anotherUser);

            this.TestInvalid(
                DomainValidationErrorCode.FolderFolderWithInvalidParent,
                () => folder.RemoveFolder(anotherUsersFolder));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_RemoveFolder_Invalid_Non_Existent_Folder_Throws()
        {
            var folder = this.CreateValidFolder();
            var child = new Folder(this.idGenerator, "Test", folder);
            folder.RemoveFolder(child);

            this.TestInvalid(
                DomainValidationErrorCode.FolderFolderWithInvalidParent,
                () => folder.RemoveFolder(child));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_ListHierarchy_Works()
        {
            var folder = this.CreateValidFolder();
            var child = new Folder(this.idGenerator, "Child", folder);
            var secondChild = new Folder(this.idGenerator, "Second Child", folder);
            var grandChild = new Folder(this.idGenerator, "Grandchild", child);

            var list = folder.ListHierarchy();
            Assert.AreEqual(list.Count, 4);
            Assert.IsTrue(list.Contains(folder));
            Assert.IsTrue(list.Contains(child));
            Assert.IsTrue(list.Contains(secondChild));
            Assert.IsTrue(list.Contains(grandChild));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_MoveTo_Valid_Works()
        {
            var folder1 = this.CreateValidFolder();
            var folder2 = this.CreateValidFolder("Folder2");
            var folder3 = this.CreateValidFolder("Folder3");

            folder2.MoveTo(folder1);
            Assert.AreEqual(folder2.Parent, folder1);

            folder2.MoveTo(folder3);
            Assert.AreEqual(folder2.Parent, folder3);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Folder_MoveTo_Invalid_Someone_Elses_Folder_Throws()
        {
            var folder = this.CreateValidFolder();
            var anotherUser = new User(this.idGenerator, "Test", "Test", this.userKeyGenerator,
                                       this.builtInCriterionProvider);

            var anotherUsersFolder = new Folder(this.idGenerator, "Test", anotherUser);

            this.TestInvalid(DomainValidationErrorCode.FolderAttemptToMoveToAnotherUsersFolder,
                             () => folder.MoveTo(anotherUsersFolder));
        }

        private Folder CreateValidFolder(string name = "My Folder")
        {
            return new Folder(this.idGenerator, name, this.user);
        }
    }
}