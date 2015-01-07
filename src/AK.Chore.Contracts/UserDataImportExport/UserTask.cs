/*******************************************************************************************************************************
 * AK.Chore.Contracts.UserDataImportExport.UserTask
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
using System;

#endregion

namespace AK.Chore.Contracts.UserDataImportExport
{
    /// <summary>
    /// Represents imported/exported user task information.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class UserTask
    {
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string State { get; set; }
        public Recurrence Recurrence { get; set; }
        public bool IsMundane { get; set; }
        public bool IsRecurring { get; set; }
    }
}