/*******************************************************************************************************************************
 * AK.Chore.Domain.DomainValidationErrorCode
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

using AK.Commons;

namespace AK.Chore.Domain
{
    /// <summary>
    /// Domain-level validation error codes.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public enum DomainValidationErrorCode
    {
        [EnumDescription("IdGenerator must be specified for Filter")]
        FilterIdGeneratorNotSet,
        [EnumDescription("User must be specified for Filter")]
        FilterUserNotSet,
        [EnumDescription("Criterion must be specified for Filter")]
        FilterCriterionNotSet,
        [EnumDescription("Filter Name cannot be empty")]
        FilterNameEmpty,

        [EnumDescription("IdGenerator must be specified for Folder")]
        FolderIdGeneratorNotSet,
        [EnumDescription("User must be specified for Folder")]
        FolderUserNotSet,
        [EnumDescription("Parent must be specified for Folder")]
        FolderParentNotSet,
        [EnumDescription("Folder Name cannot be empty")]
        FolderNameEmpty,
        [EnumDescription("Cannot operate on Task from another Folder")]
        FolderTaskWithInvalidFolder,
        [EnumDescription("Task not specified for Folder operation")]
        FolderTaskNotSet,
        [EnumDescription("Folder not specified for Folder operation")]
        FolderFolderNotSet,
        [EnumDescription("Task being added already exists in Folder")]
        FolderAttemptToAddExistingTask,
        [EnumDescription("Task being removed does not exist in Folder")]
        FolderAttemptToRemoveNonExistingTask,
        [EnumDescription("Folder being added already exists in Folder")]
        FolderAttemptToAddExistingFolder,
        [EnumDescription("Folder being removed does not exist in Folder")]
        FolderAttemptToRemoveNonExistingFolder,
        [EnumDescription("Cannot move Folder to another User's Folder")]
        FolderAttemptToMoveToAnotherUsersFolder,
        [EnumDescription("Cannot operate on Folder from another parent Folder")]
        FolderFolderWithInvalidParent,

        [EnumDescription("IdGenerator must be specified for Task")]
        TaskIdGeneratorNotSet,
        [EnumDescription("Folder must be specified for Task")]
        TaskFolderNotSet,
        [EnumDescription("Task Description cannot be empty")]
        TaskDescriptionEmpty,
        [EnumDescription("Task must have End Date assigned if it is not Recurring")]
        TaskNonRecurringWithoutEndDate,
        [EnumDescription("This operation is not valid for this Task")]
        TaskInvalidStateForOperation,
        [EnumDescription("This operation is only valid for recurring Tasks")]
        TaskRecurringOperationOnNonRecurringTask,
        [EnumDescription("Task cannot be moved to Folder belonging to another User")]
        TaskAttemptToMoveToAnotherUsersFolder,
        [EnumDescription("Task Start Date cannot be later than End Date")]
        TaskStartDateLaterThanEndDate,
        [EnumDescription("Task Start Date must be set if Start Time is set")]
        TaskStartTimeSetWithoutStartDate,
        [EnumDescription("Task End Date must be set if End Time is set")]
        TaskEndTimeSetWithoutEndDate,
        [EnumDescription("Non-recurring task cannot be set as mundane")]
        TaskNonRecurringCannotBeMundane,
        [EnumDescription("Task must be non-recurring and non-started to transition")]
        TaskCannotTransition,

        [EnumDescription("IdGenerator must be specified for User")]
        UserIdGeneratorNotSet,
        [EnumDescription("UserKeyGenerator must be specified for User")]
        UserKeyGeneratorNotSet,
        [EnumDescription("BuiltInFilterProvider must be specified for User")]
        UserBuiltInFilterProviderNotSet,
        [EnumDescription("UserName cannot be empty")]
        UserNameEmpty,
        [EnumDescription("User Nickname cannot be empty")]
        UserNicknameEmpty,
        [EnumDescription("Cannot operate on Folder from another User")]
        UserFolderWithInvalidUser,
        [EnumDescription("Folder being added already exists for User")]
        UserAttemptToAddExistingFolder,
        [EnumDescription("Folder being removed does not exist for User")]
        UserAttemptToRemoveNonExistingFolder,
        [EnumDescription("Cannot operate on Filter from another User")]
        UserFilterWithInvalidUser,
        [EnumDescription("Filter being added already exists for User")]
        UserAttemptToAddExistingFilter,
        [EnumDescription("Filter being removed does not exist for User")]
        UserAttemptToRemoveNonExistingFilter,
        [EnumDescription("Folder not specified for User operation")]
        UserFolderNotSet,
        [EnumDescription("Filter not specified for User operation")]
        UserFilterNotSet,

        [EnumDescription("TaskGrouper problem with BuildPredicate")]
        TaskGrouperPredicateBuildError,
        [EnumDescription("TaskGrouper problem with TaskSatisfiesFilter")]
        TaskGrouperSatisfactionComputationError
    }
}