/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.UserProfileServiceTests
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
using AK.Chore.Contracts.UserProfile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for UserProfileService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class UserProfileServiceTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void UserProfileService_GetUserByUserName_Interaction_Works()
        {
            var userProfileService = this.GetTarget();

            var result = userProfileService.GetUserByUserName("TestName");

            Assert.IsTrue(result.IsSuccess);

            this.userKeyGeneratorMock.Verify();
            this.userRepositoryMock.Verify(x => x.GetByKey("TestKey"));
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void UserProfileService_CreateUser_Interaction_Works()
        {
            var userProfileService = this.GetTarget();

            var result = userProfileService.CreateUser("NewTestName", "TestNickName");

            Assert.IsTrue(result.IsSuccess);

            this.userKeyGeneratorMock.Verify(x => x.GenerateKey("NewTestName"));

            this.userRepositoryMock
                .Verify(x => x.Save(It.Is<Chore.Domain.Users.User>(y => y.Nickname == "TestNickName")));

            this.folderRepositoryMock
                .Verify(x => x.Save(It.Is<Chore.Domain.Folders.Folder>(y => y.User.Nickname == "TestNickName")),
                        Times.Once());

            this.filterRepositoryMock
                .Verify(x => x.Save(It.Is<Chore.Domain.Filters.Filter>(y => y.User.Nickname == "TestNickName")),
                        Times.AtLeast(2));
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void UserProfileService_UpdateNickName_Interaction_Works()
        {
            var userProfileService = this.GetTarget();

            var testUser = new User {Id = UserId, Nickname = "NewNickName"};
            var result = userProfileService.UpdateNickname(testUser, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.userRepositoryMock
                .Verify(x => x.Save(It.Is<Chore.Domain.Users.User>(y => y.Nickname == "NewNickName")));
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void UserProfileService_DeleteUserProfile_Interaction_Works()
        {
            var userProfileService = this.GetTarget();

            var testUser = new User {Id = UserId};
            var result = userProfileService.DeleteUserProfile(testUser, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.userRepositoryMock.Verify(x => x.Delete(this.user));
        }

        private IUserProfileService GetTarget()
        {
            return new UserProfileService(
                this.userKeyGeneratorMock.Object,
                this.builtInCriterionProviderMock.Object,
                this.dataAccessMock.Object,
                this.configMock.Object,
                this.logger,
                this.entityIdGeneratorProviderSourceMock.Object);
        }
    }
}