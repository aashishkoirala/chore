/*******************************************************************************************************************************
 * AK.To|Do.Web.Controllers.ItemListTypeController
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

using AK.ToDo.Contracts.Services.Operation;
using AK.ToDo.Web.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web.Http;

#endregion

namespace AK.ToDo.Web.Controllers
{
    /// <summary>
    /// Web API controller for "/itemlisttype" - lets the client get the list of possible item list types.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export, PartCreationPolicy(CreationPolicy.NonShared), WebApiAuth, ServiceExceptionFilter]
    public class ItemListTypeController : ApiController, IUserAwareController
    {
        [Import] private ILookupService lookupService;

        public string UserName { get; set; }
        public Guid UserId { get; set; }

        public IList<Tuple<string, string>> Get()
        {
            // Again, I got too lazy to create a proper data contract class for what this returns.
            // The serialization here works for JSON (although has ugly member names), but I have 
            // no idea what it does for something else such as XML.
            //
            // TODO: Use a better data structure.
            //
            return this.lookupService.GetItemListTypeList()
                .Select(x => new Tuple<string, string>(x.Key.ToString(), x.Value))
                .ToList();
        }
   }
}