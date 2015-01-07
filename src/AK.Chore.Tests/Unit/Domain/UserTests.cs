/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Domain.UserTests
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

using AK.Chore.Domain;
using AK.Chore.Domain.Filters;
using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;

#endregion

namespace AK.Chore.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests for User.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class UserTests : TestBase
    {
        // ReSharper disable ObjectCreationAsStatement

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_Creation_Valid_Nickname_Set()
        {
            const string userName = "akoirala";
            const string nickname = "Master of the Universe";

            var user = this.CreateValidUser(userName, nickname);
            Assert.IsNotNull(user);
            Assert.AreEqual(user.Nickname, nickname);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_Creation_Valid_Key_Set()
        {
            const string userName = "akoirala";
            const string nickname = "Master of the Universe";
            var key = this.userKeyGenerator.GenerateKey(userName);

            var user = this.CreateValidUser(userName, nickname);
            Assert.IsNotNull(user);
            Assert.AreEqual(user.Key, key);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_Creation_Valid_Personal_Folder_Set()
        {
            var user = this.CreateValidUser();

            Assert.IsNotNull(user);
            Assert.IsTrue(user.AreFoldersLoaded);
            Assert.AreEqual(user.Folders.Count, 1);
            Assert.IsNotNull(user.Folders.Single());
            Assert.AreEqual(user.Folders.Single().User, user);
            Assert.AreEqual(user.Folders.Single().Name, "Personal");
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_Creation_Valid_Built_In_Filters_Set()
        {
            var user = this.CreateValidUser();

            Assert.IsNotNull(user);
            Assert.IsTrue(user.AreFiltersLoaded);
            Assert.AreEqual(user.Filters.Count, 5);

            var filters = user.Filters.ToArray();

            Assert.IsTrue(IsFilterMatch(filters[0], user, "Today", this.builtInCriterionProvider.Today));
            Assert.IsTrue(IsFilterMatch(filters[1], user, "This Week", this.builtInCriterionProvider.ThisWeek));
            Assert.IsTrue(IsFilterMatch(filters[2], user, "Completed", this.builtInCriterionProvider.Completed));
            Assert.IsTrue(IsFilterMatch(filters[3], user, "All Not Done", this.builtInCriterionProvider.AllNotCompleted));
            Assert.IsTrue(IsFilterMatch(filters[4], user, "All Recurring", this.builtInCriterionProvider.AllRecurring));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_Creation_Invalid_IdGenerator_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.UserIdGeneratorNotSet,
                () => new User(null, "akoirala", "test", this.userKeyGenerator, this.builtInCriterionProvider));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_Creation_Invalid_KeyGenerator_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.UserKeyGeneratorNotSet,
                () => new User(this.idGenerator, "akoirala", "test", null, this.builtInCriterionProvider));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_Creation_Invalid_BuiltInCriterionProvider_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.UserBuiltInFilterProviderNotSet,
                () => new User(this.idGenerator, "akoirala", "test", this.userKeyGenerator, null));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_Creation_Invalid_UserName_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.UserNameEmpty,
                () => new User(this.idGenerator, null, "test", this.userKeyGenerator, this.builtInCriterionProvider));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_Creation_Invalid_Nickname_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.UserNicknameEmpty,
                () => new User(this.idGenerator, "akoirala", null, this.userKeyGenerator, this.builtInCriterionProvider));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_Creation_Invalid_UserName_Whitespace_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.UserNameEmpty,
                () => new User(this.idGenerator, "   ", "test", this.userKeyGenerator, this.builtInCriterionProvider));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_Creation_Invalid_Nickname_Whitespace_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.UserNicknameEmpty,
                () =>
                new User(this.idGenerator, "akoirala", "   ", this.userKeyGenerator, this.builtInCriterionProvider));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_Nickname_Set_Null_Or_Whitespace_Throws()
        {
            var user = this.CreateValidUser();

            this.TestInvalid(
                DomainValidationErrorCode.UserNicknameEmpty,
                () => user.Nickname = null);

            this.TestInvalid(
                DomainValidationErrorCode.UserNicknameEmpty,
                () => user.Nickname = "   ");
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_AddFolder_Valid_Works()
        {
            var user = this.CreateValidUser();
            var folder = new Folder(this.idGenerator, "My Folder", user);

            Assert.AreEqual(user.Folders.Last(), folder);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_AddFolder_Invalid_Folder_Not_Set_Throws()
        {
            var user = this.CreateValidUser();

            this.TestInvalid(
                DomainValidationErrorCode.UserFolderNotSet,
                () => user.AddFolder(null));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_AddFolder_Invalid_Someone_Elses_Folder_Throws()
        {
            var user = this.CreateValidUser();
            var anotherUser = this.CreateValidUser();
            var folder = new Folder(this.idGenerator, "My Folder", anotherUser);

            this.TestInvalid(
                DomainValidationErrorCode.UserFolderWithInvalidUser,
                () => user.AddFolder(folder));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_AddFolder_Invalid_Existing_Folder_Throws()
        {
            var user = this.CreateValidUser();
            var folder = new Folder(this.idGenerator, "My Folder", user);

            this.TestInvalid(
                DomainValidationErrorCode.UserAttemptToAddExistingFolder,
                () => user.AddFolder(folder));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_AddFolder_Invalid_Folder_With_Same_Name_Throws()
        {
            var user = this.CreateValidUser();
            new Folder(this.idGenerator, "My Folder", user);

            this.TestInvalid(
                DomainValidationErrorCode.UserAttemptToAddExistingFolder,
                () => new Folder(this.idGenerator, "My Folder", user));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_AddFilter_Valid_Works()
        {
            var user = this.CreateValidUser();
            var filter = new Filter(this.idGenerator, "My Filter", user, Criterion.True);

            Assert.AreEqual(user.Filters.Last(), filter);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_AddFilter_Invalid_Someone_Elses_Filter_Throws()
        {
            var user = this.CreateValidUser();
            var anotherUser = this.CreateValidUser();

            var filter = new Filter(this.idGenerator, "My Filter", anotherUser, Criterion.True);

            this.TestInvalid(
                DomainValidationErrorCode.UserFilterWithInvalidUser,
                () => user.AddFilter(filter));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_AddFilter_Invalid_Existing_Filter_Throws()
        {
            var user = this.CreateValidUser();
            var filter = new Filter(this.idGenerator, "My Filter", user, Criterion.True);

            this.TestInvalid(
                DomainValidationErrorCode.UserAttemptToAddExistingFilter,
                () => user.AddFilter(filter));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_AddFilter_Invalid_Filter_With_Same_Name_Throws()
        {
            var user = this.CreateValidUser();
            new Filter(this.idGenerator, "My Filter", user, Criterion.True);

            this.TestInvalid(
                DomainValidationErrorCode.UserAttemptToAddExistingFilter,
                () => new Filter(this.idGenerator, "My Filter", user, Criterion.True));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_RemoveFolder_Valid_Works()
        {
            var user = this.CreateValidUser();
            var folder = user.Folders.First();
            var countBefore = user.Folders.Count;

            user.RemoveFolder(folder);
            Assert.AreEqual(countBefore - 1, user.Folders.Count);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_RemoveFolder_Invalid_Folder_Not_Set_Throws()
        {
            var user = this.CreateValidUser();

            this.TestInvalid(
                DomainValidationErrorCode.UserFolderNotSet,
                () => user.RemoveFolder(null));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_RemoveFolder_Invalid_Someone_Elses_Folder_Throws()
        {
            var user = this.CreateValidUser();
            var anotherUser = this.CreateValidUser();
            var folder = new Folder(this.idGenerator, "My Folder", anotherUser);

            this.TestInvalid(
                DomainValidationErrorCode.UserFolderWithInvalidUser,
                () => user.RemoveFolder(folder));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_RemoveFolder_Invalid_Non_Existent_Folder_Throws()
        {
            var user = this.CreateValidUser();
            var folder = new Folder(this.idGenerator, "My Folder", user);
            user.RemoveFolder(folder);

            this.TestInvalid(
                DomainValidationErrorCode.UserAttemptToRemoveNonExistingFolder,
                () => user.RemoveFolder(folder));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_RemoveFilter_Valid_Works()
        {
            var user = this.CreateValidUser();
            var countBefore = user.Filters.Count;
            var filter = user.Filters.First();

            user.RemoveFilter(filter);
            Assert.AreEqual(countBefore - 1, user.Filters.Count);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_RemoveFilter_Invalid_Filter_Not_Set_Throws()
        {
            var user = this.CreateValidUser();

            this.TestInvalid(
                DomainValidationErrorCode.UserFilterNotSet,
                () => user.RemoveFilter(null));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_RemoveFilter_Invalid_Someone_Elses_Filter_Throws()
        {
            var user = this.CreateValidUser();
            var anotherUser = this.CreateValidUser();
            var filter = new Filter(this.idGenerator, "My Filter", anotherUser, Criterion.True);

            this.TestInvalid(
                DomainValidationErrorCode.UserFilterWithInvalidUser,
                () => user.RemoveFilter(filter));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_RemoveFilter_Invalid_Non_Existent_Filter_Throws()
        {
            var user = this.CreateValidUser();
            var filter = new Filter(this.idGenerator, "My Filter", user, Criterion.True);
            user.RemoveFilter(filter);

            this.TestInvalid(
                DomainValidationErrorCode.UserAttemptToRemoveNonExistingFilter,
                () => user.RemoveFilter(filter));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_LoadFolders_Interaction_Works()
        {
            var user = this.CreateValidUser();
            var folder = new Folder(this.idGenerator, "My Folder", user);
            user.RemoveFolder(folder);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.Get(It.IsAny<int>()))
                .Returns(user)
                .Verifiable();

            var folderRepositoryMock = new Mock<IFolderRepository>();
            folderRepositoryMock
                .Setup(x => x.ListForUser(It.Is<User>(y => y.Equals(user)), It.IsAny<IUserRepository>()))
                .Returns(new[] {folder})
                .Verifiable();

            user.LoadFolders(userRepositoryMock.Object, folderRepositoryMock.Object);

            folderRepositoryMock.Verify();
            Assert.IsTrue(user.AreFoldersLoaded);
            Assert.AreEqual(folder, user.Folders.Last());
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void User_LoadFilters_Interaction_Works()
        {
            var user = this.CreateValidUser();
            var filter = new Filter(this.idGenerator, "My Filter", user, Criterion.True);
            user.RemoveFilter(filter);

            var filterRepositoryMock = new Mock<IFilterRepository>();
            filterRepositoryMock
                .Setup(x => x.ListForUser(It.Is<User>(y => y.Equals(user))))
                .Returns(new[] {filter})
                .Verifiable();

            user.LoadFilters(filterRepositoryMock.Object);

            filterRepositoryMock.Verify();
            Assert.IsTrue(user.AreFiltersLoaded);
            Assert.AreEqual(filter, user.Filters.Last());
        }

        private User CreateValidUser()
        {
            const string userName = "akoirala";
            const string nickname = "Master of the Universe";

            return this.CreateValidUser(userName, nickname);
        }

        private User CreateValidUser(string userName, string nickname)
        {
            return new User(this.idGenerator, userName, nickname, this.userKeyGenerator, this.builtInCriterionProvider);
        }

        private static bool IsFilterMatch(
            Filter filter, User expectedUser, string expectedName, Criterion expectedCriterion)
        {
            if (!filter.User.Equals(expectedUser)) return false;
            return filter.Name == expectedName && filter.Criterion.Equals(expectedCriterion);
        }

        // ReSharper restore ObjectCreationAsStatement
    }
}