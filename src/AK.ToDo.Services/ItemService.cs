/*******************************************************************************************************************************
 * AK.To|Do.Services.ItemService
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
using AK.Commons.DataAccess;
using AK.ToDo.Contracts.Services.Data.ItemContracts;
using AK.ToDo.Contracts.Services.Operation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Entities = AK.ToDo.Contracts.DataAccess.Entities;

#endregion

namespace AK.ToDo.Services
{
    /// <summary>
    /// Implementation of IItemService (operations related to import of To-Do items).
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof(IItemService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class ItemService : ServiceBase, IItemService
    {
        [Import] private IAuthorizationService authorizationService;

        #region Methods - Public (IItemService)

        public ToDoItem Get(Guid userId, Guid id)
        {
            this.authorizationService.AuthorizeItemOperation(userId, id);

            Entities.ToDoItem itemEntity = null;
            IList<Entities.ItemCategory> itemCategoryEntityList = null;
            this.Db
                .With(this.ToDoItemRepository, this.ItemCategoryRepository)
                .Execute(unit =>
                {
                    itemEntity = this.ToDoItemRepository.Get(id);
                    itemCategoryEntityList = this.ItemCategoryRepository
                        .GetList(q => q.Where(x => x.ItemId == id))
                        .ToList();
                });

            if (itemEntity == null) return null;

            var item = MapToDataContract(itemEntity);
            item.CategoryIdList = itemCategoryEntityList.Select(x => x.CategoryId).ToList();

            return item;
        }

        public IList<ToDoItem> GetList(ToDoItemListRequest request)
        {
            // If we're looking for "Completed" items, only show Completed state.
            // If not, and we don't have any state being looked for, the default is
            // everything that is not done.
            //
            var stateList =
                request.Type == ItemListType.Completed
                    ? new List<ToDoItemState> {ToDoItemState.Done}
                    : (request.State.HasValue
                           ? new List<ToDoItemState> {request.State.Value}
                           : new List<ToDoItemState>
                           {
                               ToDoItemState.NotStarted,
                               ToDoItemState.Paused,
                               ToDoItemState.InProgress
                           });

            var stateTextList = stateList.Select(x => x.ToString()).ToList();

            IList<Entities.ToDoItem> itemEntityList = null;
            IDictionary<Guid, List<Guid>> itemCategoryList = null;

            this.Db
                .With(this.ToDoItemRepository, this.ItemCategoryRepository)
                .Execute(unit =>
                {
                    itemEntityList = this.ToDoItemRepository
                        .GetList(q =>
                        {
                            var query = q
                                .Where(x =>
                                       x.AppUserId == request.AppUserId &&
                                       stateTextList.Contains(x.State));

                            return BuildListQuery(query, request);
                        })
                        .ToList();

                    var idList = itemEntityList.Select(x => x.Id).ToList();

                    // Map categories.
                    //
                    itemCategoryList = this.ItemCategoryRepository
                        .GetList(q => q.Where(x => idList.Contains(x.ItemId)))
                        .Select(x => new {x.ItemId, x.CategoryId})
                        .GroupBy(x => x.ItemId)
                        .ToDictionary(x => x.Key, x => x.Select(y => y.CategoryId).ToList());
                });

            var itemList = itemEntityList
                .OrderBy(GetItemDateToUseForOrdering)
                .Select(MapToDataContract).ToList();

            itemList.ForEach(x =>
            {
                x.CategoryIdList = itemCategoryList.ContainsKey(x.Id)
                                       ? itemCategoryList[x.Id]
                                       : new List<Guid>();

                x.IsLate = x.State != ToDoItemState.Done &&
                           (x.ScheduledEndDate < request.Today ||
                            (x.ScheduledStartDate.HasValue &&
                             x.ScheduledStartDate < request.Today && x.State != ToDoItemState.InProgress));
            });

            return itemList;
        }
        
        public ToDoItem Update(ToDoItem item)
        {
            this.authorizationService.AuthorizeItemOperation(item.AppUserId, item.Id);

            var itemEntity = MapToEntity(item);

            var itemCategoryEntityList = new List<Entities.ItemCategory>();
            if (item.CategoryIdList != null)
            {
                itemCategoryEntityList = item.CategoryIdList
                    .Select(x => new Entities.ItemCategory {CategoryId = x, ItemId = item.Id})
                    .ToList();
            }

            this.Db
                .With(this.ToDoItemRepository, this.ItemCategoryRepository)
                .Execute(unit =>
                {
                    // Replace existing categories with new ones first, before saving
                    // the actual item.

                    var existingItemCategories =
                        this.ItemCategoryRepository.GetList(q => q.Where(x => x.ItemId == item.Id));

                    foreach (var itemCategory in existingItemCategories)
                        this.ItemCategoryRepository.Delete(itemCategory);

                    this.ToDoItemRepository.Save(itemEntity);

                    itemCategoryEntityList.ForEach(x => x.ItemId = itemEntity.Id);
                    itemCategoryEntityList.ForEach(this.ItemCategoryRepository.Save);
                });

            item.Id = itemEntity.Id;
            return this.Get(item.AppUserId, item.Id);
        }

        public IList<ToDoItem> UpdateList(IList<ToDoItem> itemList)
        {
            // TODO: Make this more transactional.
            //
            return itemList.Select(this.Update).ToList();
        }

        public void Delete(ToDoItem item)
        {
            this.authorizationService.AuthorizeItemOperation(item.AppUserId, item.Id);

            var itemEntity = MapToEntity(item);

            this.Db
                .With(this.ToDoItemRepository, this.ItemCategoryRepository)
                .Execute(unit =>
                {
                    var existingItemCategories =
                        this.ItemCategoryRepository.GetList(q => q.Where(x => x.ItemId == item.Id));

                    existingItemCategories.ForEach(this.ItemCategoryRepository.Delete);

                    this.ToDoItemRepository.Delete(itemEntity);
                });
        }

        public void DeleteList(IList<ToDoItem> itemList)
        {
            // TODO: Make this more transactional.
            //
            itemList.ForEach(this.Delete);
        }

        public void DeleteById(Guid userId, Guid id)
        {
            var item = this.Get(userId, id);
            if (item == null) return;

            this.Delete(item);
        }

        public void DeleteListByIdList(Guid userId, IList<Guid> idList)
        {
            // TODO: Make this more transactional.
            //
            idList.ForEach(x => this.DeleteById(userId, x));
        }

        #endregion

        #region Methods - Private

        private static IEnumerable<Entities.ToDoItem> BuildListQuery(
            IQueryable<Entities.ToDoItem> query, ToDoItemListRequest request)
        {
            if (request.Type == ItemListType.Other)
            {
                if (request.ScheduledStartDate.HasValue)
                    query = query.Where(x => x.ScheduledStartDate <= request.ScheduledStartDate.Value);
                if (request.ScheduledEndDate.HasValue)
                    query = query.Where(x => x.ScheduledEndDate <= request.ScheduledEndDate.Value);
                if (request.ActualStartDate.HasValue)
                    query = query.Where(x => x.ActualStartDate <= request.ActualStartDate.Value);
                if (request.ActualEndDate.HasValue)
                    query = query.Where(x => x.ActualEndDate <= request.ActualEndDate.Value);

                return query;
            }

            switch (request.Type)
            {
                case ItemListType.Today:
                    query = query.Where(x => x.ScheduledStartDate <= request.Today ||
                                             x.ScheduledEndDate <= request.Today);
                    break;

                case ItemListType.DueToday:
                    query = query.Where(x => x.ScheduledEndDate <= request.Today);
                    break;

                case ItemListType.StartToday:
                    query = query.Where(x => x.ScheduledStartDate <= request.Today);
                    break;

                case ItemListType.ThisWeek:
                    var weekStartDate = request.Today.AddDays(-1*(int) request.Today.DayOfWeek);
                    var weekEndDate = weekStartDate.AddDays(7);

                    query = query.Where(x =>
                                        x.ScheduledStartDate <= weekEndDate ||
                                        x.ScheduledEndDate <= weekEndDate);
                    break;
            }

            return query;
        }

        private static DateTime GetItemDateToUseForOrdering(Entities.ToDoItem item)
        {
            if (!item.ScheduledStartDate.HasValue) return item.ScheduledEndDate;

            return item.ScheduledEndDate < item.ScheduledStartDate.Value
                       ? item.ScheduledEndDate
                       : item.ScheduledStartDate.Value;
        }

        private static Entities.ToDoItem MapToEntity(ToDoItem item)
        {
            return new Entities.ToDoItem
            {
                Id = item.Id,
                State = item.State.ToString(),
                Description = item.Description,
                ScheduledStartDate = item.ScheduledStartDate,
                ScheduledEndDate = item.ScheduledEndDate,
                ActualStartDate = item.ActualStartDate,
                ActualEndDate = item.ActualEndDate,
                AppUserId = item.AppUserId
            };            
        }

        private static ToDoItem MapToDataContract(Entities.ToDoItem itemEntity)
        {
            return new ToDoItem
            {
                Id = itemEntity.Id,
                AppUserId = itemEntity.AppUserId,
                ScheduledStartDate = itemEntity.ScheduledStartDate,
                ScheduledEndDate = itemEntity.ScheduledEndDate,
                ActualStartDate = itemEntity.ActualStartDate,
                ActualEndDate = itemEntity.ActualEndDate,
                Description = itemEntity.Description,
                State = (ToDoItemState) Enum.Parse(typeof (ToDoItemState), itemEntity.State)
            };
        }

        #endregion
    }
}