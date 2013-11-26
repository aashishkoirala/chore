/*******************************************************************************************************************************
 * AK.To|Do.Web.Controllers.IUserAwareController
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

using System;

namespace AK.ToDo.Web.Controllers
{
    /// <summary>
    /// Represents a controller that is capable of storing information on the currently
    /// logged in user. These values are assigned for the first time when the user logs in,
    /// then stored in cookies and assigned on each action.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IUserAwareController
    {
        /// <summary>
        /// User name of the logged in user.
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// User Id of the logged in user.
        /// </summary>
        Guid UserId { get; set; }
    }
}