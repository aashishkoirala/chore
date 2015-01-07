/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.tasks.task-search.controller.js
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

/* taskSearchController - Angular controller for the "Search Tasks" section.
 * @author Aashish Koirala
 */
define(['app'], function (app) {

    taskSearchController.$inject = ['$scope', '$location', 'filterService', 'taskService', 'appState'];
    app.register.controller('taskSearchController', taskSearchController);

    function taskSearchController($scope, $location, filterService, taskService, appState) {

        $scope.criterion = {};
        $scope.save = false;
        $scope.saveAsFilterName = '';

        $scope.canApplyFilter = function() {
            return ($scope.save && $scope.saveAsFilterName != null && $scope.saveAsFilterName.trim() != '') || (!$scope.save);
        };
        
        $scope.applyFilter = function() {

            if ($scope.save) {

                var filter = {
                    id: 0,
                    name: $scope.saveAsFilterName,
                    criterion: $scope.criterion
                };

                filterService
                    .addFilter(filter).then(function() {

                        applyFilterToTasks($scope.criterion);
                    });

            } else applyFilterToTasks($scope.criterion);
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

            filterService
                .loadUnsavedFilterIfNeeded()
                .then(function(filter) {

                    $scope.criterion = filter.criterion;
                });
        }

        function applyFilterToTasks(criterion) {

            var unsavedFilter = filterService.unsavedFilter();
            unsavedFilter.criterion = criterion;

            filterService
                .saveUnsavedFilter()
                .then(function() {

                    var filterId = filterService.filterId();
                    filterService.useUnsavedFilter(true);
                    filterService.hasFilterChanged(true);
                    $location.path('/tasks/' + filterId + '/true');
                });
        }
    }
});