/*******************************************************************************************************************************
 * AK.Chore.Domain.Filters.Filter
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

#endregion

namespace AK.Chore.Domain.Filters
{
    /// <summary>
    /// Represents the domain of a task filter.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Filter : IAggregateRoot<Filter, int>
    {
        private string name;
        private Criterion criterion;

        public int Id { get; protected set; }
        public User User { get; protected set; }

        public string Name
        {
            get { return this.name; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new DomainValidationException(DomainValidationErrorCode.FilterNameEmpty);

                this.name = value;
            }
        }

        public Criterion Criterion
        {
            get { return this.criterion; }
            set
            {
                if (value == null)
                    throw new DomainValidationException(DomainValidationErrorCode.FilterCriterionNotSet);

                this.criterion = value;
            }
        }

        public Filter(IEntityIdGenerator<int> idGenerator, string name, User user, Criterion criterion)
            : this(GenerateId(idGenerator), name, user, criterion)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainValidationException(DomainValidationErrorCode.FilterNameEmpty);

            if (user == null)
                throw new DomainValidationException(DomainValidationErrorCode.FilterUserNotSet);

            if (criterion == null)
                throw new DomainValidationException(DomainValidationErrorCode.FilterCriterionNotSet);
        }

        protected Filter(int id, string name, User user, Criterion criterion)
        {
            this.Id = id;
            this.Name = name;
            this.User = user;
            this.Criterion = criterion;

            if (user != null) user.AddFilter(this);
        }

        public bool Equals(Filter other)
        {
            return this.Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is Filter && this.Equals(obj as Filter);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        private static int GenerateId(IEntityIdGenerator<int> idGenerator)
        {
            if (idGenerator == null)
                throw new DomainValidationException(DomainValidationErrorCode.FilterIdGeneratorNotSet);

            return idGenerator.Next<Filter>();
        }
    }
}