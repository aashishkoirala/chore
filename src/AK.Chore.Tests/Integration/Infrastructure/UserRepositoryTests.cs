/*******************************************************************************************************************************
 * AK.Chore.Tests.Integration.Infrastructure.UserRepositoryTests
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

using AK.Chore.Domain.Users;
using AK.Commons.DomainDriven;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

#endregion

namespace AK.Chore.Tests.Integration.Infrastructure
{
    /// <summary>
    /// Integration tests for UserRepository.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass, Ignore]
    public class UserRepositoryTests : TestBase
    {
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
        }

        private void Execute(Action<IUserRepository> action)
        {
            this.Db
                .With<IUserRepository>()
                .Execute(map => action(map.Get<IUserRepository>()));
        }

        [TestMethod, TestCategory("Integration.Infrastructure")]
        public void UserRepository_Sanity_Check()
        {
            // ReSharper disable ImplicitlyCapturedClosure

            var user = new User(this.IdGenerator, "UserName", "Nickname", this.DomainServices.UserKeyGenerator,
                                this.DomainServices.BuiltInCriterionProvider);
            var userId = user.Id;

            User persistedUser = null;
            this.Execute(x => persistedUser = x.Get(userId));
            Assert.IsNull(persistedUser);

            this.Execute(x => x.Save(user));
            this.Execute(x => persistedUser = x.Get(userId));
            Assert.IsNotNull(persistedUser);
            Assert.AreEqual(user, persistedUser);
            Assert.AreEqual(user.Key, persistedUser.Key);
            Assert.AreEqual(user.Nickname, persistedUser.Nickname);

            var key = user.Key;
            this.Execute(x => persistedUser = x.GetByKey(key));
            Assert.IsNotNull(persistedUser);
            Assert.AreEqual(user, persistedUser);

            const string newNickname = "Nickname2";
            persistedUser.Nickname = newNickname;
            this.Execute(x => x.Save(persistedUser));

            this.Execute(x => persistedUser = x.Get(userId));
            Assert.IsNotNull(persistedUser);
            Assert.AreEqual(persistedUser.Nickname, newNickname);

            this.Execute(x => x.Delete(persistedUser));
            this.Execute(x => persistedUser = x.Get(userId));
            Assert.IsNull(persistedUser);

            // ReSharper restore ImplicitlyCapturedClosure
        }
    }
}