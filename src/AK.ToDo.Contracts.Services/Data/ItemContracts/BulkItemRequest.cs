/*******************************************************************************************************************************
 * AK.To|Do.Contracts.Services.Data.ItemContracts.BulkItemRequest
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

using System.Collections.Generic;

namespace AK.ToDo.Contracts.Services.Data.ItemContracts
{
    /// <summary>
    /// Contains information for any request on a bulk list of To-Do items.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class BulkItemRequest
    {
        public IList<string> IdList { get; set; }
    }
}