/*******************************************************************************************************************************
 * AK.To|Do.Web.Controllers.ItemController
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
using System.Net.Http;
using System.Web.Http;

#endregion

namespace AK.ToDo.Web.Controllers
{
    /// <summary>
    /// Web API controller for "/item" - lets the client retrieve, update or delete To-Do items. This treats a single
    /// To-Do item as one resource.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export, PartCreationPolicy(CreationPolicy.NonShared), WebApiAuth, ServiceExceptionFilter]
    public class ItemController : ApiController, IUserAwareController
    {
        [Import] private IItemService itemService;

        public string UserName { get; set; }
        public Guid UserId { get; set; }

        public ToDoItem Get(string id)
        {
            Guid idGuid;
            if (!Guid.TryParse(id, out idGuid)) throw new HttpResponseException(HttpStatusCode.BadRequest);

            var item = this.itemService.Get(this.UserId, idGuid);

            if (item == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            return item;
        }

        public IList<ToDoItem> GetList([FromUri] ToDoItemListRequest request)
        {
            // This method is repeated here and in ItemsController for the sake of REST API completeness.
            // Absolutely not DRY, but I just couldn't leave it out of either. If the logic was any more
            // complex, I would extract it to a common method.

            request.AppUserId = this.UserId;
            return this.itemService.GetList(request);
        }

        public HttpResponseMessage Post(ToDoItem item)
        {
            item.AppUserId = this.UserId;
            var createdItem = this.itemService.Update(item);

            var response = this.Request.CreateResponse(HttpStatusCode.Created, createdItem);
            response.Headers.Location = new Uri(this.Url.Link("DefaultApi", new {id = createdItem.Id}));

            return response;
        }

        public void Put(ToDoItem item)
        {
            item.AppUserId = this.UserId;
            this.itemService.Update(item);
        }

        public void Delete(string id)
        {
            Guid idGuid;
            if (!Guid.TryParse(id, out idGuid)) throw new HttpResponseException(HttpStatusCode.BadRequest);

            this.itemService.DeleteById(this.UserId, idGuid);
        }
    }
}