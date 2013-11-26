/*******************************************************************************************************************************
 * AK.To|Do.Web.Controllers.ItemsCommandController
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

using AK.ToDo.Contracts.Services.Data.ItemContracts;
using AK.ToDo.Contracts.Services.Operation;
using AK.ToDo.Web.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Web.Http;

#endregion

namespace AK.ToDo.Web.Controllers
{
    /// <summary>
    /// Web API controller for "/command/items" - lets the client perform commands on a bunch of To-Do items.
    /// All commands are PUT requests.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export, PartCreationPolicy(CreationPolicy.NonShared), WebApiAuth, ServiceExceptionFilter]
    public class ItemsCommandController : ApiController, IUserAwareController
    {
        [Import] private IItemCommandService itemCommandService;

        public string UserName { get; set; }
        public Guid UserId { get; set; }

        [HttpPut]
        public void Start(BulkItemRequest request)
        {
            var idGuidList = new List<Guid>();
            foreach (var id in request.IdList)
            {
                Guid idGuid;
                if (!Guid.TryParse(id, out idGuid)) throw new HttpResponseException(HttpStatusCode.BadRequest);

                idGuidList.Add(idGuid);
            }

            this.itemCommandService.StartList(this.UserId, idGuidList);
        }

        [HttpPut]
        public void Pause(BulkItemRequest request)
        {
            var idGuidList = new List<Guid>();
            foreach (var id in request.IdList)
            {
                Guid idGuid;
                if (!Guid.TryParse(id, out idGuid)) throw new HttpResponseException(HttpStatusCode.BadRequest);

                idGuidList.Add(idGuid);
            }

            this.itemCommandService.PauseList(this.UserId, idGuidList);
        }

        [HttpPut]
        public void Complete(BulkItemRequest request)
        {
            var idGuidList = new List<Guid>();
            foreach (var id in request.IdList)
            {
                Guid idGuid;
                if (!Guid.TryParse(id, out idGuid)) throw new HttpResponseException(HttpStatusCode.BadRequest);

                idGuidList.Add(idGuid);
            }

            this.itemCommandService.CompleteList(this.UserId, idGuidList);
        }
    }
}