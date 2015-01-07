/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.tasks.task-import.controller.js
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

/* taskImportController - Angular controller for the "Import Tasks" section.
 * @author Aashish Koirala
 */
define(['app'], function (app) {

    taskImportController.$inject = ['$scope', '$rootScope', '$location', 'taskService', 'filterService', 'appState'];
    app.register.controller('taskImportController', taskImportController);

    function taskImportController($scope, $rootScope, $location, taskService, filterService, appState) {

        $scope.importData = '';

        $scope.import = function() {

            if ($scope.importData == null || $scope.importData.trim() == '') return;

            var filterId = filterService.filterId();
            var useUnsavedFilter = filterService.useUnsavedFilter();

            taskService
                .importTasks(filterId, useUnsavedFilter, $scope.importData)
                .then(function(results) {

                    $rootScope.$broadcast('notifyReload');

                    var result = handleImportResults(results);

                    $scope.importData = result.message;
                    appState.isError = result.isError;

                }, function() {
                    $scope.importData = '';
                });
        };

        $scope.cancel = function() {
            $location.path('/tasks');
        };

        $scope.isWorking = function() {
            return appState.isWorking;
        };
        
        initialize();

        function initialize() {

            appState.currentView = 'Tasks';
            appState.isError = false;
        }

        function handleImportResults(results) {

            var invalids = results
                .filter(function(item) {
                    return !item.isSuccess;
                });

            if (invalids.length == 0)
                return {
                    isError: false,
                    message: results.length + ' tasks imported successfully!'
                };

            var invalidCount = results.length - invalids.length;
            var errorHeader = 'Only ' + invalidCount + ' task(s) were imported. The following lines could not be imported:';

            var message = invalids
                .map(function(item) {
                    return item.key;
                })
                .reduce(function(item1, item2) {
                    return item1 + '\r\n' + item2;
                }, errorHeader);

            return { isError: true, message: message };
        }
    }
});