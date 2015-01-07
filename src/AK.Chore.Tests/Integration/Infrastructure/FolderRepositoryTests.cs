/*******************************************************************************************************************************
 * AK.Chore.Tests.Integration.Infrastructure.FolderRepositoryTests
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
using System.Linq;

#endregion

namespace AK.Chore.Tests.Integration.Infrastructure
{
    /// <summary>
    /// Integration tests for FolderRepository.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass, Ignore]
    public class FolderRepositoryTests : TestBase
    {
        private User user;
        private static readonly object userLock = new object();

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
            if (this.user != null) return;

            lock (userLock)
            {
                if (this.user != null) return;

                var newUser = new User(this.IdGenerator, "UserName", "Nickname", this.DomainServices.UserKeyGenerator,
                                       this.DomainServices.BuiltInCriterionProvider);

                this.Db
                    .With<IUserRepository>()
                    .Execute(map => map.Get<IUserRepository>().Save(newUser));

                this.user = newUser;
            }
        }

        private void Execute(Action<IUserRepository, IFolderRepository> action)
        {
            this.Db
                .With<IUserRepository>()
                .With<IFolderRepository>()
                .Execute(map => action(map.Get<IUserRepository>(), map.Get<IFolderRepository>()));
        }

        private void Execute(Action<IUserRepository, IFolderRepository, ITaskRepository> action)
        {
            this.Db
                .With<IUserRepository>()
                .With<IFolderRepository>()
                .With<ITaskRepository>()
                .Execute(
                    map => action(map.Get<IUserRepository>(), map.Get<IFolderRepository>(), map.Get<ITaskRepository>()));
        }

        [TestMethod, TestCategory("Integration.Infrastructure")]
        public void FolderRepository_Sanity_Check()
        {
            // ReSharper disable ImplicitlyCapturedClosure

            var newFolder1 = new Folder(this.IdGenerator, "Folder1", this.user);
            var newFolder2 = new Folder(this.IdGenerator, "Folder2", this.user);
            var newFolder3 = new Folder(this.IdGenerator, "Folder3", newFolder1);
            var newFolder4 = new Folder(this.IdGenerator, "Folder4", newFolder3);

            Folder folder1 = null, folder2 = null, folder3 = null, folder4 = null;

            this.Execute((u, f) => folder1 = f.Get(newFolder1.Id, u));
            Assert.IsNull(folder1);
            this.Execute((u, f) => folder2 = f.Get(newFolder2.Id, u));
            Assert.IsNull(folder2);
            this.Execute((u, f) => folder3 = f.Get(newFolder3.Id, u));
            Assert.IsNull(folder3);
            this.Execute((u, f) => folder4 = f.Get(newFolder4.Id, u));
            Assert.IsNull(folder4);

            this.Execute((u, f) => f.Save(newFolder1));
            this.Execute((u, f) => f.Save(newFolder2));

            this.Execute((u, f) => folder1 = f.Get(newFolder1.Id, u));
            Assert.IsNotNull(folder1);
            this.Execute((u, f) => folder2 = f.Get(newFolder2.Id, u));
            Assert.IsNotNull(folder2);
            this.Execute((u, f) => folder3 = f.Get(newFolder3.Id, u));
            Assert.IsNotNull(folder3);
            this.Execute((u, f) => folder4 = f.Get(newFolder4.Id, u));
            Assert.IsNotNull(folder4);

            Assert.AreEqual(folder1, newFolder1);
            Assert.AreEqual(folder2, newFolder2);
            Assert.AreEqual(folder3, newFolder3);
            Assert.AreEqual(folder4, newFolder4);
            Assert.AreEqual(folder1, folder3.Parent);
            Assert.AreEqual(folder3, folder4.Parent);

            Folder[] folders = null;
            this.Execute((u, f) => folders = f.List(new[] {folder1.Id, folder3.Id}, u).ToArray());

            Assert.AreEqual(folders.Length, 2);
            Assert.AreEqual(folders[0], folder1);
            Assert.AreEqual(folders[1], folder3);
            Assert.AreEqual(folders[1].Parent, folders[0]);
            Assert.AreEqual(folders[0].Folders.Single(), folders[1]);

            this.Execute((u, f, t) => f.Delete(folder1, u, t));
            this.Execute((u, f, t) => f.Delete(folder2, u, t));

            this.Execute((u, f) => folder1 = f.Get(newFolder1.Id, u));
            Assert.IsNull(folder1);
            this.Execute((u, f) => folder2 = f.Get(newFolder2.Id, u));
            Assert.IsNull(folder2);
            this.Execute((u, f) => folder3 = f.Get(newFolder3.Id, u));
            Assert.IsNull(folder3);
            this.Execute((u, f) => folder4 = f.Get(newFolder4.Id, u));
            Assert.IsNull(folder4);

            // ReSharper restore ImplicitlyCapturedClosure
        }
    }
}