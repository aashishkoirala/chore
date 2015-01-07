/*******************************************************************************************************************************
 * AK.Chore.Contracts.TaskImportExport.TaskImportExportResult
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

namespace AK.Chore.Contracts.TaskImportExport
{
    /// <summary>
    /// Result codes for ITaskImportExportService operations.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public enum TaskImportExportResult
    {
        [EnumDescription("No tasks specified to export.")]
        NoTasksToExport,

        [EnumDescription("No tasks specified to import.")]
        NoTasksToImport,

        [EnumDescription("The import line for this task is invalid and could not be imported. Refer to Key for content of line.")]
        CannotImportTask
    }
}