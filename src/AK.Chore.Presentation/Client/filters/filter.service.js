/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.filters.filters.service.js
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

/* filterService - Talks to the web API for filter related stuff and state maintenance.
 * @author Aashish Koirala
 */
define(['app', 'api.service'], function (app) {

    filterService.$inject = ['$q', 'apiService'];
    return app.factory('filterService', filterService);

    function filterService($q, apiService) {

        return {
            _filterId: null,
            _filters: null,
            _filterName: null,
            _unsavedFilter: null,
            _useUnsavedFilter: false,
            _hasFilterChanged: false,

            filterId: function(value) {
                if (value != undefined) {
                    this._hasFilterChanged = this._hasFilterChanged || this._filterId != value;
                    this._filterId = value;
                    this._filterName = getFilterName(this._filters, this._filterId);
                }
                return this._filterId;
            },

            filters: function(value) {
                if (value != undefined) {
                    this._hasFilterChanged = this._hasFilterChanged || this._filters != value;
                    this._filters = value;
                    this._filterName = getFilterName(this._filters, this._filterId);
                }
                return this._filters;
            },

            filterName: function() {
                return this._filterName;
            },

            unsavedFilter: function(value) {
                if (value != undefined) {
                    this._hasFilterChanged = true;
                    this._unsavedFilter = value;
                }
                return this._unsavedFilter;
            },

            useUnsavedFilter: function(value) {
                if (value != undefined) {
                    this._hasFilterChanged = this._hasFilterChanged || this._useUnsavedFilter != value;
                    this._useUnsavedFilter = value;
                    this._filterName = getFilterName(this._filters, this._filterId, this._useUnsavedFilter);
                }
                return this._useUnsavedFilter;
            },
            
            hasFilterChanged: function(value) {
                if (value != undefined) this._hasFilterChanged = value;
                return this._hasFilterChanged;
            },

            loadFilters: function() {

                var deferred = $q.defer();
                var self = this;

                apiService
                    .getFilters()
                    .then(function(response) {
                        self._filters = response;
                        self._filterId = response[0].id;
                        self._filterName = getFilterName(self._filters, self._filterId);
                        deferred.resolve();
                    });

                return deferred.promise;
            },

            loadFiltersIfNeeded: function() {

                var deferred = $q.defer();
                var self = this;

                if (self._filters != null) deferred.resolve();
                else self.loadFilters().then(deferred.resolve);

                return deferred.promise;
            },
            
            loadUnsavedFilter: function() {

                var deferred = $q.defer();
                var self = this;

                apiService
                    .getUnsavedFilter(0)
                    .then(function(filter) {

                        self._unsavedFilter = filter;
                        deferred.resolve(filter);
                    }, deferred.reject);

                return deferred.promise;
            },
            
            loadUnsavedFilterIfNeeded: function() {

                var deferred = $q.defer();
                var self = this;

                if (self._unsavedFilter != null) {
                    deferred.resolve(self._unsavedFilter);
                } else {
                    self.loadUnsavedFilter().then(function(filter) {
                        deferred.resolve(filter);
                    }, deferred.reject);
                }

                return deferred.promise;
            },
            
            saveUnsavedFilter: function () {
                return apiService.setUnsavedFilter(this._unsavedFilter);
            },
            
            addFilter: function (filter) {
                
                var deferred = $q.defer();
                var self = this;

                apiService
                    .addFilter(filter)
                    .then(function(filterResponse) {

                        self._filters.push(filterResponse);
                        deferred.resolve();
                    }, deferred.reject);

                return deferred.promise;
            },
            
            updateFilter: function(filter) {

                var deferred = $q.defer();
                var self = this;

                apiService
                    .updateFilter(filter)
                    .then(function(filterResponse) {

                        var existingFilter = findFilterById(self._filters);
                        if (existingFilter != null) {
                            existingFilter.id = filterResponse.id;
                            existingFilter.name = filterResponse.name;
                            existingFilter.criterion = filterResponse.criterion;
                        }

                        deferred.resolve();
                    }, deferred.reject);

                return deferred.promise;
            },
            
            deleteFilter: function(filter) {

                var deferred = $q.defer();
                var self = this;
                var index = self._filters.indexOf(filter);

                var filterId = self.filterId();

                if (filter.id == 0) {
                    self._filters.splice(index, 1);
                    deferred.resolve();
                    return deferred.promise;
                }

                apiService
                    .deleteFilter(filter.id)
                    .then(function() {
                        self._filters.splice(index, 1);
                        if (filterId == filter.id) self.filterId(self.filters()[0].id);
                        deferred.resolve();
                    }, deferred.reject);

                return deferred.promise;
            }
        };

        function getFilterName(filters, filterId, useUnsavedFilter) {

            if (useUnsavedFilter != undefined && useUnsavedFilter) return 'Custom';

            if (filters == null) return 'Unknown';

            var matchingFilters = filters.filter(function(item) {
                return item.id == filterId;
            });

            if (matchingFilters.length == 0) return 'Unknown';
            return matchingFilters[0].name;
        }

        function findFilterById(filters, id) {

            var matchingFilters = filters.filter(function(item) {
                return item.id == id;
            });

            return matchingFilters.length == 0 ? null : matchingFilters[0];
        }
    }
});