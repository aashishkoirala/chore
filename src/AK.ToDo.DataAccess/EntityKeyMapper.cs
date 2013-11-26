/*******************************************************************************************************************************
 * AK.To|Do.DataAccess.EntityKeyMapper
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

using AK.Commons.DataAccess;
using AK.ToDo.Contracts.DataAccess.Entities;
using System;

#endregion

namespace AK.ToDo.DataAccess
{
    /// <summary>
    /// Mapping of entities to entity key properties.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class EntityKeyMapper : IEntityKeyMapper
    {
        public void Map(IEntityKeyMap map)
        {
            map.Map<AppUser, Guid>(x => x.Id);
            map.Map<ToDoCategory, Guid>(x => x.Id);
            map.Map<ToDoItem, Guid>(x => x.Id);
            map.Map<ItemCategory, Guid>(x => x.Id);
        }
    }
}