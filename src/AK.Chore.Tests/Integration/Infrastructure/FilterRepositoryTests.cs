/*******************************************************************************************************************************
 * AK.Chore.Tests.Integration.Infrastructure.FilterRepositoryTests
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
using AK.Chore.Domain.Users;
using AK.Commons.DomainDriven;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

#endregion

namespace AK.Chore.Tests.Integration.Infrastructure
{
    /// <summary>
    /// Integration tests for FilterRepository.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass, Ignore]
    public class FilterRepositoryTests : TestBase
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

        private void Execute(Action<IUserRepository, IFilterRepository> action)
        {
            this.Db
                .With<IUserRepository>()
                .With<IFilterRepository>()
                .Execute(map => action(map.Get<IUserRepository>(), map.Get<IFilterRepository>()));
        }

        [TestMethod, TestCategory("Integration.Infrastructure")]
        public void FilterRepository_Sanity_Check()
        {
            var filter = new Filter(this.IdGenerator, "Filter", this.user, Criterion.True);
            var filterId = filter.Id;

            Filter persistedFilter = null;
            this.Execute((u, f) => persistedFilter = f.Get(filterId, u));
            Assert.IsNull(persistedFilter);

            this.Execute((u, f) => f.Save(filter));
            this.Execute((u, f) => persistedFilter = f.Get(filterId, u));
            Assert.IsNotNull(persistedFilter);
            Assert.AreEqual(filter, persistedFilter);
            Assert.AreEqual(filter.Name, persistedFilter.Name);
            Assert.AreEqual(filter.Criterion, persistedFilter.Criterion);

            const string newName = "Filter2";
            persistedFilter.Name = newName;
            this.Execute((u, f) => f.Save(persistedFilter));
            this.Execute((u, f) => persistedFilter = f.Get(filterId, u));
            Assert.IsNotNull(persistedFilter);
            Assert.AreEqual(persistedFilter.Name, newName);

            var simpleCriterion = new SimpleCriterion(Field.StartDate, Operator.Equals, FieldValue.TodaysDate);
            var recurrenceCriterion =
                new RecurrenceCriterion(new[]
                    {
                        new RecurrenceCriterion.LiteralOrSpecialDate(FieldValue.TodaysDate),
                        new RecurrenceCriterion.LiteralOrSpecialDate(FieldValue.ThisWeeksStartDate)
                    });
            var complexCriterion = new ComplexCriterion(simpleCriterion, Conjunction.AndNot, recurrenceCriterion);

            persistedFilter.Criterion = simpleCriterion;
            this.Execute((u, f) => f.Save(persistedFilter));
            this.Execute((u, f) => persistedFilter = f.Get(filterId, u));
            Assert.IsNotNull(persistedFilter);
            Assert.AreEqual(persistedFilter.Criterion, simpleCriterion);

            persistedFilter.Criterion = recurrenceCriterion;
            this.Execute((u, f) => f.Save(persistedFilter));
            this.Execute((u, f) => persistedFilter = f.Get(filterId, u));
            Assert.IsNotNull(persistedFilter);
            Assert.AreEqual(persistedFilter.Criterion, recurrenceCriterion);

            persistedFilter.Criterion = complexCriterion;
            this.Execute((u, f) => f.Save(persistedFilter));
            this.Execute((u, f) => persistedFilter = f.Get(filterId, u));
            Assert.IsNotNull(persistedFilter);
            Assert.AreEqual(persistedFilter.Criterion, complexCriterion);

            this.Execute((u, f) => f.Delete(persistedFilter));
            this.Execute((u, f) => persistedFilter = f.Get(filterId, u));
            Assert.IsNull(persistedFilter);
        }
    }
}