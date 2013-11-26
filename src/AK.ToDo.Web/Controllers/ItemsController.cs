/*******************************************************************************************************************************
 * AK.To|Do.Web.Controllers.ItemsController
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
    /// Web API controller for "/items" - lets the client retrieve, update or delete To-Do items. This treats a bunch of
    /// To-Do items as one resource.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export, PartCreationPolicy(CreationPolicy.NonShared), WebApiAuth, ServiceExceptionFilter]
    public class ItemsController : ApiController, IUserAwareController
    {
        [Import] private IItemService itemService;

        public string UserName { get; set; }
        public Guid UserId { get; set; }

        public IList<ToDoItem> Get([FromUri] ToDoItemListRequest request)
        {
            // This method is repeated here and in ItemsController.GetList for the sake of REST API completeness.
            // Absolutely not DRY, but I just couldn't leave it out of either. If the logic was any more
            // complex, I would extract it to a common method.

            request.AppUserId = this.UserId;
            return this.itemService.GetList(request);
        }

        public HttpResponseMessage Post(IList<ToDoItem> itemList)
        {
            var createdItems = this.itemService.UpdateList(itemList);

            var response = this.Request.CreateResponse(HttpStatusCode.Created, createdItems);

            return response;
        }

        public void Put(IList<ToDoItem> itemList)
        {
            this.itemService.UpdateList(itemList);
        }

        public void Delete(string idList)
        {
            // This piece of string splittery needed because DELETE only works with request parameters.

            var idGuidList = new List<Guid>();
            foreach (var id in idList.Split('|'))
            {
                Guid idGuid;
                if (!Guid.TryParse(id, out idGuid)) throw new HttpResponseException(HttpStatusCode.BadRequest);
                
                idGuidList.Add(idGuid);
            }

            this.itemService.DeleteListByIdList(this.UserId, idGuidList);
        }
    }
}