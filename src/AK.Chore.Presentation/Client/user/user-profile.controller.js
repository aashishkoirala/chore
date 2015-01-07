/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.user.user-profile.controller.js
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

/* userProfileController - Angular controller for the "User Profile" section.
 * @author Aashish Koirala
 */
define(['app'], function (app) {

    userProfileController.$inject = ['$scope', '$rootScope', '$timeout', '$window', 'userService', 'appState'];
    app.register.controller('userProfileController', userProfileController);

    function userProfileController($scope, $rootScope, $timeout, $window, userService, appState) {

        $scope.nickname = '';
        $scope.userUploadData = '';
        $scope.userDownloadData = '';

        $scope.isWorking = function() {
            return appState.isWorking;
        };

        $scope.updateNickname = function() {

            $scope.nickname = $scope.nickname.trim();
            if ($scope.nickname == '') return;

            userService.updateUserNickname($scope.nickname);
        };

        $scope.downloadData = function() {

            userService
                .downloadUserData()
                .then(function(userData) {
                    $scope.userDownloadData = JSON.stringify(userData);
                });
        };

        $scope.uploadData = function() {

            $scope.userUploadData = $scope.userUploadData.trim();
            if ($scope.userUploadData == '') return;

            var userData = JSON.parse($scope.userUploadData);
            
            userService
                .uploadUserData(userData)
                .then(function () {
                    $rootScope.$broadcast('reloadPage');
                    $timeout(function() {
                        $window.location.href = '';
                    }, 2000);
                });
        };

        $scope.deleteAccount = function() {

            appState.isError = false;
            appState.isWorking = true;

            userService.deleteUserProfile()
                .then(function() {
                    $window.location.href = 'Logout';
                }, function() {
                    appState.isWorking = false;
                    appState.isError = true;
                });
        };

        initialize();

        function initialize() {

            appState.currentView = 'User';
            appState.isError = false;

            userService
                .loadUserNickname()
                .then(function() {
                    $scope.nickname = userService.userNickname();
                });
        }
    }
});