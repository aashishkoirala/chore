/*******************************************************************************************************************************
 * AK.Chore.Domain.Tasks.Task
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

using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Users;
using AK.Commons.DomainDriven;
using System;
using System.Text;

#endregion

namespace AK.Chore.Domain.Tasks
{
    /// <summary>
    /// Represents the domain of a task - that is what this application revolves around.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Task : IAggregateRoot<Task, int>
    {
        private string description;
        private DateTime? startDate;
        private TimeSpan? startTime;
        private DateTime? endDate;
        private TimeSpan? endTime;
        private TaskState state;
        private Recurrence recurrence;
        private bool isMundane;

        public int Id { get; protected set; }

        public string Description
        {
            get { return this.description; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new DomainValidationException(DomainValidationErrorCode.TaskDescriptionEmpty);

                this.description = value;
            }
        }

        public DateTime? StartDate
        {
            get { return this.startDate; }
            set
            {
                this.ValidateDatesAndTimes(value, this.endDate, this.startTime, this.endTime);
                this.startDate = value;
            }
        }

        public TimeSpan? StartTime
        {
            get { return this.startTime; }
            set
            {
                this.ValidateDatesAndTimes(this.startDate, this.endDate, value, this.endTime);
                this.startTime = value;
            }
        }

        public DateTime? EndDate
        {
            get { return this.endDate; }
            set
            {
                this.ValidateDatesAndTimes(this.startDate, value, this.startTime, this.endTime);
                this.endDate = value;
            }
        }

        public TimeSpan? EndTime
        {
            get { return this.endTime; }
            set
            {
                this.ValidateDatesAndTimes(this.startDate, this.endDate, this.startTime, value);
                this.endTime = value;
            }
        }

        public void SetDatesAndTimes(DateTime? endDate, TimeSpan? endTime, DateTime? startDate, TimeSpan? startTime)
        {
            this.ValidateDatesAndTimes(startDate, endDate, startTime, endTime);
            this.endDate = endDate;
            this.endTime = endTime;
            this.startDate = startDate;
            this.startTime = startTime;
        }

        public TaskState State
        {
            get { return this.state; }
            protected set
            {
                if (this.state == value) return;
                this.state = value;

                if (this.state == TaskState.Recurring &&
                    (this.recurrence == null || this.recurrence.Type == RecurrenceType.NonRecurring))
                {
                    this.recurrence = Recurrence.Daily(1);
                }

                if (this.state != TaskState.Recurring &&
                    (this.recurrence == null || this.recurrence.Type != RecurrenceType.NonRecurring))
                {
                    this.recurrence = Recurrence.NonRecurring();
                }
            }
        }

        public Folder Folder { get; protected set; }

        public User User
        {
            get { return this.Folder.User; }
        }

        public Recurrence Recurrence
        {
            get { return this.recurrence ?? Recurrence.NonRecurring(); }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (this.recurrence != null && this.recurrence.Equals(value)) return;
                this.recurrence = value;

                if (this.recurrence.Type != RecurrenceType.NonRecurring) this.state = TaskState.Recurring;
                else
                {
                    this.isMundane = false;
                    if (this.state == TaskState.Recurring) this.state = TaskState.NotStarted;
                }
            }
        }

        public bool IsMundane
        {
            get { return this.isMundane && this.IsRecurring; }
            set
            {
                if (!this.IsRecurring && value)
                {
                    throw new DomainValidationException(DomainValidationErrorCode.TaskNonRecurringCannotBeMundane);
                }
                this.isMundane = value;
            }
        }

        public bool IsRecurring
        {
            get
            {
                return this.state == TaskState.Recurring &&
                       this.Recurrence != null &&
                       this.Recurrence.Type != RecurrenceType.NonRecurring;
            }
        }

        public bool CanStart
        {
            get { return this.State == TaskState.NotStarted && this.StartDate.HasValue && !this.IsRecurring; }
        }

        public bool CanPause
        {
            get { return this.state == TaskState.InProgress && this.StartDate.HasValue && !this.IsRecurring; }
        }

        public bool CanResume
        {
            get { return this.State == TaskState.Paused && this.StartDate.HasValue && !this.IsRecurring; }
        }

        public bool CanComplete
        {
            get
            {
                return !((this.IsRecurring ||
                          this.State == TaskState.Completed ||
                          this.State == TaskState.Paused) ||
                         (this.StartDate.HasValue && this.State != TaskState.InProgress));
            }
        }

        public string DateOrRecurrenceSummary
        {
            get
            {
                if (this.IsRecurring) return this.Recurrence.Summary;

                var builder = new StringBuilder();
                builder.AppendFormat("Due on {0}", this.EndDate.GetValueOrDefault(DateTime.Now).ToString("dd MMM yyyy"));

                if (this.EndTime.HasValue)
                    builder.AppendFormat(" at {0}", this.EndTime.Value.ToString(@"hh\:mm"));

                if (this.StartDate.HasValue)
                    builder.AppendFormat(", start by {0}", this.StartDate.Value.ToString("dd MMM yyyy"));

                if (this.StartTime.HasValue)
                    builder.AppendFormat(" at {0}", this.StartTime.Value.ToString(@"hh\:mm"));

                return builder.ToString();
            }
        }

        protected Task(int id, string description, Folder folder)
        {
            if (folder == null)
                throw new DomainValidationException(DomainValidationErrorCode.TaskFolderNotSet);

            this.Id = id;
            this.Description = description;
            this.Folder = folder;
            this.State = TaskState.NotStarted;
            this.Folder.AddTask(this);
        }

        private Task(IEntityIdGenerator<int> idGenerator, string description, Folder folder)
            : this(GenerateId(idGenerator), description, folder)
        {
        }

        public Task(IEntityIdGenerator<int> idGenerator, string description, Folder folder, DateTime endDate,
                    TimeSpan? endTime = null) : this(idGenerator, description, folder)
        {
            this.EndDate = endDate;
            this.EndTime = endTime;
            this.Recurrence = Recurrence.NonRecurring();
        }

        public Task(IEntityIdGenerator<int> idGenerator, string description, Folder folder, DateTime endDate,
                    DateTime startDate, TimeSpan? endTime = null, TimeSpan? startTime = null)
            : this(idGenerator, description, folder)
        {
            this.EndDate = endDate;
            this.EndTime = endTime;
            this.StartDate = startDate;
            this.StartTime = startTime;
            this.Recurrence = Recurrence.NonRecurring();
        }

        public Task(IEntityIdGenerator<int> idGenerator, string description, Folder folder,
                    Recurrence recurrence) : this(idGenerator, description, folder)
        {
            this.State = TaskState.Recurring;
            this.Recurrence = recurrence;
        }

        public bool Equals(Task other)
        {
            return this.Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is Task && this.Equals(obj as Task);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public bool IsLate(DateTime now)
        {
            if (this.IsRecurring || this.State == TaskState.Completed) return false;

            if (this.EndDate.HasValue)
            {
                var endDateValue = this.EndDate.Value;
                if (this.EndTime.HasValue) endDateValue = endDateValue.Add(this.EndTime.Value);

                if (now > endDateValue && this.State != TaskState.Completed) return true;
            }

            if (this.StartDate.HasValue)
            {
                var startDateValue = this.StartDate.Value;
                if (this.StartTime.HasValue) startDateValue = startDateValue.Add(this.StartTime.Value);

                if (now > startDateValue && this.State != TaskState.Completed && this.State != TaskState.InProgress)
                    return true;
            }

            return false;
        }

        public void Start()
        {
            if (this.CanStart)
            {
                this.State = TaskState.InProgress;
                return;
            }

            throw new DomainValidationException(
                DomainValidationErrorCode.TaskInvalidStateForOperation,
                new
                    {
                        TaskId = this.Id,
                        this.State,
                        RecurrenceType = this.Recurrence.Type,
                        this.StartDate,
                        Operation = "Start"
                    });
        }

        public void Pause()
        {
            if (this.CanPause)
            {
                this.State = TaskState.Paused;
                return;
            }

            throw new DomainValidationException(
                DomainValidationErrorCode.TaskInvalidStateForOperation,
                new
                    {
                        TaskId = this.Id,
                        this.State,
                        RecurrenceType = this.Recurrence.Type,
                        this.StartDate,
                        Operation = "Pause"
                    });
        }

        public void Resume()
        {
            if (this.CanResume)
            {
                this.State = TaskState.InProgress;
                return;
            }

            throw new DomainValidationException(
                DomainValidationErrorCode.TaskInvalidStateForOperation,
                new
                    {
                        TaskId = this.Id,
                        this.State,
                        RecurrenceType = this.Recurrence.Type,
                        this.StartDate,
                        Operation = "Resume"
                    });
        }

        public void Complete()
        {
            if (this.CanComplete)
            {
                this.State = TaskState.Completed;
                return;
            }

            throw new DomainValidationException(
                DomainValidationErrorCode.TaskInvalidStateForOperation,
                new
                    {
                        TaskId = this.Id,
                        this.State,
                        RecurrenceType = this.Recurrence.Type,
                        this.StartDate,
                        Operation = "Complete"
                    });
        }

        public void EnableRecurrence()
        {
            if (this.IsRecurring && !this.Recurrence.IsEnabled)
            {
                this.Recurrence = Recurrence.Enabled(this.Recurrence);
                return;
            }

            throw new DomainValidationException(
                DomainValidationErrorCode.TaskRecurringOperationOnNonRecurringTask,
                new
                    {
                        TaskId = this.Id,
                        this.State,
                        RecurrenceType = this.Recurrence.Type,
                        Operation = "EnableRecurrence"
                    });
        }

        public void DisableRecurrence()
        {
            if (this.IsRecurring && this.Recurrence.IsEnabled)
            {
                this.Recurrence = Recurrence.Disabled(this.Recurrence);
                return;
            }

            throw new DomainValidationException(
                DomainValidationErrorCode.TaskRecurringOperationOnNonRecurringTask,
                new
                    {
                        TaskId = this.Id,
                        this.State,
                        RecurrenceType = this.Recurrence.Type,
                        Operation = "DisableRecurrence"
                    });
        }

        public void MoveTo(Folder folder)
        {
            if (!folder.User.Equals(this.Folder.User))
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.TaskAttemptToMoveToAnotherUsersFolder,
                    new
                        {
                            TaskId = this.Id,
                            FromFolderId = this.Folder.Id,
                            ToFolderId = folder.Id
                        });
            }

            if (this.Folder.AreTasksLoaded) this.Folder.RemoveTask(this);
            this.Folder = folder;
            folder.AddTask(this);
        }

        public void TransitionTo(TaskState state)
        {
            if (this.State != TaskState.NotStarted || state == TaskState.Recurring)
                throw new DomainValidationException(DomainValidationErrorCode.TaskCannotTransition);

            if (state == TaskState.NotStarted) return;

            switch (state)
            {
                case TaskState.InProgress:
                    this.Start();
                    break;

                case TaskState.Paused:
                    if (this.CanStart) this.Start();
                    this.Pause();
                    break;

                case TaskState.Completed:
                    if (this.CanStart) this.Start();
                    this.Complete();
                    break;
            }
        }

        private void ValidateDatesAndTimes(
            DateTime? startDate,
            DateTime? endDate,
            TimeSpan? startTime,
            TimeSpan? endTime)
        {
            if (startDate.HasValue && (!endDate.HasValue || endDate.Value < startDate.Value))
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.TaskStartDateLaterThanEndDate,
                    new
                        {
                            TaskId = this.Id,
                            StartDate = startDate.Value,
                            EndDate = endDate
                        });
            }

            if (!startDate.HasValue && startTime.HasValue)
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.TaskStartTimeSetWithoutStartDate,
                    new
                        {
                            TaskId = this.Id,
                            StartTime = startTime.Value
                        });
            }

            if (!endDate.HasValue && endTime.HasValue)
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.TaskEndTimeSetWithoutEndDate,
                    new
                        {
                            TaskId = this.Id,
                            EndTime = endTime.Value
                        });
            }

            if (!endDate.HasValue && !this.IsRecurring)
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.TaskNonRecurringWithoutEndDate,
                    new {TaskID = this.Id});
            }
        }

        private static int GenerateId(IEntityIdGenerator<int> idGenerator)
        {
            if (idGenerator == null)
                throw new DomainValidationException(DomainValidationErrorCode.TaskIdGeneratorNotSet);

            return idGenerator.Next<Task>();
        }
    }
}