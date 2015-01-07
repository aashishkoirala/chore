/*******************************************************************************************************************************
 * AK.Chore.Infrastructure.Persistence.FilterRepository
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

using AK.Chore.Domain.Users;
using AK.Commons.DomainDriven;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

#endregion

namespace AK.Chore.Infrastructure.Persistence
{
    /// <summary>
    /// Implementation of IFilterRepository that uses MongoDB.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (Domain.Filters.IFilterRepository)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class FilterRepository : DomainRepositoryBase, Domain.Filters.IFilterRepository
    {
        public IReadOnlyCollection<Domain.Filters.Filter> ListForUser(Domain.Users.User user)
        {
            return this.UnitOfWork.Repository<Filter>().Query
                       .Where(x => x.UserId == user.Id)
                       .ToArray()
                       .Select(x => Map(x, user))
                       .ToArray();
        }

        public Domain.Filters.Filter Get(int id, IUserRepository userRepository)
        {
            var filter = this.UnitOfWork.Repository<Filter>().Query.SingleOrDefault(x => x.Id == id);
            if (filter == null) return null;

            var user = userRepository.Get(filter.UserId);
            return Map(filter, user);
        }

        public void Save(Domain.Filters.Filter filter)
        {
            this.UnitOfWork.Repository<Filter>().Save(Map(filter));
        }

        public void Delete(Domain.Filters.Filter filter)
        {
            this.UnitOfWork.Repository<Filter>().Delete(Map(filter));
        }

        public int GetCountForUser(int userId)
        {
            return this.UnitOfWork
                       .Repository<Filter>()
                       .Query
                       .Count(x => x.UserId == userId);
        }

        public bool ExistsForUser(int userId, string name, int idToExclude)
        {
            return this.UnitOfWork
                       .Repository<Filter>()
                       .Query
                       .Any(x => x.UserId == userId && x.Name == name && x.Id != idToExclude);
        }

        private static Domain.Filters.Criterion Map(Criterion criterion)
        {
            if (criterion.IsTrue) return Domain.Filters.Criterion.True;

            if (!criterion.IsComplex && !criterion.IsRecurrence)
            {
                return criterion.Value == Domain.Filters.FieldValue.Literal
                           ? new Domain.Filters.SimpleCriterion(criterion.Field, criterion.Operator, criterion.Literal)
                           : new Domain.Filters.SimpleCriterion(criterion.Field, criterion.Operator, criterion.Value);
            }

            if (criterion.IsRecurrence)
            {
                if (criterion.RecurrenceDateEquals != null)
                    return new Domain.Filters.RecurrenceCriterion(criterion.RecurrenceDateEquals);

                return criterion.RecurrenceDateIn != null
                           ? new Domain.Filters.RecurrenceCriterion(criterion.RecurrenceDateIn)
                           : new Domain.Filters.RecurrenceCriterion(criterion.RecurrenceDateOnOrBefore,
                                                                    criterion.RecurrenceDateOnOrAfter);
            }

            return new Domain.Filters.ComplexCriterion(Map(criterion.Criterion1), criterion.Conjunction,
                                                       Map(criterion.Criterion2));
        }

        private static Criterion Map(Domain.Filters.Criterion criterion)
        {
            if (criterion == Domain.Filters.Criterion.True)
                return new Criterion {IsTrue = true};

            var simpleCriterion = criterion as Domain.Filters.SimpleCriterion;
            if (simpleCriterion != null)
            {
                return new Criterion
                    {
                        Field = simpleCriterion.Field,
                        Operator = simpleCriterion.Operator,
                        Value = simpleCriterion.Value,
                        Literal = simpleCriterion.Literal
                    };
            }

            var recurrenceCriterion = criterion as Domain.Filters.RecurrenceCriterion;
            if (recurrenceCriterion != null)
            {
                return new Criterion
                    {
                        IsRecurrence = true,
                        RecurrenceDateEquals = recurrenceCriterion.RecurrenceDateEquals,
                        RecurrenceDateIn = recurrenceCriterion.RecurrenceDateIn,
                        RecurrenceDateOnOrBefore = recurrenceCriterion.RecurrenceDateOnOrBefore,
                        RecurrenceDateOnOrAfter = recurrenceCriterion.RecurrenceDateOnOrAfter
                    };
            }

            var complexCriterion = criterion as Domain.Filters.ComplexCriterion;
            if (complexCriterion == null)
                throw new NotSupportedException("This type of Criterion is not supported.");

            return new Criterion
                {
                    IsComplex = true,
                    Criterion1 = Map(complexCriterion.Criterion1),
                    Conjunction = complexCriterion.Conjunction,
                    Criterion2 = Map(complexCriterion.Criterion2)
                };
        }

        private static Domain.Filters.Filter Map(Filter filter, Domain.Users.User user)
        {
            return new FilterImpl(filter.Id, filter.Name, user, filter.Criterion);
        }

        private static Filter Map(Domain.Filters.Filter filter)
        {
            return new Filter
                {
                    Id = filter.Id,
                    Name = filter.Name,
                    Criterion = Map(filter.Criterion),
                    UserId = filter.User.Id
                };
        }

        private class FilterImpl : Domain.Filters.Filter
        {
            public FilterImpl(int id, string name, Domain.Users.User user, Criterion criterion)
                : base(id, name, user, Map(criterion))
            {
            }
        }
    }
}