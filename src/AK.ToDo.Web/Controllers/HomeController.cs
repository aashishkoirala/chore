/*******************************************************************************************************************************
 * AK.To|Do.Web.Controllers.HomeController
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

using AK.Todo.Web.Filters;
using System;
using System.ComponentModel.Composition;
using System.Web.Mvc;
using System.Web.Security;

#endregion

namespace AK.ToDo.Web.Controllers
{
    /// <summary>
    /// Entry point MVC controller. All actions decorated with MvcAuth are
    /// protected and will cause SSO login if not already logged in.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class HomeController : Controller, IUserAwareController
    {
        public string UserName { get; set; }
        public Guid UserId { get; set; }

        /// <summary>
        /// Entry point action. Redirects to "Main" if logged in,
        /// otherwise renders its own view - which is the login page, if not.
        /// </summary>
        /// <returns>ActionResult</returns>
        public ActionResult Index()
        {
            // We have to use this gem here rather than RedirectToAction
            // because that does not add the trailing "slash" and without that,
            // Angular routes goes crazy.
            //
            if (this.User.Identity.IsAuthenticated)
                return this.Redirect(this.Url.Action("Main") + "/");

            return this.View();
        }

        /// <summary>
        /// Causes SSO login and redirects to the Index entry point action.
        /// </summary>
        /// <returns>ActionResult</returns>
        [MvcAuth]
        public ActionResult Login()
        {
            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// Renders the main view that is the application 
        /// the user sees after logging in.
        /// </summary>
        /// <returns></returns>
        [MvcAuth]
        public ActionResult Main()
        {
            return this.View();
        }

        /// <summary>
        /// Logs the user out.
        /// </summary>
        /// <returns>ActionResult</returns>
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return this.RedirectToAction("Index");
        }
    }
}