/*******************************************************************************************************************************
 * AK.To|Do.Services.LookupService
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

using AK.ToDo.Contracts.Services.Data.ItemContracts;
using AK.ToDo.Contracts.Services.Operation;
using System.Collections.Generic;
using System.ComponentModel.Composition;

#endregion

namespace AK.ToDo.Services
{
    /// <summary>
    /// Implementation of ILookupService (operations related to import of To-Do items).
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof(ILookupService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class LookupService : ServiceBase, ILookupService
    {
        public IDictionary<ToDoItemState, string> GetItemStateList()
        {
            return new Dictionary<ToDoItemState, string>
            {
                {ToDoItemState.NotStarted, "Pending"},
                {ToDoItemState.InProgress, "In Progress"},
                {ToDoItemState.Paused, "Paused"},
                {ToDoItemState.Done, "Done"}
            };
        }

        public IDictionary<ItemListType, string> GetItemListTypeList()
        {
            return new Dictionary<ItemListType, string>
            {
                {ItemListType.Today, "Today"},
                {ItemListType.DueToday, "Due Today"},
                {ItemListType.StartToday, "Start Today"},
                {ItemListType.ThisWeek, "This Week"},
                {ItemListType.Completed, "Completed"},
                {ItemListType.Other, "Other"}
            };
        }
   }
}