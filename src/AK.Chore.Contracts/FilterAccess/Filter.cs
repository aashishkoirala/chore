/*******************************************************************************************************************************
 * AK.Chore.Contracts.FilterAccess.Filter
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

namespace AK.Chore.Contracts.FilterAccess
{
    /// <summary>
    /// Represents a filter.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Filter
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public Criterion Criterion { get; set; }
    }
}