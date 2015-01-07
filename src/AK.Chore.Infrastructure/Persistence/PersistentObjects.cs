/*******************************************************************************************************************************
 * AK.Chore.Infrastructure.Persistence.PersistentObjects
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

namespace AK.Chore.Infrastructure.Persistence
{
    /// <summary>
    /// Represents an item in the User MongoDB collection.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class User
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Nickname { get; set; }
    }

    /// <summary>
    /// Represents an item in the Folder MongoDB collection.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Folder
    {
        public int Id { get; set; }
        public int? ParentFolderId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Represents a criterion as stored within an item in the Filter MongoDB collection.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Criterion
    {
        public bool IsTrue { get; set; }
        public bool IsComplex { get; set; }
        public bool IsRecurrence { get; set; }
        public Domain.Filters.Field Field { get; set; }
        public Domain.Filters.Operator Operator { get; set; }
        public Domain.Filters.FieldValue Value { get; set; }
        public string Literal { get; set; }
        public Criterion Criterion1 { get; set; }
        public Domain.Filters.Conjunction Conjunction { get; set; }
        public Criterion Criterion2 { get; set; }
        public Domain.Filters.RecurrenceCriterion.LiteralOrSpecialDate RecurrenceDateEquals { get; set; }
        public Domain.Filters.RecurrenceCriterion.LiteralOrSpecialDate[] RecurrenceDateIn { get; set; }
        public Domain.Filters.RecurrenceCriterion.LiteralOrSpecialDate RecurrenceDateOnOrBefore { get; set; }
        public Domain.Filters.RecurrenceCriterion.LiteralOrSpecialDate RecurrenceDateOnOrAfter { get; set; }
    }

    /// <summary>
    /// Represents an item in the Filter MongoDB collection.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Filter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public Criterion Criterion { get; set; }
    }

    /// <summary>
    /// Represents a Recurrence object within an item in the Task MongoDB collection.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Recurrence
    {
        public bool IsEnabled { get; set; }
        public Domain.Tasks.RecurrenceType Type { get; set; }
        public int Interval { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public string DaysOfWeek { get; set; }
        public int DayOfMonth { get; set; }
        public int MonthOfYear { get; set; }
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Represents an item in the Task MongoDB collection.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Task
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int FolderId { get; set; }
        public int UserId { get; set; }
        public Domain.Tasks.TaskState State { get; set; }
        public Recurrence Recurrence { get; set; }
        public bool IsMundane { get; set; }
    }
}