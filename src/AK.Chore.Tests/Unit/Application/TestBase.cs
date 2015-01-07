/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.TestBase
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
using AK.Commons;
using AK.Commons.Configuration;
using AK.Commons.DataAccess;
using AK.Commons.DomainDriven;
using AK.Commons.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq.Expressions;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Base class with common functionality for all application layer unit tests.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public abstract class TestBase
    {
        protected const string DatabaseKey = "Test";
        protected const int UserId = 123;

        protected IAppLogger logger;
        protected Mock<IAppConfig> configMock;
        protected Mock<IUnitOfWork> unitOfWorkMock;
        protected Mock<IUnitOfWorkFactory> unitOfWorkFactoryMock;
        protected Mock<IAppDataAccess> dataAccessMock;
        protected Mock<IEntityIdGeneratorProvider> entityIdGeneratorProviderMock;
        protected Mock<IProviderSource<IEntityIdGeneratorProvider>> entityIdGeneratorProviderSourceMock;
        protected Mock<IUserRepository> userRepositoryMock;
        protected Mock<IFolderRepository> folderRepositoryMock;
        protected Mock<IFilterRepository> filterRepositoryMock;
        protected Mock<ITaskRepository> taskRepositoryMock;
        protected Mock<IUserKeyGenerator> userKeyGeneratorMock;
        protected Mock<IBuiltInCriterionProvider> builtInCriterionProviderMock;
        protected User user;

        protected IEntityIdGenerator<int> IdGenerator
        {
            get { return new TestEntityIdGenerator(); }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            this.configMock = new Mock<IAppConfig>();
            this.configMock
                .Setup(x => x.Get("DatabaseKey", It.IsAny<string>()))
                .Returns(DatabaseKey)
                .Verifiable();

            this.logger = new Mock<IAppLogger>().Object;

            this.unitOfWorkMock = new Mock<IUnitOfWork>();

            this.unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>();
            this.unitOfWorkFactoryMock
                .Setup(x => x.Create())
                .Returns(unitOfWorkMock.Object)
                .Verifiable();

            this.dataAccessMock = new Mock<IAppDataAccess>();
            this.dataAccessMock
                .Setup(x => x[DatabaseKey])
                .Returns(this.unitOfWorkFactoryMock.Object)
                .Verifiable();

            this.entityIdGeneratorProviderMock = new Mock<IEntityIdGeneratorProvider>();
            this.entityIdGeneratorProviderMock
                .Setup(x => x.Get<int>())
                .Returns(new TestEntityIdGenerator())
                .Verifiable();

            this.entityIdGeneratorProviderSourceMock = new Mock<IProviderSource<IEntityIdGeneratorProvider>>();
            this.entityIdGeneratorProviderSourceMock
                .Setup(x => x[DatabaseKey])
                .Returns(this.entityIdGeneratorProviderMock.Object)
                .Verifiable();

            this.userKeyGeneratorMock = new Mock<IUserKeyGenerator>();
            this.userKeyGeneratorMock.Setup(x => x.GenerateKey("TestName")).Returns("TestKey").Verifiable();
            this.userKeyGeneratorMock.Setup(x => x.GenerateKey("NewTestName")).Returns("NewTestKey");

            this.builtInCriterionProviderMock = new Mock<IBuiltInCriterionProvider>();
            this.builtInCriterionProviderMock.SetupGet(x => x.AllNotCompleted).Returns(Criterion.True);
            this.builtInCriterionProviderMock.SetupGet(x => x.AllRecurring).Returns(Criterion.True);
            this.builtInCriterionProviderMock.SetupGet(x => x.Completed).Returns(Criterion.True);
            this.builtInCriterionProviderMock.SetupGet(x => x.ThisWeek).Returns(Criterion.True);
            this.builtInCriterionProviderMock.SetupGet(x => x.Today).Returns(Criterion.True);

            this.user = new User(new TestEntityIdGenerator(), "TestName", "TestNickName",
                                 this.userKeyGeneratorMock.Object, this.builtInCriterionProviderMock.Object);

            this.userRepositoryMock = new Mock<IUserRepository>();
            this.userRepositoryMock
                .Setup(x => x.Get(UserId))
                .Returns(this.user);
            this.userRepositoryMock
                .Setup(x => x.GetByKey(It.Is<string>(y => y != "NewTestKey")))
                .Returns(this.user);

            this.folderRepositoryMock = new Mock<IFolderRepository>();
            this.folderRepositoryMock
                .Setup(x => x.ListForUser(this.user, this.userRepositoryMock.Object))
                .Returns(new Folder[0]);
            this.folderRepositoryMock
                .Setup(x => x.Get(It.IsAny<int>(), this.userRepositoryMock.Object))
                .Returns(new Folder(new TestEntityIdGenerator(), "TestGet", this.user));

            this.filterRepositoryMock = new Mock<IFilterRepository>();
            this.filterRepositoryMock
                .Setup(x => x.ListForUser(this.user))
                .Returns(new Filter[0]);
            this.filterRepositoryMock
                .Setup(x => x.Get(It.IsAny<int>(), this.userRepositoryMock.Object))
                .Returns(new Filter(new TestEntityIdGenerator(), "TestGet", this.user, Criterion.True));

            this.taskRepositoryMock = new Mock<ITaskRepository>();
            this.taskRepositoryMock
                .Setup(
                    x => x.Get(It.IsAny<int>(), this.userRepositoryMock.Object, this.folderRepositoryMock.Object))
                .Returns(new Task(new TestEntityIdGenerator(), "TestGet",
                                  new Folder(new TestEntityIdGenerator(), "TestTaskFolder", this.user), DateTime.Now));

            this.taskRepositoryMock
                .Setup(
                    x =>
                    x.ListForPredicate(It.IsAny<Expression<Func<Task, bool>>>(),
                                       this.userRepositoryMock.Object,
                                       this.folderRepositoryMock.Object))
                .Returns(new Task[0]);

            DomainDrivenUtility.DomainRepositoryFactory = type =>
                {
                    if (type == typeof (ITaskRepository)) return this.taskRepositoryMock.Object;
                    if (type == typeof (IFolderRepository)) return this.folderRepositoryMock.Object;
                    if (type == typeof (IFilterRepository)) return this.filterRepositoryMock.Object;
                    if (type == typeof (IUserRepository)) return this.userRepositoryMock.Object;

                    throw new NotSupportedException();
                };
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.logger = null;
            this.configMock = null;
            this.unitOfWorkMock = null;
            this.unitOfWorkFactoryMock = null;
            this.dataAccessMock = null;
            this.entityIdGeneratorProviderMock = null;
            this.entityIdGeneratorProviderSourceMock = null;
            this.userRepositoryMock = null;
            this.folderRepositoryMock = null;
            this.filterRepositoryMock = null;
            this.taskRepositoryMock = null;
            DomainDrivenUtility.DomainRepositoryFactory = null;
        }

        private class TestEntityIdGenerator : IEntityIdGenerator<int>
        {
            private static int nextId;

            public int Next<TEntity>() where TEntity : IEntity<TEntity, int>
            {
                return typeof (TEntity) == typeof (User) ? UserId : nextId++;
            }
        }
    }
}