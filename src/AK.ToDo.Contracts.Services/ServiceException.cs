/*******************************************************************************************************************************
 * AK.To|Do.Contracts.Services.ServiceException
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

using AK.Commons.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

#endregion

namespace AK.ToDo.Contracts.Services
{
    /// <summary>
    /// Represents service exceptions.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class ServiceException : ReasonedException<ServiceErrorCodes>
    {
        private static readonly IDictionary<ServiceErrorCodes, string> ReasonByReasonCodeDictionary =
            new Dictionary<ServiceErrorCodes, string>
            {
                {ServiceErrorCodes.Unauthorized, "The user does not have rights to perform this operation."},
                {ServiceErrorCodes.CategoryParentIdSameAsId, "A category cannot be nested within itself."},
                {ServiceErrorCodes.GeneralServiceError, "Sorry, but something went wrong with the last operation."}
            };

        public ServiceException(ServiceErrorCodes reason) : base(reason) {}
        public ServiceException(ServiceErrorCodes reason, string message) : base(reason, message) {}
        public ServiceException(ServiceErrorCodes reason, Exception innerException) : base(reason, innerException) {}
        public ServiceException(ServiceErrorCodes reason, string message, Exception innerException) : base(reason, message, innerException) {}
        public ServiceException(SerializationInfo info, StreamingContext context) : base(info, context) {}

        protected override string GetReasonDescription(ServiceErrorCodes reason)
        {
            string description;
            return ReasonByReasonCodeDictionary.TryGetValue(reason, out description) ? description : string.Empty;
        }
    }
}