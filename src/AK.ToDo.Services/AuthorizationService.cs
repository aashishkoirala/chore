/*******************************************************************************************************************************
 * AK.To|Do.Services.AuthorizationService
 * Copyright © 2013 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of To Do.
 *  
 * To Do is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * To Do is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with To Do.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

#region Namespace Imports

using AK.Commons.DataAccess;
using AK.ToDo.Contracts.DataAccess.Entities;
using AK.ToDo.Contracts.Services;
using AK.ToDo.Contracts.Services.Operation;
using System;
using System.ComponentModel.Composition;
using System.Linq;

#endregion

namespace AK.ToDo.Services
{
    /// <summary>
    /// Implementation of IAuthorizationService (operations related to authorization/user access).
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof(IAuthorizationService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class AuthorizationService : ServiceBase, IAuthorizationService
    {
        public void AuthorizeItemOperation(Guid userId, Guid itemId)
        {
            var authorized = false;

            this.Db
                .With(this.AppUserRepository, this.ToDoItemRepository)
                .Execute(unit =>
                {
                    var userExists = this.AppUserRepository.GetFor(q => q.Any(x => x.Id == userId));

                    // If the item does not exist yet, you're probably trying to create it, so
                    // we let you do so.
                    //
                    if (userExists && itemId == Guid.Empty) authorized = true;

                    else if (userExists)
                    {
                        authorized =
                            this.ToDoItemRepository.GetFor(q => q.Any(x => x.AppUserId == userId && x.Id == itemId)) ||
                            this.ToDoItemRepository.GetFor(q => !q.Any(x => x.Id == itemId));
                    }
                });

            if (authorized) return;

            this.Logger.Error(string.Format("User {0} does not have access to item {1}.", userId, itemId));
            throw new ServiceException(ServiceErrorCodes.Unauthorized);
        }

        public void AuthorizeCategoryOperation(Guid userId, Guid categoryId)
        {
            var authorized = false;

            this.Db
                .With(this.AppUserRepository, this.ToDoCategoryRepository)
                .Execute(unit =>
                {
                    var userExists = this.AppUserRepository.GetFor(q => q.Any(x => x.Id == userId));

                    // If the category does not exist yet, you're probably trying to create it, so
                    // we let you do so.
                    //
                    if (userExists && categoryId == Guid.Empty) authorized = true;

                    else if (userExists)
                    {
                        authorized = 
                            this.ToDoCategoryRepository.GetFor(q => q.Any(x => x.AppUserId == userId && x.Id == categoryId)) ||
                            this.ToDoCategoryRepository.GetFor(q => !q.Any(x => x.Id == categoryId));
                    }
                });

            if (authorized) return;

            this.Logger.Error(string.Format("User {0} does not have access to category {1}.", userId, categoryId));
            throw new ServiceException(ServiceErrorCodes.Unauthorized);
        }

        public Guid CreateOrLoadUserIdByName(string userName)
        {
            var userId = Guid.Empty;

            this.Db
                .With(this.AppUserRepository)
                .Execute(unit =>
                {
                    userId = this.AppUserRepository.GetFor(
                        q => q
                                 .Where(x => x.UserName == userName)
                                 .Select(x => x.Id)
                                 .FirstOrDefault());

                    // User exists.
                    //
                    if (userId != default(Guid)) return;

                    this.Logger.Information(string.Format("Creating new user with name {0}...", userName));

                    var user = new AppUser {UserName = userName};
                    this.AppUserRepository.Save(user);

                    userId = user.Id;

                    this.Logger.Information(string.Format(
                        "Created new user with name {0}, Id {1}.", userName, userId));
                });

            return userId;
        }
    }
}