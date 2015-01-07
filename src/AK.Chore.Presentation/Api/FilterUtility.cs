/*******************************************************************************************************************************
 * AK.Chore.Presentation.Api.FilterUtility
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

using AK.Chore.Contracts.FilterAccess;
using AK.Commons.Web;
using System.Web;
using System.Web.Http.Controllers;

#endregion

namespace AK.Chore.Presentation.Api
{
    /// <summary>
    /// Gets/sets unsaved filter from/to cookie.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class FilterUtility
    {
        private static readonly Filter defaultFilter = new Filter
            {
                Name = "Unsaved Filter",
                UserId = 1,
                Criterion = new Criterion {Type = CriterionType.True}
            };

        public static Filter GetFilter(this HttpControllerContext context)
        {
            var filterCookie = HttpContext.Current.Request.Cookies["X-UnsavedFilter"];
            if (filterCookie == null) return defaultFilter;

            var filterJson = filterCookie.Value;
            return string.IsNullOrWhiteSpace(filterJson) ? defaultFilter : JsonUtility.Deserialize<Filter>(filterJson);
        }

        public static void SetFilter(this HttpControllerContext context, Filter filter)
        {
            var filterJson = JsonUtility.Serialize(filter);
            HttpContext.Current.Response.SetCookie(new HttpCookie("X-UnsavedFilter", filterJson));
        }
    }
}