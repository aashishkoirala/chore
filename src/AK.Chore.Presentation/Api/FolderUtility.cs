/*******************************************************************************************************************************
 * AK.Chore.Presentation.Api.FolderUtility
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

using AK.Commons.Web;
using System.Collections.Generic;
using System.Web;
using System.Web.Http.Controllers;

#endregion

namespace AK.Chore.Presentation.Api
{
    /// <summary>
    /// Gets/sets selected folder ID list from/to cookie.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class FolderUtility
    {
        public static IReadOnlyCollection<int> GetFolderIds(this HttpControllerContext context)
        {
            var folderIdsCookie = HttpContext.Current.Request.Cookies["X-FolderIds"];
            if (folderIdsCookie == null) return new int[0];

            var folderIdsJson = folderIdsCookie.Value;
            return string.IsNullOrWhiteSpace(folderIdsJson) ? new int[0] : JsonUtility.Deserialize<int[]>(folderIdsJson);
        }

        public static void SetFolderIds(this HttpControllerContext context, IReadOnlyCollection<int> folderIds)
        {
            var folderIdsJson = JsonUtility.Serialize(folderIds);

            HttpContext.Current.Response.SetCookie(new HttpCookie("X-FolderIds", folderIdsJson));
        }
    }
}