/*******************************************************************************************************************************
 * AK.Chore.Domain.Filters.IFilterRepository
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
using System.Collections.Generic;

#endregion

namespace AK.Chore.Domain.Filters
{
    /// <summary>
    /// Persistence interface for Filter domain objects.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IFilterRepository : IDomainRepository
    {
        IReadOnlyCollection<Filter> ListForUser(User user);
        Filter Get(int id, IUserRepository userRepository);
        void Save(Filter filter);
        void Delete(Filter filter);
        int GetCountForUser(int userId);
        bool ExistsForUser(int userId, string name, int idToExclude);
    }
}