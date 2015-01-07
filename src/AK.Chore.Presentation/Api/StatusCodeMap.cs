/*******************************************************************************************************************************
 * AK.Chore.Presentation.Api.StatusCodeMap
 * Copyright © 2014-2015 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of CHORE.
 *  
 * CHORE is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * CHORE is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with CHORE.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

#region Namespace Imports

using AK.Chore.Contracts;
using AK.Chore.Contracts.FilterAccess;
using AK.Chore.Contracts.FolderAccess;
using AK.Chore.Contracts.TaskAccess;
using AK.Chore.Contracts.TaskImportExport;
using AK.Chore.Contracts.UserProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

#endregion

namespace AK.Chore.Presentation.Api
{
    /// <summary>
    /// Maps operation result codes to HTTP status codes.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class StatusCodeMap
    {
        private static readonly IDictionary<Enum, HttpStatusCode> statusCodeMap;
        private static readonly Type[] codeTypes;

        static StatusCodeMap()
        {
            statusCodeMap = new Dictionary<Enum, HttpStatusCode>
                {
                    {GeneralResult.Success, HttpStatusCode.OK},
                    {GeneralResult.Error, HttpStatusCode.InternalServerError},
                    {GeneralResult.InvalidRequest, HttpStatusCode.BadRequest},
                    {GeneralResult.NotAuthorized, HttpStatusCode.Unauthorized},
                    {TaskAccessResult.TaskDoesNotExist, HttpStatusCode.NotFound},
                    {TaskAccessResult.TaskAlreadyExists, HttpStatusCode.BadRequest},
                    {TaskAccessResult.TaskCouldNotBeMapped, HttpStatusCode.BadRequest},
                    {FilterAccessResult.FilterDoesNotExist, HttpStatusCode.NotFound},
                    {FilterAccessResult.FilterAlreadyExists, HttpStatusCode.BadRequest},
                    {FilterAccessResult.FilterUserDoesNotExist, HttpStatusCode.BadRequest},
                    {FilterAccessResult.CannotDeleteOnlyFilter, HttpStatusCode.BadRequest},
                    {FolderAccessResult.FolderDoesNotExist, HttpStatusCode.NotFound},
                    {FolderAccessResult.FolderAlreadyExists, HttpStatusCode.BadRequest},
                    {FolderAccessResult.FolderUserOrParentDoesNotExist, HttpStatusCode.BadRequest},
                    {FolderAccessResult.CannotMoveToChildFolder, HttpStatusCode.BadRequest},
                    {FolderAccessResult.CannotDeleteOnlyRootFolder, HttpStatusCode.BadRequest},
                    {TaskImportExportResult.NoTasksToExport, HttpStatusCode.BadRequest},
                    {TaskImportExportResult.NoTasksToImport, HttpStatusCode.BadRequest},
                    {TaskImportExportResult.CannotImportTask, HttpStatusCode.BadRequest},
                    {UserProfileResult.UserNameNotSpecified, HttpStatusCode.BadRequest},
                    {UserProfileResult.UserDoesNotExist, HttpStatusCode.NotFound},
                    {UserProfileResult.UserAlreadyExists, HttpStatusCode.BadRequest}
                };

            codeTypes = statusCodeMap.Keys.Select(x => x.GetType()).Distinct().ToArray();
        }

        public static HttpStatusCode GetStatusCode(string code)
        {
            foreach (var type in codeTypes)
            {
                try
                {
                    var codeEnum = (Enum) Enum.Parse(type, code);
                    HttpStatusCode statusCode;
                    if (!statusCodeMap.TryGetValue(codeEnum, out statusCode))
                        statusCode = HttpStatusCode.InternalServerError;
                    return statusCode;
                }
                catch (ArgumentException)
                {
                }
            }
            return HttpStatusCode.InternalServerError;
        }
    }
}