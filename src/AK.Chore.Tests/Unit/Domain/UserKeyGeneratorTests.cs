/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Domain.UserKeyGeneratorTests
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

using System;
using System.Security.Cryptography;
using System.Text;
using AK.Chore.Domain;
using AK.Chore.Domain.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace AK.Chore.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests for UserKeyGenerator.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class UnitKeyGeneratorTests
    {
        private IUserKeyGenerator userKeyGenerator;

        [TestInitialize]
        public void Initialize()
        {
            this.userKeyGenerator = new UserKeyGenerator();
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void UserKeyGenerator_GenerateKey_Valid_Works()
        {
            const string userName = "akoirala";

            var data = Encoding.ASCII.GetBytes(userName);
            using (var hash = SHA1.Create())
            {
                data = hash.ComputeHash(data);
            }

            var expected = Convert.ToBase64String(data);
            var key = userKeyGenerator.GenerateKey(userName);

            Assert.AreEqual(expected, key);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void UserKeyGenerator_GenerateKey_Invalid_UserName_Not_Set_Throws()
        {
            try
            {
                userKeyGenerator.GenerateKey(null);
                Assert.Fail("Exception expected.");
            }
            catch (DomainValidationException ex)
            {
                Assert.AreEqual(ex.Reason, DomainValidationErrorCode.UserNameEmpty);
            }
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void UserKeyGenerator_GenerateKey_Invalid_UserName_Whitespace_Throws()
        {
            try
            {
                userKeyGenerator.GenerateKey("   ");
                Assert.Fail("Exception expected.");
            }
            catch (DomainValidationException ex)
            {
                Assert.AreEqual(ex.Reason, DomainValidationErrorCode.UserNameEmpty);
            }
        }
    }
}