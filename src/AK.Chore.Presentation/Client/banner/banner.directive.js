/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.banner.banner.directive.js
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

'use strict';

/* bannerDirective - The "chore-banner" directive for the banner section.
 * @author Aashish Koirala
 */
define(['app'], function(app) {

    choreBannerDirective.$inject = ['$location', 'appState', 'userService'];
    app.directive('choreBanner', choreBannerDirective);

    function choreBannerDirective($location, appState, userService) {
        return {
            restrict: 'E',
            templateUrl: 'Client/banner/banner.html',
            link: function(scope) {

                scope.isError = function(value) {
                    if (value != undefined) appState.isError = value;
                    return appState.isError;
                };

                scope.errorMessage = function() {
                    return appState.errorMessage == null ?
                        'We had a problem doing that. Sorry for the inconvenience, please try again!' :
                        appState.errorMessage;
                };
                
                scope.isWorking = function() {
                    return appState.isWorking;
                };

                scope.userNickname = function() {
                    return userService.userNickname();
                };

                scope.currentView = function() {
                    return appState.currentView;
                };

                userService.loadUserNickname();
            }
        };
    }
});