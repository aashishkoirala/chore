/*******************************************************************************************************************************
 * AK.To|Do.Services.ItemImportService
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
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

#endregion

namespace AK.ToDo.Services
{
    /// <summary>
    /// Implementation of IItemImportService (operations related to import of To-Do items).
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof(IItemImportService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class ItemImportService : ServiceBase, IItemImportService
    {
        [Import] private IItemService itemService;

        public IList<ToDoItem> Import(Guid userId, string importData)
        {
            if (string.IsNullOrWhiteSpace(importData))
            {
                this.Logger.Information(string.Format("User {0} tried to import an empty list.", userId));
                return new List<ToDoItem>();
            }

            var importLines = importData.Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);

            var itemList = importLines
                .Select(CreateItemFromImportLine)
                .Where(x => x != null)
                .ToList();

            this.Logger.Information(string.Format("Importing {0} items for user {1}...", itemList.Count, userId));

            itemList.ForEach(x => x.AppUserId = userId);

            return this.itemService.UpdateList(itemList);
        }

        private static ToDoItem CreateItemFromImportLine(string importLine)
        {
            var parts = importLine.Split('\t');

            var scheduledEndDate = DateTime.Today;
            DateTime? scheduledStartDate = null;

            var description = parts[0];
            if (string.IsNullOrWhiteSpace(description)) return null;

            if (parts.Length > 1) 
                DateTime.TryParse(parts[1], out scheduledEndDate);

            if (parts.Length > 2)
            {
                DateTime date;
                if (DateTime.TryParse(parts[2], out date))
                    scheduledStartDate = date;
            }

            return new ToDoItem
            {
                State = ToDoItemState.NotStarted,
                ScheduledEndDate = scheduledEndDate,
                ScheduledStartDate = scheduledStartDate,
                Description = description
            };
        }
    }
}