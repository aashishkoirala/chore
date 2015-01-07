/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.LoginServiceTests
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
using AK.Commons.Security;
using AK.Commons.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for LoginService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class LoginServiceTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void LoginService_GetLoginSplashInfo_Interaction_Works()
        {
            var userProfileServiceMock = new Mock<IUserProfileService>();

            var loginService = new LoginService
                {
                    UserProfileServiceOverride = userProfileServiceMock.Object,
                    RequestUriOverride = new Uri("http://www.test.com/")
                };

            var splashInfo = loginService.GetLoginSplashInfo();
            Assert.IsNotNull(splashInfo);
            Assert.AreEqual("Chore", splashInfo.ApplicationName);
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void LoginService_GetUser_Interaction_Works()
        {
            const string userName = "TestUser";

            var userProfileServiceMock = new Mock<IUserProfileService>();
            userProfileServiceMock
                .Setup(x => x.GetUserByUserName(userName))
                .Returns(new OperationResult<User>(new User {Nickname = "Nickname"}))
                .Verifiable();

            var loginService = new LoginService {UserProfileServiceOverride = userProfileServiceMock.Object};

            var userInfo = loginService.GetUser(userName);
            Assert.IsTrue(userInfo.UserExists);
            Assert.AreEqual(userName, userInfo.UserName);

            userProfileServiceMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void LoginService_CreateUser_Interaction_Works()
        {
            const string userName = "TestUser";

            var userProfileServiceMock = new Mock<IUserProfileService>();
            userProfileServiceMock
                .Setup(x => x.CreateUser(userName, It.IsAny<string>()))
                .Returns(new OperationResult<User>(new User {Nickname = "Nickname"}))
                .Verifiable();

            var loginService = new LoginService {UserProfileServiceOverride = userProfileServiceMock.Object};

            var userInfo = loginService.CreateUser(new LoginUserInfo {UserName = userName});
            Assert.IsTrue(userInfo.UserExists);
            Assert.AreEqual(userName, userInfo.UserName);

            userProfileServiceMock.Verify();
        }
    }
}