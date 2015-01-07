/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.banner.notification.controller.js
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

/* notificationController - Globally scoped Angular controller that manages SignalR push notifications.
 * @author Aashish Koirala
 */
define(['app', 'jquery', 'signalRHubs'], function (app, $) {

    notificationController.$inject = ['$scope', '$rootScope', '$window', 'initializationService'];
    app.controller('notificationController', notificationController);

    function notificationController($scope, $rootScope, $window, initializationService) {

        $scope.showReloadMessage = false;

        var notificationHub = $.connection.notificationHub;

        notificationHub.client.notifyReload = function() {
            $scope.$apply('showReloadMessage = true');
        };

        $.connection.hub.start().done(function() {
            notificationHub.server.joinGroup();

            $scope.reload = function() {
                $scope.showReloadMessage = false;
                initializationService
                    .reinitialize(false)
                    .then(function() {
                        $rootScope.$broadcast('reloadTasks');
                        $rootScope.$broadcast('reloadCalendar');
                    });
            };

            $scope.$on('notifyReload', function() {
                notificationHub.server.notifyReload();
            });

            $scope.$on('reloadPage', function() {
                $window.location.href = '';
            });
        });
    }
});