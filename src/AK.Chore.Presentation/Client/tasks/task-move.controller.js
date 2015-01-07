/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.tasks.task-move.controller.js
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

/* taskMoveController - Angular controller for the "Move Tasks" section.
 * @author Aashish Koirala
 */
define(['app'], function (app) {

    taskMoveController.$inject = ['$scope', '$rootScope', '$routeParams', '$location',
        'initializationService', 'folderService', 'filterService', 'taskService', 'appState'];

    app.register.controller('taskMoveController', taskMoveController);

    function taskMoveController($scope, $rootScope, $routeParams, $location, initializationService, folderService, filterService, taskService, appState) {

        $scope.folders = [];
        $scope.folderId = null;
        $scope.id = null;
        $scope.ids = [];

        $scope.move = function() {
            if ($scope.folderId == null) return;

            var filterId = filterService.filterId();
            var useUnsavedFilter = filterService.useUnsavedFilter();

            var promise = $scope.id != null ?
                taskService.moveTask($scope.id, $scope.folderId, filterId, useUnsavedFilter) :
                taskService.moveTasks($scope.ids, $scope.folderId, filterId, useUnsavedFilter);

            promise.then(function() {

                $rootScope.$broadcast('notifyReload');

                var tasks = taskService.tasks();

                tasks.forEach(function(task) {
                    task.isSelected = false;
                    task.folderPath = folderService.getFolderPath(task.folderId);
                });

                $location.path('/tasks');
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

            appState.isError = false;

            initializationService
                .initialize()
                .then(function() {

                    appState.currentView = 'Tasks';
                    $scope.folders = folderService.folders();

                    $scope.id = $routeParams['taskId'];
                    if ($scope.id == null) $scope.ids = getSelectedTaskIds();
                });
        }

        function getSelectedTaskIds() {

            return taskService.tasks()
                .filter(function(item) {
                    return item.isSelected;
                })
                .map(function(item) {
                    return item.id;
                });
        }
    }
});