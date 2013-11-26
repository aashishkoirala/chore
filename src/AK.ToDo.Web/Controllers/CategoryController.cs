/*******************************************************************************************************************************
 * AK.To|Do.Web.Controllers.CategoryController
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

using AK.ToDo.Contracts.Services.Data.CategoryContracts;
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
    /// Web API controller for "/category" - lets the client retrieve, update or delete To-Do item categories.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export, PartCreationPolicy(CreationPolicy.NonShared), WebApiAuth, ServiceExceptionFilter]
    public class CategoryController : ApiController, IUserAwareController
    {
        [Import] private ICategoryService categoryService;

        public string UserName { get; set; }
        public Guid UserId { get; set; }

        public ToDoCategory Get(string id)
        {
            Guid idGuid;
            if (!Guid.TryParse(id, out idGuid)) throw new HttpResponseException(HttpStatusCode.BadRequest);

            var category = this.categoryService.Get(this.UserId, idGuid);

            if (category == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            return category;
        }

        public IList<ToDoCategory> GetList()
        {
            return this.categoryService.GetList(this.UserId);
        }

        public HttpResponseMessage Post(ToDoCategory category)
        {
            category.AppUserId = this.UserId;

            var createdCategory = this.categoryService.Update(category);

            var response = this.Request.CreateResponse(HttpStatusCode.Created, createdCategory);
            response.Headers.Location = new Uri(this.Url.Link("DefaultApi", new { id = createdCategory.Id }));

            return response;
        }

        public void Put(ToDoCategory category)
        {
            category.AppUserId = this.UserId;

            this.categoryService.Update(category);
        }

        public void Delete(string id)
        {
            Guid idGuid;
            if (!Guid.TryParse(id, out idGuid)) throw new HttpResponseException(HttpStatusCode.BadRequest);

            this.categoryService.DeleteById(this.UserId, idGuid);
        }
    }
}