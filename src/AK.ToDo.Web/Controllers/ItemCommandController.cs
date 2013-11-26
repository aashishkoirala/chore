/*******************************************************************************************************************************
 * AK.To|Do.Web.Controllers.ItemCommandController
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
using System.ComponentModel.Composition;
using System.Net;
using System.Web.Http;

#endregion

namespace AK.ToDo.Web.Controllers
{
    /// <summary>
    /// Web API controller for "/command/item" - lets the client perform commands on a single To-Do item.
    /// All commands are PUT requests.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export, PartCreationPolicy(CreationPolicy.NonShared), WebApiAuth, ServiceExceptionFilter]
    public class ItemCommandController : ApiController, IUserAwareController
    {
        [Import] private IItemCommandService itemCommandService;

        public string UserName { get; set; }
        public Guid UserId { get; set; }

        [HttpPut]
        public void Start(string id)
        {
            Guid idGuid;
            if (!Guid.TryParse(id, out idGuid)) throw new HttpResponseException(HttpStatusCode.BadRequest);

            this.itemCommandService.Start(this.UserId, idGuid);
        }

        [HttpPut]
        public void Pause(string id)
        {
            Guid idGuid;
            if (!Guid.TryParse(id, out idGuid)) throw new HttpResponseException(HttpStatusCode.BadRequest);

            this.itemCommandService.Pause(this.UserId, idGuid);
        }

        [HttpPut]
        public void Complete(string id)
        {
            Guid idGuid;
            if (!Guid.TryParse(id, out idGuid)) throw new HttpResponseException(HttpStatusCode.BadRequest);

            this.itemCommandService.Complete(this.UserId, idGuid);
        }
    }
}