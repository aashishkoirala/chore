/*******************************************************************************************************************************
 * AK.Chore.Domain.Folders.Folder
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
using AK.Chore.Domain.Tasks;
using AK.Chore.Domain.Users;
using AK.Commons;
using AK.Commons.DomainDriven;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#endregion

namespace AK.Chore.Domain.Folders
{
    /// <summary>
    /// Represents the domain of a folder that contains tasks or other folders.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Folder : IAggregateRoot<Folder, int>
    {
        private string name;
        protected readonly Collection<Task> tasks = new Collection<Task>();
        protected readonly Collection<Folder> folders = new Collection<Folder>();

        public Folder(IEntityIdGenerator<int> idGenerator, string name, User user)
            : this(GenerateId(idGenerator), name, null, user)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainValidationException(DomainValidationErrorCode.FolderNameEmpty);

            if (user == null)
                throw new DomainValidationException(DomainValidationErrorCode.FolderUserNotSet);
        }

        public Folder(IEntityIdGenerator<int> idGenerator, string name, Folder parent)
            : this(GenerateId(idGenerator), name, parent, null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainValidationException(DomainValidationErrorCode.FolderNameEmpty);

            if (parent == null)
                throw new DomainValidationException(DomainValidationErrorCode.FolderParentNotSet);
        }

        protected Folder(int id, string name, Folder parent, User user)
        {
            this.Id = id;
            this.Name = name;
            this.Parent = parent;
            this.User = user;

            if (this.Parent != null) this.Parent.AddFolder(this);
            if (this.User != null) this.User.AddFolder(this);

            if (this.User == null && this.Parent != null) this.User = this.Parent.User;
        }

        public int Id { get; protected set; }
        public User User { get; protected set; }
        public Folder Parent { get; protected set; }
        public bool AreTasksLoaded { get; private set; }
        public bool AreFoldersLoaded { get; private set; }

        public string Name
        {
            get { return this.name; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new DomainValidationException(DomainValidationErrorCode.FolderNameEmpty);

                this.name = value;
            }
        }

        public string FullPath
        {
            get { return this.Parent == null ? this.Name : this.Parent.FullPath + "/" + this.Name; }
        }

        public IReadOnlyCollection<Task> Tasks
        {
            get { return this.tasks; }
            protected set { value.AssignTo(this.tasks); }
        }

        public IReadOnlyCollection<Folder> Folders
        {
            get { return this.folders; }
            protected set { value.AssignTo(this.folders); }
        }

        public bool Equals(Folder other)
        {
            return this.Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is Folder && this.Equals(obj as Folder);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public virtual void LoadFolders(IUserRepository userRepository, IFolderRepository folderRepository)
        {
            this.Folders = folderRepository.ListForFolder(this, userRepository);
            this.AreFoldersLoaded = true;
        }

        public virtual void LoadTasks(ITaskGrouper taskGrouper,
                                      IUserRepository userRepository,
                                      IFolderRepository folderRepository,
                                      ITaskRepository taskRepository)
        {
            this.Tasks = taskGrouper.LoadForCriterion(
                Criterion.True, this.User, DateTime.Today, userRepository,
                folderRepository, taskRepository, new[] {this});

            this.AreTasksLoaded = true;
        }

        public void AddTask(Task task)
        {
            this.ValidateTaskForAddition(task);
            this.tasks.Add(task);
        }

        public void RemoveTask(Task task)
        {
            this.ValidateTaskForRemoval(task);
            this.tasks.Remove(task);
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
            folder.Parent = null;
        }

        public IReadOnlyCollection<Folder> ListHierarchy()
        {
            return GetListOfAllChildrenIncludingSelf(this);
        }

        public void MoveTo(Folder folder)
        {
            if (folder == null)
            {
                if (this.Parent == null) return;
                this.Parent.RemoveFolder(this);
                this.User.AddFolder(this);
                return;
            }

            if (!folder.User.Equals(this.User))
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.FolderAttemptToMoveToAnotherUsersFolder,
                    new
                        {
                            FolderId = this.Id,
                            FromFolderId = this.Parent == null ? null : (int?) this.Parent.Id,
                            ToFolderId = folder.Id
                        });
            }

            if (this.Parent != null && this.Parent.AreFoldersLoaded) this.Parent.RemoveFolder(this);
            this.Parent = folder;
            folder.AddFolder(this);
        }

        private void ValidateTaskForAddition(Task task)
        {
            if (task == null)
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.FolderTaskNotSet, new {Operation = "Add"});
            }

            if (!this.Equals(task.Folder))
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.FolderTaskWithInvalidFolder, new
                        {
                            Operation = "Add",
                            TaskId = task.Id,
                            TaskDescription = task.Description,
                            FolderId = task.Folder.Id,
                            FolderName = task.Folder.Name
                        });
            }

            if (!this.tasks.Contains(task) && this.tasks.All(x => x.Description != task.Description)) return;

            throw new DomainValidationException(DomainValidationErrorCode.FolderAttemptToAddExistingTask, new
                {
                    TaskId = task.Id,
                    TaskDescription = task.Description
                });
        }

        private void ValidateTaskForRemoval(Task task)
        {
            if (task == null)
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.FolderTaskNotSet, new {Operation = "Remove"});
            }

            if (!this.Equals(task.Folder))
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.FolderTaskWithInvalidFolder, new
                        {
                            Operation = "Remove",
                            FolderId = task.Folder.Id,
                            FolderName = task.Folder.Name
                        });
            }

            if (this.tasks.Contains(task)) return;

            throw new DomainValidationException(DomainValidationErrorCode.FolderAttemptToRemoveNonExistingTask, new
                {
                    TaskId = task.Id,
                    TaskDescription = task.Description
                });
        }

        private void ValidateFolderForAddition(Folder folder)
        {
            if (folder == null)
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.FolderFolderNotSet, new {Operation = "Add"});
            }

            if (folder.Parent == null || !this.Equals(folder.Parent))
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.FolderFolderWithInvalidParent, new
                        {
                            Operation = "Add",
                            FolderId = folder.Id,
                            FolderName = folder.Name
                        });
            }

            if (!this.folders.Contains(folder) && this.folders.All(x => x.Name != folder.Name)) return;

            throw new DomainValidationException(DomainValidationErrorCode.FolderAttemptToAddExistingFolder, new
                {
                    FolderId = folder.Id,
                    FolderName = folder.Name
                });
        }

        private void ValidateFolderForRemoval(Folder folder)
        {
            if (folder == null)
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.FolderFolderNotSet, new {Operation = "Remove"});
            }

            if (folder.Parent == null || !this.Equals(folder.Parent))
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.FolderFolderWithInvalidParent, new
                        {
                            Operation = "Remove",
                            FolderId = folder.Id,
                            FolderName = folder.Name
                        });
            }

            if (this.folders.Contains(folder)) return;

            throw new DomainValidationException(DomainValidationErrorCode.FolderAttemptToRemoveNonExistingFolder, new
                {
                    FolderId = folder.Id,
                    FolderName = folder.Name
                });
        }

        private static IReadOnlyCollection<Folder> GetListOfAllChildrenIncludingSelf(Folder root)
        {
            var list = new List<Folder> {root};
            foreach (var folder in root.Folders)
            {
                list.AddRange(GetListOfAllChildrenIncludingSelf(folder));
            }
            return list;
        }

        private static int GenerateId(IEntityIdGenerator<int> idGenerator)
        {
            if (idGenerator == null)
                throw new DomainValidationException(DomainValidationErrorCode.FolderIdGeneratorNotSet);

            return idGenerator.Next<Folder>();
        }
    }
}