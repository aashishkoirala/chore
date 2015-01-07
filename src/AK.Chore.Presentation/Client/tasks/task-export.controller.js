/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.tasks.task-export.controller.js
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

/* taskExportController - Angular controller for the "Export Tasks" section.
 * @author Aashish Koirala
 */
define(['app'], function (app) {

    taskExportController.$inject = ['$scope', '$location', 'taskService', 'appState'];
    app.register.controller('taskExportController', taskExportController);

    function taskExportController($scope, $location, taskService, appState) {

        $scope.selectedTaskIds = [];
        $scope.exportedData = '';

        $scope.export = function() {

            taskService
                .exportTasks($scope.selectedTaskIds)
                .then(function(exportedData) {
                    $scope.exportedData = exportedData;
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

            $scope.selectedTaskIds = (taskService.tasks() || [])
                .filter(function(item) {
                    return item.isSelected;
                })
                .map(function(item) {
                    return item.id;
                });

            if ($scope.selectedTaskIds.length == 0) $scope.cancel();
        }
    }
});