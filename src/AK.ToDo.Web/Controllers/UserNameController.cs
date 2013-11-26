﻿/*******************************************************************************************************************************
 * AK.To|Do.Web.Controllers.UserNameController
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

using AK.ToDo.Web.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Web.Http;

#endregion

namespace AK.ToDo.Web.Controllers
{
    /// <summary>
    /// Web API controller for "/username" - lets the client get the name of the currently logged in user.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export, PartCreationPolicy(CreationPolicy.NonShared), WebApiAuth, ServiceExceptionFilter]
    public class UserNameController : ApiController, IUserAwareController
    {
        public string UserName { get; set; }
        public Guid UserId { get; set; }

        public IDictionary<string, string> Get()
        {
            // All I wanted to do was return a user name - but it would not translate nicely.
            // So I'm returning this dictionary to make it { UserName: UserName} type JSON.
            // Not sure if it works for XML - probably doesn't.
            //
            // TODO: Use a better data structure.
            //
            return new Dictionary<string, string> {{"UserName", this.UserName}};
        }
   }
}