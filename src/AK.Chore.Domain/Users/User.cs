/*******************************************************************************************************************************
 * AK.Chore.Domain.Users.User
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
using AK.Commons;
using AK.Commons.DomainDriven;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#endregion

namespace AK.Chore.Domain.Users
{
    /// <summary>
    /// Represents the domain of the application user.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class User : IAggregateRoot<User, int>
    {
        private string nickname;
        protected readonly Collection<Folder> folders = new Collection<Folder>();
        protected readonly Collection<Filter> filters = new Collection<Filter>();

        public User(
            IEntityIdGenerator<int> idGenerator,
            string userName,
            string nickname,
            IUserKeyGenerator userKeyGenerator,
            IBuiltInCriterionProvider builtInCriterionProvider)
        {
            if (idGenerator == null)
                throw new DomainValidationException(DomainValidationErrorCode.UserIdGeneratorNotSet);

            if (userKeyGenerator == null)
                throw new DomainValidationException(DomainValidationErrorCode.UserKeyGeneratorNotSet);

            if (builtInCriterionProvider == null)
                throw new DomainValidationException(DomainValidationErrorCode.UserBuiltInFilterProviderNotSet);

            if (string.IsNullOrWhiteSpace(userName))
                throw new DomainValidationException(DomainValidationErrorCode.UserNameEmpty);

            if (string.IsNullOrWhiteSpace(nickname))
                throw new DomainValidationException(DomainValidationErrorCode.UserNicknameEmpty);

            this.Id = idGenerator.Next<User>();
            this.Key = userKeyGenerator.GenerateKey(userName);
            this.nickname = nickname;

            this.AssignDefaultFolders(idGenerator);
            this.AssignDefaultFilters(idGenerator, builtInCriterionProvider);
        }

        // ReSharper disable ObjectCreationAsStatement

        private void AssignDefaultFolders(IEntityIdGenerator<int> idGenerator)
        {
            new Folder(idGenerator, "Personal", this);
            this.AreFoldersLoaded = true;
        }

        private void AssignDefaultFilters(
            IEntityIdGenerator<int> idGenerator,
            IBuiltInCriterionProvider builtInCriterionProvider)
        {
            new Filter(idGenerator, "Today", this, builtInCriterionProvider.Today);
            new Filter(idGenerator, "This Week", this, builtInCriterionProvider.ThisWeek);
            new Filter(idGenerator, "Completed", this, builtInCriterionProvider.Completed);
            new Filter(idGenerator, "All Not Done", this, builtInCriterionProvider.AllNotCompleted);
            new Filter(idGenerator, "All Recurring", this, builtInCriterionProvider.AllRecurring);
            this.AreFiltersLoaded = true;
        }

        // ReSharper restore ObjectCreationAsStatement

        protected User()
        {
        }

        public int Id { get; protected set; }
        public string Key { get; protected set; }

        public string Nickname
        {
            get { return this.nickname; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new DomainValidationException(DomainValidationErrorCode.UserNicknameEmpty);

                this.nickname = value;
            }
        }

        public bool AreFoldersLoaded { get; private set; }
        public bool AreFiltersLoaded { get; private set; }

        public IReadOnlyCollection<Folder> Folders
        {
            get { return this.folders; }
            protected set { value.AssignTo(this.folders); }
        }

        public IReadOnlyCollection<Filter> Filters
        {
            get { return this.filters; }
            protected set { value.AssignTo(this.filters); }
        }

        public bool Equals(User other)
        {
            return this.Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is User && this.Equals(obj as User);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public void LoadFolders(IUserRepository userRepository, IFolderRepository folderRepository)
        {
            this.Folders = folderRepository.ListForUser(this, userRepository);
            this.AreFoldersLoaded = true;
        }

        public void LoadFilters(IFilterRepository filterRepository)
        {
            this.Filters = filterRepository.ListForUser(this);
            this.AreFiltersLoaded = true;
        }

        public void AddFolder(Folder folder)
        {
            this.ValidateFolderForAddition(folder);
            this.folders.Add(folder);
        }

        public void RemoveFolder(Folder folder)
        {
            this.ValidateFolderForRemoval(folder);
            this.folders.Remove(folder);
        }

        public void AddFilter(Filter filter)
        {
            this.ValidateFilterForAddition(filter);
            this.filters.Add(filter);
        }

        public void RemoveFilter(Filter filter)
        {
            this.ValidateFilterForRemoval(filter);
            this.filters.Remove(filter);
        }

        private void ValidateFolderForAddition(Folder folder)
        {
            if (folder == null) throw new DomainValidationException(DomainValidationErrorCode.UserFolderNotSet);

            if (!this.Equals(folder.User))
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.UserFolderWithInvalidUser, new
                        {
                            Operation = "Add",
                            FolderId = folder.Id,
                            FolderName = folder.Name,
                            UserId = folder.User.Id
                        });
            }

            if (!this.folders.Contains(folder) && this.folders.All(x => x.Name != folder.Name)) return;

            throw new DomainValidationException(
                DomainValidationErrorCode.UserAttemptToAddExistingFolder,
                new
                    {
                        Operation = "Add",
                        FolderId = folder.Id,
                        FolderName = folder.Name,
                        UserId = this.Id
                    });
        }

        private void ValidateFolderForRemoval(Folder folder)
        {
            if (folder == null) throw new DomainValidationException(DomainValidationErrorCode.UserFolderNotSet);

            if (!this.Equals(folder.User))
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.UserFolderWithInvalidUser, new
                        {
                            Operation = "Remove",
                            FolderId = folder.Id,
                            FolderName = folder.Name,
                            UserId = folder.User.Id
                        });
            }

            if (this.folders.Contains(folder)) return;

            throw new DomainValidationException(
                DomainValidationErrorCode.UserAttemptToRemoveNonExistingFolder,
                new
                    {
                        Operation = "Remove",
                        FolderId = folder.Id,
                        FolderName = folder.Name,
                        UserId = this.Id
                    });
        }

        private void ValidateFilterForAddition(Filter filter)
        {
            if (filter == null) throw new DomainValidationException(DomainValidationErrorCode.UserFilterNotSet);

            if (!this.Equals(filter.User))
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.UserFilterWithInvalidUser, new
                        {
                            Operation = "Add",
                            FilterId = filter.Id,
                            FilterName = filter.Name,
                            UserId = filter.User.Id
                        });
            }

            if (!this.filters.Contains(filter) && this.filters.All(x => x.Name != filter.Name)) return;

            throw new DomainValidationException(
                DomainValidationErrorCode.UserAttemptToAddExistingFilter,
                new
                    {
                        Operation = "Add",
                        FilterId = filter.Id,
                        FilterName = filter.Name,
                        UserId = this.Id
                    });
        }

        private void ValidateFilterForRemoval(Filter filter)
        {
            if (filter == null) throw new DomainValidationException(DomainValidationErrorCode.UserFilterNotSet);

            if (!this.Equals(filter.User))
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.UserFilterWithInvalidUser, new
                        {
                            Operation = "Remove",
                            FilterId = filter.Id,
                            FilterName = filter.Name,
                            UserId = filter.User.Id
                        });
            }

            if (this.filters.Contains(filter)) return;

            throw new DomainValidationException(
                DomainValidationErrorCode.UserAttemptToRemoveNonExistingFilter,
                new
                    {
                        Operation = "Remove",
                        FilterId = filter.Id,
                        FilterName = filter.Name,
                        UserId = this.Id
                    });
        }
    }
}