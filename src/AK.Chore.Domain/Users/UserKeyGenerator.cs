/*******************************************************************************************************************************
 * AK.Chore.Domain.Users.UserKeyGenerator
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
using System.ComponentModel.Composition;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace AK.Chore.Domain.Users
{
    /// <summary>
    /// Generates unique keys for users based on name.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IUserKeyGenerator
    {
        string GenerateKey(string userName);
    }

    [Export(typeof (IUserKeyGenerator)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class UserKeyGenerator : IUserKeyGenerator
    {
        public string GenerateKey(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new DomainValidationException(DomainValidationErrorCode.UserNameEmpty);

            var data = Encoding.ASCII.GetBytes(userName);
            using (var hash = SHA1.Create())
            {
                data = hash.ComputeHash(data);
            }

            return Convert.ToBase64String(data);
        }
    }
}