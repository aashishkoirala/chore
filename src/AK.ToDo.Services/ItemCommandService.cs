/*******************************************************************************************************************************
 * AK.To|Do.Services.ItemCommandService
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

using AK.Commons;
using AK.ToDo.Contracts.Services.Data.ItemContracts;
using AK.ToDo.Contracts.Services.Operation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

#endregion

namespace AK.ToDo.Services
{
    /// <summary>
    /// Implementation of IItemCommandService (operations related to commands performed on To-Do items).
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof(IItemCommandService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class ItemCommandService : ServiceBase, IItemCommandService
    {
        [Import] private IItemService itemService;

        public void Start(Guid userId, Guid id)
        {
            var item = this.itemService.Get(userId, id);
            if (item == null) return;
            if (item.State == ToDoItemState.InProgress) return;
            if (!item.ScheduledStartDate.HasValue) return;

            item.State = ToDoItemState.InProgress;
            item.ActualStartDate = DateTime.Today;

            this.itemService.Update(item);
        }

        public void Pause(Guid userId, Guid id)
        {
            var item = this.itemService.Get(userId, id);
            if (item == null) return;
            if (item.State == ToDoItemState.Paused) return;
            if (!item.ScheduledStartDate.HasValue) return;

            item.State = ToDoItemState.Paused;

            this.itemService.Update(item);
        }

        public void Complete(Guid userId, Guid id)
        {
            var item = this.itemService.Get(userId, id);
            if (item == null) return;
            if (item.State == ToDoItemState.Done) return;

            item.State = ToDoItemState.Done;
            item.ActualEndDate = DateTime.Today;

            this.itemService.Update(item);
        }

        public void StartList(Guid userId, IList<Guid> idList)
        {
            // TODO: Make it more transactional.
            //
            idList.ForEach(x => this.Start(userId, x));
        }

        public void PauseList(Guid userId, IList<Guid> idList)
        {
            // TODO: Make it more transactional.
            //
            idList.ForEach(x => this.Pause(userId, x));
        }

        public void CompleteList(Guid userId, IList<Guid> idList)
        {
            // TODO: Make it more transactional.
            //
            idList.ForEach(x => this.Complete(userId, x));
        }
    }
}