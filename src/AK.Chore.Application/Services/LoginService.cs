/*******************************************************************************************************************************
 * AK.Chore.Application.Services.LoginService
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

using AK.Chore.Contracts.UserProfile;
using AK.Commons;
using AK.Commons.Security;
using System;
using System.ServiceModel;

#endregion

namespace AK.Chore.Application.Services
{
    /// <summary>
    /// Service implementation - ILoginService; this is a WCF service.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class LoginService : ILoginService
    {
        private const string ApplicationName = "Chore";
        private const string BannerImageUrlFormat = "{0}/client/screenshot.png";

        private const string TitleCssFormat =
            "@font-face {{ font-family: 'Oswald'; font-style: normal; font-weight: 400; " +
            "src: url('{0}/client/oswald.eot'); src: local('Oswald'), local('Oswald-Regular'), " +
            "url('{0}/client/oswald.eot') format('embedded-opentype'), " +
            "url('{0}/client/oswald.woff') format('woff'), " +
            "url('{0}/client/oswald.woff2') format('woff2'); unicode-range: U+0000-00FF, U+0131, " +
            "U+0152-0153, U+02C6, U+02DA, U+02DC, U+2000-206F, U+2074, U+20AC, U+2212, U+2215, U+E0FF, " +
            "U+EFFD, U+F000; }} .chore-app-title {{ font-family: 'Oswald', sans-serif; font-size: 50px; " +
            "font-weight: 800; }}";

        private const string TitleHtmlFormat = "<span class=\"chore-app-title\">{0}</span>";

        private const string DescriptionCss =
            ".chore-description-list li { font-family: arial, sans-serif; font-size: 12px; }";

        private const string DescriptionHtml =
            "<ul class=\"chore-description-list\">" +
            "<li>Create and track tasks based on deadlines or recurrence.</li>" +
            "<li>Organize tasks into folders.</li>" +
            "<li>Use built-in or custom filters to look for tasks.</li>" +
            "<li>Get a weekly calendar view based on your tasks.</li>" +
            "<li>Export and import tasks to/from flat data.</li></ul>";

        private IUserProfileService userProfileService;
        private LoginSplashInfo loginSplashInfo;

        public IUserProfileService UserProfileServiceOverride { get; set; }

        public Uri RequestUriOverride { get; set; }

        private IUserProfileService UserProfileService
        {
            get
            {
                return this.UserProfileServiceOverride ?? this.userProfileService ??
                       (this.userProfileService = AppEnvironment.Composer.Resolve<IUserProfileService>());
            }
        }

        public LoginSplashInfo GetLoginSplashInfo()
        {
            return this.loginSplashInfo ??
                   (this.loginSplashInfo = GetLoginSplashInfo(OperationContext.Current, this.RequestUriOverride));
        }

        public LoginUserInfo GetUser(string userName)
        {
            var result = this.UserProfileService.GetUserByUserName(userName);
            if (result.IsSuccess)
            {
                var user = result.Result;
                return new LoginUserInfo
                    {
                        UserExists = true,
                        UserId = user.Id.ToString(),
                        UserName = userName,
                        DisplayName = user.Nickname
                    };
            }

            if (result.ErrorCode == UserProfileResult.UserDoesNotExist.ToString())
                return new LoginUserInfo {UserExists = false};

            throw new FaultException(new FaultReason(result.Message));
        }

        public LoginUserInfo CreateUser(LoginUserInfo userInfo)
        {
            var result = this.UserProfileService.CreateUser(userInfo.UserName, userInfo.DisplayName);
            if (!result.IsSuccess) throw new FaultException(new FaultReason(result.Message));

            userInfo.UserExists = true;
            userInfo.UserId = result.Result.Id.ToString();

            return userInfo;
        }

        private static LoginSplashInfo GetLoginSplashInfo(OperationContext operationContext, Uri requestUriOverride)
        {
            var requestUri = requestUriOverride ?? operationContext.RequestContext.RequestMessage.Headers.To;
            var baseUriString = requestUri.GetSchemeAndHost().ToString().TrimEnd('/');

            return new LoginSplashInfo
                {
                    ApplicationName = ApplicationName,
                    BannerImageUrl = string.Format(BannerImageUrlFormat, baseUriString),
                    TitleCss = string.Format(TitleCssFormat, baseUriString),
                    TitleHtml = string.Format(TitleHtmlFormat, ApplicationName.ToUpper()),
                    DescriptionCss = DescriptionCss,
                    DescriptionHtml = DescriptionHtml
                };
        }
    }
}