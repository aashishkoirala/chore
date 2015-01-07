/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.filters.filters.controller.js
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

/* filtersController - Angular controller for the "Filters" section.
 * @author Aashish Koirala
 */
define(['app'], function (app) {

    filtersController.$inject = ['$scope', '$rootScope', '$routeParams', '$location', '$timeout',
        'initializationService', 'filterService', 'appState'];
    app.register.controller('filtersController', filtersController);

    function filtersController($scope, $rootScope, $routeParams, $location, $timeout, initializationService, filterService, appState) {

        $scope.state = {
            currentFilter: null,
            showSuccess: false
        };

        $scope.filters = function() {
            return filterService.filters();
        };

        $scope.isWorking = function() {
            return appState.isWorking;
        };
        
        $scope.selectFilter = function(filter) {
            $scope.state.currentFilter = JSON.parse(JSON.stringify(filter));
        };
        
        $scope.cancel = function() {
            $location.path('/tasks');
        };

        $scope.add = function() {
            $scope.state.currentFilter = { id: 0, name: 'New Filter', userId: 0, criterion: { type: 'true' } };
        };
        
        $scope.save = function () {
            
            var filter = $scope.state.currentFilter;
            if (filter == null) return;

            var promise = filter.id == 0 ? filterService.addFilter(filter) : filterService.updateFilter(filter);

            promise.then(function () {
                $scope.state.showSuccess = true;
                $timeout(function() {
                    $scope.state.showSuccess = false;
                }, 1000);
                $rootScope.$broadcast('notifyReload');
            });
        };

        $scope.delete = function() {

            var filter = $scope.state.currentFilter;
            if (filter == null) return;

            filterService
                .deleteFilter(filter)
                .then(function() {
                    $scope.selectFilter($scope.filters()[0]);
                    $scope.state.showSuccess = true;
                    $timeout(function () {
                        $scope.state.showSuccess = false;
                    }, 1000);
                    $rootScope.$broadcast('notifyReload');
                });
        };

        initialize();

        function initialize() {

            appState.currentView = 'Filters';
            appState.isError = false;

            var filterId = $routeParams['filterId'];

            initializationService
                .initialize()
                .then(function () {

                    var filters = $scope.filters();
                    var filter = filterId == null ? filters[0] : (findFilterById(filters, filterId) || filters[0]);

                    $scope.selectFilter(filter);
                });
        }
        
        function findFilterById(filters, id) {
            var matches = filters.filter(function(item) {
                return item.id == id;
            });
            return matches.length == 0 ? null : matches[0];
        }
    }
});