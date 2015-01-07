/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Domain.FilterTests
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

using System.Linq;
using AK.Chore.Domain;
using AK.Chore.Domain.Filters;
using AK.Chore.Domain.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace AK.Chore.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests for Filter.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class FilterTests : TestBase
    {
        private User user;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            this.user = new User(this.idGenerator, "akoirala", "Test", this.userKeyGenerator,
                                 this.builtInCriterionProvider);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Filter_Creation_Valid_Works()
        {
            var filter = new Filter(this.idGenerator, "My Filter", this.user, Criterion.True);
            Assert.IsNotNull(filter);
            Assert.AreEqual(filter.Name, "My Filter");
            Assert.AreEqual(filter.Criterion, Criterion.True);
            Assert.IsTrue(this.user.Filters.Contains(filter));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Filter_Creation_Invalid_IdGenerator_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.FilterIdGeneratorNotSet,
                () => new Filter(null, "Test", this.user, Criterion.True));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Filter_Creation_Invalid_Name_Not_Set_Or_Whitespace_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.FilterNameEmpty,
                () => new Filter(this.idGenerator, null, this.user, Criterion.True));

            this.TestInvalid(
                DomainValidationErrorCode.FilterNameEmpty,
                () => new Filter(this.idGenerator, "  ", this.user, Criterion.True));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Filter_Creation_Invalid_User_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.FilterUserNotSet,
                () => new Filter(this.idGenerator, "Test", null, Criterion.True));
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Filter_Creation_Invalid_Criterion_Not_Set_Throws()
        {
            this.TestInvalid(
                DomainValidationErrorCode.FilterCriterionNotSet,
                () => new Filter(this.idGenerator, "Test", this.user, null));
        }
    }
}