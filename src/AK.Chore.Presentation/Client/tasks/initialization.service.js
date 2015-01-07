/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.tasks.initialization.service.js
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

/* initializationService - Initializes state (i.e. folders, filters, tasks).
 * @author Aashish Koirala
 */
define(['app', 'folders/folder.service', 'filters/filter.service', 'tasks/task.service'], function (app) {

    initializationService.$inject = ['$q', 'folderService', 'filterService', 'taskService'];
    app.factory('initializationService', initializationService);

    function initializationService($q, folderService, filterService, taskService) {

        return {
            initialize: function(filterId, useUnsavedFilter) {

                var deferred = $q.defer();

                filterService
                    .loadFiltersIfNeeded()
                    .then(function() {

                        filterId = filterId || filterService.filterId();
                        useUnsavedFilter = useUnsavedFilter || false;

                        filterService.filterId(filterId);
                        filterService.useUnsavedFilter(useUnsavedFilter);

                        handleFiltersLoaded(filterId, useUnsavedFilter, deferred.resolve, deferred.reject);

                    }, deferred.reject);

                return deferred.promise;
            },
            
            reinitialize: function(reloadTasks, filterId, useUnsavedFilter) {

                var deferred = $q.defer();

                filterService
                    .loadFilters()
                    .then(function() {

                        filterId = filterId || filterService.filterId();
                        useUnsavedFilter = useUnsavedFilter || false;

                        filterService.filterId(filterId);
                        filterService.useUnsavedFilter(useUnsavedFilter);

                        folderService
                            .loadFolders()
                            .then(function() {

                                if (!reloadTasks) deferred.resolve();
                                else {
                                    taskService
                                        .loadTasks(filterId, useUnsavedFilter)
                                        .then(deferred.resolve, deferred.reject);
                                }

                            }, deferred.reject);

                    }, deferred.reject);

                return deferred.promise;
            }
        };

        function handleFiltersLoaded(filterId, useUnsavedFilter, success, failure) {

            folderService
                .loadFoldersIfNeeded()
                .then(function() {
                    handleFoldersLoaded(filterId, useUnsavedFilter, success, failure);
                }, failure);
        }

        function handleFoldersLoaded(filterId, useUnsavedFilter, success, failure) {

            if (filterService.hasFilterChanged()) {
                filterService.hasFilterChanged(false);
                taskService
                    .loadTasks(filterId, useUnsavedFilter)
                    .then(success, failure);
            } else {
                taskService
                    .loadTasksIfNeeded(filterId, useUnsavedFilter)
                    .then(success, failure);
            }
        }
    }
});