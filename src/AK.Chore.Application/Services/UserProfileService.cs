/*******************************************************************************************************************************
 * AK.Chore.Application.Services.UserProfileService
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

using AK.Chore.Application.Aspects;
using AK.Chore.Contracts;
using AK.Chore.Contracts.UserProfile;
using AK.Chore.Domain.Filters;
using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Tasks;
using AK.Chore.Domain.Users;
using AK.Commons;
using AK.Commons.Configuration;
using AK.Commons.DataAccess;
using AK.Commons.DomainDriven;
using AK.Commons.Logging;
using AK.Commons.Services;
using System;
using System.ComponentModel.Composition;
using User = AK.Chore.Contracts.UserProfile.User;

#endregion

namespace AK.Chore.Application.Services
{
    /// <summary>
    /// Service implementation - IUserProfileService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (IUserProfileService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class UserProfileService : ServiceBase, IUserProfileService
    {
        private readonly IUserKeyGenerator userKeyGenerator;
        private readonly IBuiltInCriterionProvider builtInCriterionProvider;

        [ImportingConstructor]
        public UserProfileService(
            [Import] IUserKeyGenerator userKeyGenerator,
            [Import] IBuiltInCriterionProvider builtInCriterionProvider,
            [Import] IAppDataAccess appDataAccess,
            [Import] IAppConfig appconfig,
            [Import] IAppLogger appLogger,
            [Import] IProviderSource<IEntityIdGeneratorProvider> entityIdGeneratorProvider)
            : base(appDataAccess, appconfig, appLogger, entityIdGeneratorProvider)
        {
            this.userKeyGenerator = userKeyGenerator;
            this.builtInCriterionProvider = builtInCriterionProvider;
        }

        [CatchToReturn("We had a problem getting a user by that name.")]
        public OperationResult<User> GetUserByUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return new OperationResult<User>(UserProfileResult.UserNameNotSpecified);

            var key = this.userKeyGenerator.GenerateKey(userName);
            var user = this.Execute(userRepository => userRepository.GetByKey(key));

            if (user == null) return new OperationResult<User>(UserProfileResult.UserDoesNotExist, userName);

            return new OperationResult<User>(new User
                {
                    Id = user.Id,
                    Key = user.Key,
                    Nickname = user.Nickname
                });
        }

        [CatchToReturn("We had a problem creating that user.")]
        public OperationResult<User> CreateUser(string userName, string nickname)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return new OperationResult<User>(UserProfileResult.UserNameNotSpecified);

            if (string.IsNullOrWhiteSpace(nickname))
                nickname = "You";

            var key = this.userKeyGenerator.GenerateKey(userName);
            var user = this.Execute(userRepository => userRepository.GetByKey(key));

            if (user != null)
                return new OperationResult<User>(UserProfileResult.UserAlreadyExists, userName);

            user = new Domain.Users.User(
                this.IdGenerator,
                userName,
                nickname,
                this.userKeyGenerator,
                this.builtInCriterionProvider);

            this.Execute((userRepository, folderRepository, filterRepository) =>
                {
                    userRepository.Save(user);
                    user.Folders.ForEach(folderRepository.Save);
                    user.Filters.ForEach(filterRepository.Save);
                });

            return new OperationResult<User>(new User
                {
                    Id = user.Id,
                    Key = user.Key,
                    Nickname = user.Nickname
                });
        }

        [CatchToReturn("We had a problem changing the nickname for that user.")]
        public OperationResult UpdateNickname(User user, int userId)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.Id != userId) return new OperationResult(GeneralResult.NotAuthorized, userId);

            var result = new OperationResult();
            this.Execute(userRepository =>
                {
                    var userEntity = userRepository.Get(user.Id);

                    if (userEntity == null)
                    {
                        result = new OperationResult(UserProfileResult.UserDoesNotExist, userId);
                        return;
                    }

                    userEntity.Nickname = user.Nickname;

                    userRepository.Save(userEntity);
                });

            return result;
        }

        [CatchToReturn("We had a problem deleting that user's profile.")]
        public OperationResult DeleteUserProfile(User user, int userId)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.Id != userId) return new OperationResult(GeneralResult.NotAuthorized, userId);

            var result = new OperationResult();
            this.Execute(
                (userRepository, filterRepository, folderRepository, taskRepository) =>
                    {
                        var userEntity = userRepository.Get(userId);

                        if (userEntity == null)
                        {
                            result = new OperationResult(UserProfileResult.UserDoesNotExist, userId);
                            return;
                        }

                        userEntity.LoadFilters(filterRepository);
                        userEntity.LoadFolders(userRepository, folderRepository);

                        var taskList = taskRepository.ListForPredicate(
                            x => x.User.Id == userId, userRepository, folderRepository);

                        taskList.ForEach(taskRepository.Delete);
                        userEntity.Folders.ForEach(x => folderRepository.Delete(x, userRepository, taskRepository));
                        userEntity.Filters.ForEach(filterRepository.Delete);

                        userRepository.Delete(userEntity);
                    });
            return result;
        }

        private void Execute(Action<IUserRepository> action)
        {
            this.Db.Execute(action);
        }

        private Domain.Users.User Execute(Func<IUserRepository, Domain.Users.User> action)
        {
            return this.Db.Execute(action);
        }

        private void Execute(Action<IUserRepository, IFolderRepository, IFilterRepository> action)
        {
            this.Db.Execute(action);
        }

        private void Execute(Action<IUserRepository, IFilterRepository, IFolderRepository, ITaskRepository> action)
        {
            this.Db.Execute(action);
        }
    }
}