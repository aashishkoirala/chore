/*******************************************************************************************************************************
 * AK.Chore.Presentation.ChoreApplication
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

using AK.Chore.Presentation.Api;
using AK.Commons;
using AK.Commons.Configuration;
using AK.Commons.Logging;
using AK.Commons.Web;
using AK.Commons.Web.LibraryResources;
using AK.Commons.Web.Security;
using Owin;
using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

#endregion

namespace AK.Chore.Presentation
{
    /// <summary>
    /// Main web application.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class ChoreApplication : HttpApplication
    {
        private const string ApplicationName = "Chore";
        private const string IsInitializedKey = ApplicationName + ".IsInitialized";

        private static readonly object initializationLock = new object();

        private IAppLogger logger;
        private IAppConfig config;

        protected void Application_Start(object sender, EventArgs e)
        {
            lock (initializationLock)
            {
                var isInitializedObject = this.Application[IsInitializedKey];
                var isInitialized = isInitializedObject != null && ((bool) isInitializedObject);

                if (isInitialized) return;

                this.Initialize();

                this.Application[IsInitializedKey] = true;
            }
        }

        protected void Application_End(object sender, EventArgs e)
        {
            lock (initializationLock)
            {
                var isInitializedObject = this.Application[IsInitializedKey];
                var isInitialized = isInitializedObject != null && ((bool) isInitializedObject);

                if (!isInitialized) return;

                this.logger.Information("Shutting down...");

                // Give the log message 2 seconds to get to wherever it needs to go.
                //
                Thread.Sleep(2000);

                AppEnvironment.ShutDown();

                this.Application[IsInitializedKey] = false;
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            this.AddToAllowedAudience();
        }

        private void Initialize()
        {
            this.InitializeEnvironment();

            this.logger.Information("Initialized environment, configuring web API...");
            ApiConfig.Configure();

            GlobalFilters.Filters.Add(new HandleErrorAttribute());

            this.logger.Information("Mapping MVC routes...");
            this.MapRoutes();

            this.logger.Information("Configuring secure login...");
            this.ConfigureSecurity();
        }

        private void InitializeEnvironment()
        {
            var configPath = this.Server.MapPath("~/Web.config");
            var configMap = new ExeConfigurationFileMap {ExeConfigFilename = configPath};

            var configContent = ConfigurationManager.OpenMappedExeConfiguration(
                configMap, ConfigurationUserLevel.None);

            AppEnvironment.Initialize(ApplicationName, new InitializationOptions
                {
                    ConfigStore = configContent.GetConfigStore(),
                    EnableLogging = true,
                    GenerateServiceClients = false
                });

            WebEnvironment.Initialize(this.Server);

            this.logger = AppEnvironment.Logger;
            this.config = AppEnvironment.Config;
        }

        private void MapRoutes()
        {
            var mainPageHtmlPath = this.config.Get("MainPageHtmlPath", "~/client/main.html");

            RouteTable.Routes.MapLibraryContent();
            RouteTable.Routes.MapSecureSpaRoutes(mainPageHtmlPath, () =>
                {
                    var cookie = new HttpCookie(UserUtility.NicknameCookie, string.Empty)
                        {
                            Expires = DateTime.Now.AddHours(-1)
                        };
                    HttpContext.Current.Response.Cookies.Add(cookie);
                });
        }

        private void ConfigureSecurity()
        {
            var allowedAudienceUriList = this.config.Get<string>("AllowedAudienceUriList");

            this.logger.Information(string.Format("Allowed audience URI list = {0}", allowedAudienceUriList));

            this.ConfigureSecureLogin(new SecureLoginInfo
                {
                    Realm = string.Format("urn:{0}", ApplicationName),
                    AllowedAudienceUriList = allowedAudienceUriList,
                    FurtherAction = x =>
                        {
                            x.IdentityConfiguration.CertificateValidationMode = X509CertificateValidationMode.None;
                            x.IdentityConfiguration.RevocationMode = X509RevocationMode.NoCheck;
                        }
                });
        }

        public void Configuration(IAppBuilder app)
        {
            if (this.logger != null) this.logger.Information("Mapping SignalR routes...");
            app.MapSignalR();
        }
    }
}