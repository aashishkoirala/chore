/*******************************************************************************************************************************
 * AK.Chore.Presentation.Api.TransferObjects
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

using AK.Chore.Contracts.TaskAccess;
using AK.Commons.Services;

#endregion

namespace AK.Chore.Presentation.Api
{
    /// <summary>
    /// Task list query.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class TaskQuery
    {
        public bool UseUnsavedFilter { get; set; }
        public int FilterId { get; set; }
    }

    /// <summary>
    /// Update user-nickname command.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class UpdateUserNicknameCommand
    {
        public string Nickname { get; set; }
    }

    /// <summary>
    /// The "check if task satisfies filter" command.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class TaskSatisfiesFilterCommand
    {
        public int TaskId { get; set; }
        public int FilterId { get; set; }
        public bool UseUnsavedFilter { get; set; }
        public bool Satisfies { get; set; }
    }

    /// <summary>
    /// Import task command.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class TaskImportCommand
    {
        public string ImportData { get; set; }
        public OperationResults<Task> ImportResults { get; set; }
    }

    /// <summary>
    /// Export task command.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class TaskExportCommand
    {
        public int[] TaskIds { get; set; }
        public string ExportedData { get; set; }
    }

    /// <summary>
    /// Move task command.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class TaskMoveCommand
    {
        public int[] TaskIds { get; set; }
        public int FolderId { get; set; }
        public OperationResults MoveResults { get; set; }
    }

    /// <summary>
    /// Move folder command.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class FolderMoveCommand
    {
        public int FolderId { get; set; }
        public int? MoveToFolderId { get; set; }
    }
}