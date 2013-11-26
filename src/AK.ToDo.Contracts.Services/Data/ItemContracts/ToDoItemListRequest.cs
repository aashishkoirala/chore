/*******************************************************************************************************************************
 * AK.To|Do.Contracts.Services.Data.ItemContracts.ToDoItemListRequest
 * Copyright © 2013 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of To Do.
 *  
 * To Do is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * To Do is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with To Do.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

#region Namespace Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

#endregion

namespace AK.ToDo.Contracts.Services.Data.ItemContracts
{
    /// <summary>
    /// Contains request parameters to get a list of To-Do items.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class ToDoItemListRequest
    {
        private const string DateFormat = "yyyy-MM-dd";

        public ToDoItemListRequest()
        {
            this.Today = DateTime.Today;
        }

        public ItemListType Type { get; set; }

        [IgnoreDataMember] public Guid AppUserId { get; set; }
        [IgnoreDataMember] public ToDoItemState? State { get; set; }
        [IgnoreDataMember] public DateTime? ScheduledStartDate { get; set; }
        [IgnoreDataMember] public DateTime? ScheduledEndDate { get; set; }
        [IgnoreDataMember] public DateTime? ActualStartDate { get; set; }
        [IgnoreDataMember] public DateTime? ActualEndDate { get; set; }
        [IgnoreDataMember] public DateTime Today { get; set; }
        [IgnoreDataMember] public IList<Guid> CategoryIdList { get; set; }

        // TODO: These different "AsString" properties are here because Web API does not let me easily control
        // TODO: how they are serialized and its default serialization does not play nice with my UI controls.
        // TODO: Since I have all the above properties set to IgnoreDataMember, this would get in the way, say,
        // TODO: if I wanted to WCF'ize my services. Come up with a way where we don't need to pollute the
        // TODO: data contract with all these derived properties.

        public string ScheduledStartDateAsString
        {
            get { return this.ScheduledStartDate.HasValue ? this.ScheduledStartDate.Value.ToString(DateFormat) : string.Empty; }
            set { this.ScheduledStartDate = string.IsNullOrWhiteSpace(value) ? (DateTime?)null : DateTime.Parse(value); }
        }

        public string ScheduledEndDateAsString
        {
            get { return this.ScheduledEndDate.HasValue ? this.ScheduledEndDate.Value.ToString(DateFormat) : string.Empty; }
            set { this.ScheduledEndDate = string.IsNullOrWhiteSpace(value) ? (DateTime?)null : DateTime.Parse(value); }
        }

        public string ActualStartDateAsString
        {
            get { return this.ActualStartDate.HasValue ? this.ActualStartDate.Value.ToString(DateFormat) : string.Empty; }
            set { this.ActualStartDate = string.IsNullOrWhiteSpace(value) ? (DateTime?)null : DateTime.Parse(value); }
        }

        public string ActualEndDateAsString
        {
            get { return this.ActualEndDate.HasValue ? this.ActualEndDate.Value.ToString(DateFormat) : string.Empty; }
            set { this.ActualEndDate = string.IsNullOrWhiteSpace(value) ? (DateTime?)null : DateTime.Parse(value); }
        } 

        public string StateAsString
        {
            get { return this.State.HasValue ? this.State.Value.ToString() : string.Empty; }
            set { this.State = string.IsNullOrWhiteSpace(value) ? (ToDoItemState?)null : (ToDoItemState)Enum.Parse(typeof(ToDoItemState), value); }
        }

        public string CategoryIdListAsString
        {
            get
            {
                return this.CategoryIdList == null
                           ? string.Empty
                           : string.Join(",", this.CategoryIdList.Select(x => x.ToString()).ToArray());
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    this.CategoryIdList = null;
                    return;
                }
                this.CategoryIdList = value.Split(',').Select(Guid.Parse).ToList();
            }
        }

        public string TodayAsString
        {
            get { return this.Today.ToString(DateFormat); }
            set { this.Today = DateTime.Parse(value); }
        }
    }
}