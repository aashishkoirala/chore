/*******************************************************************************************************************************
 * AK.Chore.Tests.Integration.TestBase
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
using AK.Commons.DataAccess;
using AK.Commons.DomainDriven;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using System.ComponentModel.Composition;
using System.Linq;

#endregion

namespace AK.Chore.Tests.Integration
{
    /// <summary>
    /// Base class with common functionality for all integration tests.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public abstract class TestBase
    {
        protected IUnitOfWorkFactory Db
        {
            get { return AppEnvironment.DataAccess["ChoreTests"]; }
        }

        protected IEntityIdGenerator<int> IdGenerator
        {
            get { return AppEnvironment.EntityIdGenerator["ChoreTests"].Get<int>(); }
        }

        protected IDomainServices DomainServices
        {
            get { return AppEnvironment.Composer.Resolve<IDomainServices>(); }
        }

        protected IRepositories Repositories
        {
            get { return AppEnvironment.Composer.Resolve<IRepositories>(); }
        }

        [TestInitialize]
        public virtual void Initialize()
        {
            InitializeDatabase();
        }

        private static void InitializeDatabase()
        {
            var config = AppEnvironment.Config;

            var connectionString = config.Get<string>("ak.commons.dataaccess.uowfactory.ChoreTests.connectionstring");
            var databaseName = config.Get<string>("ak.commons.dataaccess.uowfactory.ChoreTests.database");

            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var database = server.GetDatabase(databaseName);

            foreach (var collectionName in database.GetCollectionNames().Where(x => !x.StartsWith("system")))
                database.DropCollection(collectionName);
        }
    }

    public interface IDomainServices
    {
        IUserKeyGenerator UserKeyGenerator { get; set; }
        IBuiltInCriterionProvider BuiltInCriterionProvider { get; set; }
        IRecurrenceGrouper RecurrenceGrouper { get; set; }
        IRecurrencePredicateRewriter RecurrencePredicateRewriter { get; set; }
        ITaskGrouper TaskGrouper { get; set; }
    }

    public interface IRepositories
    {
        IUserRepository UserRepository { get; set; }
        IFolderRepository FolderRepository { get; set; }
        IFilterRepository FilterRepository { get; set; }
        ITaskRepository TaskRepository { get; set; }
    }

    [Export(typeof (IDomainServices))]
    public class DomainServicesImpl : IDomainServices
    {
        [Import]
        public IUserKeyGenerator UserKeyGenerator { get; set; }

        [Import]
        public IBuiltInCriterionProvider BuiltInCriterionProvider { get; set; }

        [Import]
        public IRecurrenceGrouper RecurrenceGrouper { get; set; }

        [Import]
        public IRecurrencePredicateRewriter RecurrencePredicateRewriter { get; set; }

        [Import]
        public ITaskGrouper TaskGrouper { get; set; }
    }

    [Export(typeof (IRepositories))]
    public class RepositoriesImpl : IRepositories
    {
        [Import]
        public IUserRepository UserRepository { get; set; }

        [Import]
        public IFolderRepository FolderRepository { get; set; }

        [Import]
        public IFilterRepository FilterRepository { get; set; }

        [Import]
        public ITaskRepository TaskRepository { get; set; }
    }
}