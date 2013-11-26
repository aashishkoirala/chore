/*******************************************************************************************************************************
 * AK.To|Do.Web.ToDoApplication
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
using AK.Commons.Configuration;
using AK.Commons.Web.Composition;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

#endregion

namespace AK.ToDo.Web
{
    /// <summary>
    /// Web application entry point.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class ToDoApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            ConfigureWebApi();
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            ConfigureMvcRoutes();

            this.InitializeEnvironment();

            var bundlesJson = File.ReadAllText(this.Server.MapPath("~/Bundles.json"));
            AppEnvironment.BundleConfigurator.Configure(bundlesJson);

            ControllerBuilder.Current.SetControllerFactory(new ComposableControllerFactory());
            GlobalConfiguration.Configuration.DependencyResolver = new ComposableDependencyResolver();
        }

        private static void ConfigureWebApi()
        {
            var routes = GlobalConfiguration.Configuration.Routes;

            routes.MapHttpRoute("ItemCommandApi", "api/command/item/{action}/{id}",
                new { controller = "itemcommand" });

            routes.MapHttpRoute("ItemsCommandApi", "api/command/items/{action}",
                new { controller = "itemscommand" });

            routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}",
                new { id = RouteParameter.Optional });
        }

        private static void ConfigureMvcRoutes()
        {
            RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            RouteTable.Routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new
                {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                });            
        }

        private void InitializeEnvironment()
        {
            var mappedExeConfiguration = new ExeConfigurationFileMap
            {
                ExeConfigFilename = this.Server.MapPath("~/Web.config")
            };

            var config = ConfigurationManager.OpenMappedExeConfiguration(
                mappedExeConfiguration, ConfigurationUserLevel.None);

            var configStore = config.GetConfigStore();

            AppEnvironment.Initialize("ToDo", new InitializationOptions
            {
                ConfigStore = configStore,
                EnableLogging = true
            });           
        }
    }
}