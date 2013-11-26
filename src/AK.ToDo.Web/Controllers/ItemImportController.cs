/*******************************************************************************************************************************
 * AK.To|Do.Web.Controllers.ItemImportController
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
using System.Net;
using System.Net.Http;
using System.Web.Http;

#endregion

namespace AK.ToDo.Web.Controllers
{
    /// <summary>
    /// Web API controller for "/itemimport" - lets the client import items from provided text data.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export, PartCreationPolicy(CreationPolicy.NonShared), WebApiAuth, ServiceExceptionFilter]
    public class ItemImportController : ApiController, IUserAwareController
    {
        [Import] private IItemImportService itemImportService;

        public string UserName { get; set; }
        public Guid UserId { get; set; }

        public HttpResponseMessage Post(IDictionary<string, string> request)
        {
            // I just need the importData - but I cannot use a simple string. So I have to make it want
            // JSON of the type "{ importData: importData }". The right way would have been to create a
            // data contract class for this, but I got lazy here. This works for JSON, but I have no idea
            // what it does for something else such as XML.
            //
            // TODO: Use a better data structure.
            //
            string importData;
            if (!request.TryGetValue("importData", out importData))
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            var createdItems = this.itemImportService.Import(this.UserId, importData);

            return this.Request.CreateResponse(HttpStatusCode.Created, createdItems);
        }
   }
}