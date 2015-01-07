/*******************************************************************************************************************************
 * AK.Chore.Contracts.GeneralResult
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

namespace AK.Chore.Contracts
{
    /// <summary>
    /// Represents general error codes.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public enum GeneralResult
    {
        [EnumDescription("The operation completed successfully.")] 
        Success,

        [EnumDescription("There was an error trying to perform the last operation.")] 
        Error,

        [EnumDescription("You are not authorized to perform this operation. Refer to Key for Id of resource you are trying to work with.")]
        NotAuthorized,

        [EnumDescription("Your request had certain invalid parameters. Refer to Key for Id of resource you are trying to work with.")]
        InvalidRequest
    }
}