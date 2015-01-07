/*******************************************************************************************************************************
 * AK.Chore.Presentation.Api.UserUtility
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

using AK.Commons.Security;
using Microsoft.AspNet.SignalR.Hubs;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Http.Controllers;

#endregion

namespace AK.Chore.Presentation.Api
{
    /// <summary>
    /// Gets/sets user ID/nickname from/to cookie/encrypted federation cookie.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class UserUtility
    {
        public const string NicknameCookie = "X-Nickname";

        public static int GetUserId(this HttpControllerContext context)
        {
            var userId = HttpContext.Current.User.GetUserId<int>();
            if (!userId.IsThere) throw new HttpException((int) HttpStatusCode.Unauthorized, "Unauthorized");

            return userId;
        }

        public static int GetUserId(this HubCallerContext context)
        {
            var userId = context.User.GetUserId<int>();
            if (!userId.IsThere) throw new HttpException((int) HttpStatusCode.Unauthorized, "Unauthorized");

            return userId;
        }

        public static string GetNickname(this HttpControllerContext context)
        {
            var nicknameCookie = HttpContext.Current.Request.Cookies[NicknameCookie];
            if (nicknameCookie != null) return nicknameCookie.Value;

            var nickname = GetNickname(HttpContext.Current.User);
            if (string.IsNullOrWhiteSpace(nickname)) return string.Empty;

            SetNickname(context, nickname);

            return nickname;
        }

        public static void SetNickname(this HttpControllerContext context, string nickname)
        {
            HttpContext.Current.Response.SetCookie(new HttpCookie(NicknameCookie, nickname));
        }

        private static string GetNickname(IPrincipal principal)
        {
            if (!principal.Identity.IsAuthenticated) return null;

            var claimsPrincipal = principal as ClaimsPrincipal;
            if (claimsPrincipal == null) return null;

            var claimsIdentity = claimsPrincipal.Identities.Single();
            var nameClaim = claimsIdentity.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Name);
            return nameClaim == null ? null : nameClaim.Value;
        }
    }
}