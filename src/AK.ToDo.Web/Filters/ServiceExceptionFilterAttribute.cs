/*******************************************************************************************************************************
 * AK.To|Do.Web.Filters.ServiceExceptionFilterAttribute
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

using AK.ToDo.Contracts.Services;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

#endregion

namespace AK.ToDo.Web.Filters
{
    /// <summary>
    /// Handles exceptions on web API controllers. Passes deliberate ServiceExceptions through and wraps other
    /// errors in a ServiceException. Maps the ServiceErrorCodes to proper HTTP status codes.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class ServiceExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private static readonly IDictionary<ServiceErrorCodes, HttpStatusCode>
            ErrorToStatusMap = new Dictionary<ServiceErrorCodes, HttpStatusCode>
            {
                {ServiceErrorCodes.Unauthorized, HttpStatusCode.Unauthorized},
                {ServiceErrorCodes.CategoryParentIdSameAsId, HttpStatusCode.BadRequest},
                {ServiceErrorCodes.GeneralServiceError, HttpStatusCode.InternalServerError}
            };

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var serviceException =
                actionExecutedContext.Exception as ServiceException ??
                new ServiceException(ServiceErrorCodes.GeneralServiceError, actionExecutedContext.Exception);

            var statusCode = ErrorToStatusMap[serviceException.Reason];

            actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(
                statusCode, serviceException.Message);
        }
    }
}