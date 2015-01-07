/*******************************************************************************************************************************
 * AK.Chore.Contracts.TaskAccess.Task
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

using System;

namespace AK.Chore.Contracts.TaskAccess
{
    /// <summary>
    /// Represents a task.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Task
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FolderId { get; set; }
        public string FolderPath { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string State { get; set; }
        public Recurrence Recurrence { get; set; }
        public bool IsMundane { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsLate { get; set; }
        public bool CanStart { get; set; }
        public bool CanPause { get; set; }
        public bool CanResume { get; set; }
        public bool CanComplete { get; set; }
        public DateTime Now { get; set; }
        public string DateOrRecurrenceSummary { get; set; }
    }
}