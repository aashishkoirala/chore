/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Domain.TestBase
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
using AK.Chore.Domain.Tasks;
using AK.Chore.Domain.Users;
using AK.Commons.DomainDriven;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;

#endregion

namespace AK.Chore.Tests.Unit.Domain
{
    /// <summary>
    /// Base class with common functionality for domain layer unit tests.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public abstract class TestBase
    {
        protected IEntityIdGenerator<int> idGenerator;
        protected IUserKeyGenerator userKeyGenerator;
        protected IBuiltInCriterionProvider builtInCriterionProvider;
        protected IRecurrenceGrouper recurrenceGrouper;
        protected IRecurrencePredicateRewriter recurrencePredicateRewriter;

        private static int entityId;

        [TestInitialize]
        public virtual void Initialize()
        {
            this.AssignIdGenerator();
            this.userKeyGenerator = new UserKeyGenerator();
            this.builtInCriterionProvider = new BuiltInCriterionProvider();
            this.recurrenceGrouper = new RecurrenceGrouper();
            this.recurrencePredicateRewriter = new RecurrencePredicateRewriter(this.recurrenceGrouper);
        }

        protected void TestInvalid(DomainValidationErrorCode code, Action action)
        {
            Expect.Error<DomainValidationException, DomainValidationErrorCode>(code, action);
        }

        private void AssignIdGenerator()
        {
            var idGeneratorMock = new Mock<IEntityIdGenerator<int>>();
            Func<int> idGeneratorFunc = () =>
                {
                    Interlocked.Increment(ref entityId);
                    return entityId;
                };
            idGeneratorMock.Setup(x => x.Next<User>()).Returns(idGeneratorFunc);
            idGeneratorMock.Setup(x => x.Next<Filter>()).Returns(idGeneratorFunc);
            idGeneratorMock.Setup(x => x.Next<Folder>()).Returns(idGeneratorFunc);
            idGeneratorMock.Setup(x => x.Next<Task>()).Returns(idGeneratorFunc);
            this.idGenerator = idGeneratorMock.Object;
        }
    }
}