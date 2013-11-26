/*******************************************************************************************************************************
 * AK.To|Do.Services.CategoryService
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
using AK.ToDo.Contracts.Services;
using AK.ToDo.Contracts.Services.Data.CategoryContracts;
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
    /// Implementation of ICategoryService (operations related to storage and retrieval of To-Do item categories).
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof(ICategoryService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class CategoryService : ServiceBase, ICategoryService
    {
        [Import] private IAuthorizationService authorizationService;

        #region Methods - Public (ICategoryService)

        public ToDoCategory Get(Guid userId, Guid id)
        {
            this.authorizationService.AuthorizeCategoryOperation(userId, id);

            ToDoCategory category = null;

            this.Db
                .With(this.ToDoCategoryRepository)
                .Execute(unit =>
            {
                var categoryEntity = this.ToDoCategoryRepository.Get(id);
                if (categoryEntity == null) return;

                category = new ToDoCategory
                {
                    AppUserId = userId,
                    Id = id,
                    Name = categoryEntity.Name,
                    ParentId = categoryEntity.ParentId
                };
            });

            return category;
        }

        public IList<ToDoCategory> GetList(Guid userId)
        {
            IList<ToDoCategory> categoryList = null;

            this.Db
                .With(this.ToDoCategoryRepository)
                .Execute(unit =>
                {
                    var categoryEntityList = this.ToDoCategoryRepository
                        .GetList(q => q.Where(x => x.AppUserId == userId));

                    categoryList = categoryEntityList
                        .Select(x => new ToDoCategory
                        {
                            AppUserId = userId,
                            Id = x.Id,
                            Name = x.Name,
                            ParentId = x.ParentId
                        })
                        .ToList();
                });

            var rootList = categoryList.Where(x => !x.ParentId.HasValue).ToList();
            var returnList = new List<ToDoCategory>();

            this.OrderAndAssignLevel(categoryList, rootList, returnList, 0);

            return returnList;
        }

        public ToDoCategory Update(ToDoCategory category)
        {
            this.authorizationService.AuthorizeCategoryOperation(category.AppUserId, category.Id);

            if (category.Id != Guid.Empty && category.Id == category.ParentId)
                throw new ServiceException(ServiceErrorCodes.CategoryParentIdSameAsId);

            var categoryEntity = new Entities.ToDoCategory
            {
                AppUserId = category.AppUserId,
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId
            };

            this.Db
                .With(this.ToDoCategoryRepository)
                .Execute(unit => this.ToDoCategoryRepository.Save(categoryEntity));

            category.Id = categoryEntity.Id;

            return category;
        }

        public IList<ToDoCategory> UpdateList(IList<ToDoCategory> categoryList)
        {
            categoryList.ForEach(x => this.authorizationService.AuthorizeCategoryOperation(x.AppUserId, x.Id));

            var categoryEntityList = categoryList
                .Select(x => new Entities.ToDoCategory
                {
                    AppUserId = x.AppUserId,
                    Id = x.Id,
                    Name = x.Name,
                    ParentId = x.ParentId
                })
                .ToList();

            this.Db
                .With(this.ToDoCategoryRepository)
                .Execute(unit =>
                {
                    foreach (var categoryEntity in categoryEntityList)
                        this.ToDoCategoryRepository.Save(categoryEntity);
                });

            return categoryEntityList
                .Select(x => new ToDoCategory
                {
                    AppUserId = x.AppUserId,
                    Id = x.Id,
                    Name = x.Name,
                    ParentId = x.ParentId
                })
                .ToList();
        }

        public void Delete(ToDoCategory category)
        {
            this.authorizationService.AuthorizeCategoryOperation(category.AppUserId, category.Id);
            
            this.DeleteCategory(category, false);
        }

        public void DeleteList(IList<ToDoCategory> categoryList)
        {
            categoryList.ForEach(x => this.authorizationService.AuthorizeCategoryOperation(x.AppUserId, x.Id));
            
            this.DeleteCategoryList(categoryList, false);
        }

        public void DeleteById(Guid userId, Guid id)
        {
            var category = this.Get(userId, id);

            if (category == null) return;

            this.DeleteCategory(category, true);
        }

        public void DeleteListByIdList(Guid userId, IList<Guid> idList)
        {
            idList.ForEach(x => this.authorizationService.AuthorizeCategoryOperation(userId, x));

            IList<ToDoCategory> categoryList = null;

            this.Db
                .With(this.ToDoCategoryRepository)
                .Execute(unit =>
            {
                categoryList = this.ToDoCategoryRepository
                    .GetList(q => q.Where(x => idList.Contains(x.Id)))
                    .Select(
                        x => new ToDoCategory
                        {
                            Id = x.Id, 
                            AppUserId = x.AppUserId, 
                            Name = x.Name, 
                            ParentId = x.ParentId
                        })
                    .ToList();
            });

            if (categoryList == null) return;
            categoryList = categoryList.Where(x => x != null).ToList();

            this.DeleteCategoryList(categoryList, true);
        }

        #endregion

        #region Methods - Private

        private void OrderAndAssignLevel(IEnumerable<ToDoCategory> masterList,
            IEnumerable<ToDoCategory> sourceList, ICollection<ToDoCategory> targetList, int level)
        {
            var fullList = masterList.ToList();

            sourceList.ForEach(x =>
            {
                x.Level = level;
                var newSourceList = fullList.Where(y => y.ParentId == x.Id).ToList();
                targetList.Add(x);
                this.OrderAndAssignLevel(fullList, newSourceList, targetList, level + 1);
            });
        }

        private void DeleteCategory(ToDoCategory category, bool skipExistCheck)
        {
            this.Db
                .With(this.ToDoCategoryRepository)
                .Execute(unit =>
                {
                    var exists = true;
                    if (!skipExistCheck)
                        exists = this.ToDoCategoryRepository.GetFor(q => q.Any(x => x.Id == category.Id));

                    if (!exists) return;

                    var entity = new Entities.ToDoCategory
                    {
                        AppUserId = category.AppUserId,
                        Id = category.Id,
                        Name = category.Name,
                        ParentId = category.ParentId
                    };

                    this.ToDoCategoryRepository.Delete(entity);
                });            
        }

        private void DeleteCategoryList(IEnumerable<ToDoCategory> categoryList, bool skipExistCheck)
        {
            this.Db
                .With(this.ToDoCategoryRepository)
                .Execute(unit =>
                {
                    foreach (var category in categoryList)
                    {
                        var exists = true;
                        if (!skipExistCheck)
                        {
                            var eachCategory = category;
                            exists = this.ToDoCategoryRepository.GetFor(q => q.Any(x => x.Id == eachCategory.Id));
                        }

                        if (!exists) return;

                        var entity = new Entities.ToDoCategory
                        {
                            AppUserId = category.AppUserId,
                            Id = category.Id,
                            Name = category.Name,
                            ParentId = category.ParentId
                        };

                        this.ToDoCategoryRepository.Delete(entity);
                    }
                });
        }

        #endregion
    }
}