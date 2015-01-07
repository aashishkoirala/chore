/*******************************************************************************************************************************
 * AK.Chore.Application.Services.FilterAccessService
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

using AK.Chore.Application.Aspects;
using AK.Chore.Application.Mappers;
using AK.Chore.Contracts;
using AK.Chore.Contracts.FilterAccess;
using AK.Chore.Domain.Filters;
using AK.Chore.Domain.Users;
using AK.Commons;
using AK.Commons.Configuration;
using AK.Commons.DataAccess;
using AK.Commons.DomainDriven;
using AK.Commons.Logging;
using AK.Commons.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Filter = AK.Chore.Contracts.FilterAccess.Filter;

#endregion

namespace AK.Chore.Application.Services
{
    /// <summary>
    /// Service implementation - IFilterAccessService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (IFilterAccessService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class FilterAccessService : ServiceBase, IFilterAccessService
    {
        [ImportingConstructor]
        public FilterAccessService(
            [Import] IAppDataAccess appDataAccess,
            [Import] IAppConfig appConfig,
            [Import] IAppLogger logger,
            [Import] IProviderSource<IEntityIdGeneratorProvider> entityIdGeneratorProvider)
            : base(appDataAccess, appConfig, logger, entityIdGeneratorProvider)
        {
        }

        [CatchToReturn("We had a problem getting the filters for that user.")]
        public OperationResult<IReadOnlyCollection<Filter>> GetFiltersForUser(int userId)
        {
            OperationResult<IReadOnlyCollection<Filter>> result = null;

            this.Execute((filterRepository, userRepository) =>
                {
                    var user = userRepository.Get(userId);
                    if (user == null)
                    {
                        result = new OperationResult<IReadOnlyCollection<Filter>>(
                            FilterAccessResult.FilterUserDoesNotExist, userId);
                        return;
                    }

                    user.LoadFilters(filterRepository);
                    result = new OperationResult<IReadOnlyCollection<Filter>>(
                        user.Filters.Select(x => x.Map()).ToArray());
                });

            return result;
        }

        [CatchToReturn("We had a problem getting that filter.")]
        public OperationResult<Filter> GetFilter(int id, int userId)
        {
            OperationResult<Filter> result = null;

            this.Execute((filterRepository, userRepository) =>
                {
                    var filter = filterRepository.Get(id, userRepository);
                    if (filter == null)
                    {
                        result = new OperationResult<Filter>(FilterAccessResult.FilterDoesNotExist, id);
                        return;
                    }

                    result = filter.User.Id != userId
                                 ? new OperationResult<Filter>(GeneralResult.NotAuthorized, id)
                                 : new OperationResult<Filter>(filter.Map());
                });

            return result;
        }

        [CatchToReturn("We had a problem saving that filter.")]
        public OperationResult<Filter> SaveFilter(Filter filter, int userId)
        {
            if (filter == null) throw new ArgumentNullException("filter");

            OperationResult<Filter> result = null;

            this.Execute((filterRepository, userRepository) =>
                {
                    result = this.MapAndSave(filter, userId, filterRepository, userRepository);
                });

            return result;
        }

        [CatchToReturnMany("We had a problem saving those filters.")]
        public OperationResults<Filter> SaveFilters(IReadOnlyCollection<Filter> filters, int userId)
        {
            if (filters == null) throw new ArgumentNullException("filters");

            var results = new Collection<OperationResult<Filter>>();

            this.Execute((filterRepository, userRepository) =>
                {
                    foreach (var filter in filters)
                        results.Add(this.MapAndSave(filter, userId, filterRepository, userRepository));
                });

            return new OperationResults<Filter>(results);
        }

        [CatchToReturn("We had a problem deleting that filter.")]
        public OperationResult DeleteFilter(int id, int userId)
        {
            OperationResult result = null;

            this.Execute((filterRepository, userRepository) =>
                {
                    result = Delete(id, userId, filterRepository, userRepository);
                });

            return result;
        }

        [CatchToReturnMany("We had a problem deleting those filters.")]
        public OperationResults DeleteFilters(IReadOnlyCollection<int> ids, int userId)
        {
            if (ids == null) throw new ArgumentNullException("ids");

            var results = new Collection<OperationResult>();

            this.Execute((filterRepository, userRepository) =>
                {
                    foreach (var id in ids)
                        results.Add(Delete(id, userId, filterRepository, userRepository));
                });

            return new OperationResults(results);
        }

        private OperationResult<Filter> MapAndSave(
            Filter filter, int userId,
            IFilterRepository filterRepository, IUserRepository userRepository)
        {
            if (filter.UserId != userId)
                return new OperationResult<Filter>(GeneralResult.NotAuthorized, filter.Id);

            if (filterRepository.ExistsForUser(userId, filter.Name, filter.Id))
                return new OperationResult<Filter>(FilterAccessResult.FilterAlreadyExists, filter.Id);

            var entity = filter.Map(
                this.IdGenerator,
                id => filterRepository.Get(id, userRepository),
                userRepository.Get);

            if (entity == null)
            {
                return filter.Id > 0
                           ? new OperationResult<Filter>(FilterAccessResult.FilterDoesNotExist, filter.Id)
                           : new OperationResult<Filter>(FilterAccessResult.FilterUserDoesNotExist, userId);
            }

            filterRepository.Save(entity);
            filter.Id = entity.Id;

            return new OperationResult<Filter>(filter);
        }

        private static OperationResult Delete(
            int id, int userId,
            IFilterRepository filterRepository, IUserRepository userRepository)
        {
            var filterCount = filterRepository.GetCountForUser(userId);
            if (filterCount == 1)
                return new OperationResult(FilterAccessResult.CannotDeleteOnlyFilter, id);

            var filter = filterRepository.Get(id, userRepository);
            if (filter == null)
                return new OperationResult(FilterAccessResult.FilterDoesNotExist, id);

            if (filter.User.Id != userId)
                return new OperationResult(GeneralResult.NotAuthorized, id);

            filterRepository.Delete(filter);
            return new OperationResult();
        }

        private void Execute(Action<IFilterRepository, IUserRepository> action)
        {
            this.Db.Execute(action);
        }
    }
}