/*******************************************************************************************************************************
 * AK.Chore.Contracts.FilterAccess.FilterAccessResult
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

using AK.Commons;

namespace AK.Chore.Contracts.FilterAccess
{
    /// <summary>
    /// Result codes for IFilterAccess operations.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public enum FilterAccessResult
    {
        [EnumDescription("The filter does not exist. Refer to Key for Id of specified filter.")]
        FilterDoesNotExist,

        [EnumDescription("The filter already exists. Refer to Key for Id of specified filter.")]
        FilterAlreadyExists,

        [EnumDescription("The user specified on this filter does not exist. Refer to Key for Id of specified user.")]
        FilterUserDoesNotExist,

        [EnumDescription("You cannot delete the only filter you have.")]
        CannotDeleteOnlyFilter
    }
}