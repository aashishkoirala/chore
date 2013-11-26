/*******************************************************************************************************************************
 * AK.To|Do.Web.Filters.MvcAuthAttribute
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

using AK.Commons;
using AK.Commons.Web.Security;
using AK.ToDo.Contracts.Services.Operation;
using AK.ToDo.Web.Controllers;
using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

#endregion

namespace AK.Todo.Web.Filters
{
    /// <summary>
    /// Handles Google SSO redirection and authentication for MVC controllers. Once logged in,
    /// writes out the auth-token and user names into cookies.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class MvcAuthAttribute : WebAuthenticationFilterAttributeBase
    {
        private const string UserNameCookieName = "TD_UserName";

        protected override void OnAlreadyLoggedIn(AuthorizationContext filterContext)
        {
            var userNameCookie = filterContext.HttpContext.Request.Cookies[UserNameCookieName];
            if (userNameCookie == null)
            {
                FormsAuthentication.SignOut();
                filterContext.Result = new RedirectToRouteResult("Default",
                    new RouteValueDictionary(new { controller = "Home", action = "Index" }));
                return;
            }

            var ticket = FormsAuthentication.Decrypt(userNameCookie.Value);
            var userData = ticket.UserData;

            var controller = filterContext.Controller as IUserAwareController;
            if (controller == null) return;

            var parts = userData.Split('|');
            controller.UserId = Guid.Parse(parts[0]);
            controller.UserName = parts[1];
        }

        protected override void OnSuccess(AuthorizationContext filterContext, WebAuthenticationResult authenticationResult)
        {
            var authorizationService = AppEnvironment.Composer.Resolve<IAuthorizationService>();

            var userName = authenticationResult.UserName;
            var userId = authorizationService.CreateOrLoadUserIdByName(userName);
            var userData = string.Format("{0}|{1}", userId, userName);

            var ticket = new FormsAuthenticationTicket(1, UserNameCookieName, DateTime.Now, 
                DateTime.Now.AddDays(1), false, userData);
            var cookieData = FormsAuthentication.Encrypt(ticket);

            filterContext.HttpContext.Response.Cookies.Add(new HttpCookie(UserNameCookieName, cookieData));

            filterContext.Result = new RedirectToRouteResult("Default",
                new RouteValueDictionary(new { controller = "Home", action = "Index" }));
        }

        protected override void OnDenied(AuthorizationContext filterContext, WebAuthenticationResult authenticationResult)
        {
            filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized, authenticationResult.ErrorMessage);
        }

        protected override void OnError(AuthorizationContext filterContext, WebAuthenticationResult authenticationResult)
        {
            filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized, authenticationResult.ErrorMessage);
        }
    }
}