/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Presentation.filter.service.tests.js
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

/// <reference path="angular-mocks.js" />
/// <reference path="jasmine\jasmine.js" />

'use strict';

/* Tests for filterService.
 * @author Aashish Koirala
 */
define(['app', 'ngMock', 'api.service', 'filter.service'], function (app) {

    app.value('links', []);

    describe('filterService', function() {

        beforeEach(module('app'));

        it('loads filters, makes them available via filters() and sets selected filter properties when loadFilters() is called', function(done) {
            inject(function($q, $rootScope, apiService, filterService) {

                apiService.getFilters = function() {
                    var deferred = $q.defer();
                    deferred.resolve([{ id: 1, name: 'Test1' }, { id: 2, name: 'Test2' }]);
                    return deferred.promise;
                };

                filterService.loadFilters().then(function() {
                    expect(filterService.filters()).not.toBeNull();
                    expect(filterService.filters().length).toBe(2);
                    expect(filterService.filters()[0].id).toBe(1);
                    expect(filterService.filters()[0].name).toBe('Test1');
                    expect(filterService.filters()[1].id).toBe(2);
                    expect(filterService.filters()[1].name).toBe('Test2');
                    expect(filterService.filterId()).toBe(1);
                    expect(filterService.filterName()).toBe('Test1');
                    done();
                });

                $rootScope.$apply();
            });
        });


        it('loads filters again only if not loaded when loadFiltersIfNeeded() is called', function(done) {
            inject(function($q, $rootScope, filterService) {

                filterService.filters([]);
                spyOn(filterService, 'loadFilters').and.callFake(function() {
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });

                filterService.loadFiltersIfNeeded().then(function() {
                    expect(filterService.loadFilters).not.toHaveBeenCalled();
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls getUnsavedFilter() on the API when loadUnsavedFilter() is called', function(done) {
            inject(function($q, $rootScope, apiService, filterService) {

                var testFilter = { id: 1, name: 'Unsaved' };

                apiService.getUnsavedFilter = function(id) {
                    console.log(id);
                    var deferred = $q.defer();
                    deferred.resolve(testFilter);
                    return deferred.promise;
                };

                spyOn(apiService, 'getUnsavedFilter').and.callThrough();

                filterService.loadUnsavedFilter().then(function() {

                    expect(apiService.getUnsavedFilter).toHaveBeenCalledWith(0);
                    expect(filterService.unsavedFilter()).toBe(testFilter);
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('loads unsaved filter again only if not loaded when loadUnsavedFilterIfNeeded() is called', function(done) {
            inject(function($q, $rootScope, filterService) {

                filterService.unsavedFilter({});
                spyOn(filterService, 'loadUnsavedFilter').and.callFake(function() {
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });

                filterService.loadUnsavedFilterIfNeeded().then(function() {
                    expect(filterService.loadUnsavedFilter).not.toHaveBeenCalled();
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls setUnsavedFilter() on the API when saveUnsavedFilter() is called', function(done) {
            inject(function($q, $rootScope, apiService, filterService) {

                var testFilter = { id: 2, name: 'TestFilter' };

                filterService.unsavedFilter(testFilter);

                apiService.setUnsavedFilter = function(filter) {
                    console.log(filter);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'setUnsavedFilter').and.callThrough();

                filterService.saveUnsavedFilter().then(function() {
                    expect(apiService.setUnsavedFilter).toHaveBeenCalledWith(testFilter);
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls addFilter() on the API when addFilter() is called', function(done) {
            inject(function($q, $rootScope, apiService, filterService) {

                filterService.filters([]);

                apiService.addFilter = function(filter) {
                    console.log(filter);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'addFilter').and.callThrough();

                var testFilter = { id: 1, name: 'TestFilter' };

                filterService.addFilter(testFilter).then(function() {

                    expect(apiService.addFilter).toHaveBeenCalledWith(testFilter);
                    expect(filterService.filters().length).toBe(1);
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls updateFilter() on the API when updateFilter() is called', function(done) {
            inject(function($q, $rootScope, apiService, filterService) {

                filterService.filters([]);

                apiService.updateFilter = function(filter) {
                    console.log(filter);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'updateFilter').and.callThrough();

                var testFilter = { id: 1, name: 'TestFilter' };

                filterService.updateFilter(testFilter).then(function() {

                    expect(apiService.updateFilter).toHaveBeenCalledWith(testFilter);
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls deleteFilter() on the API when deleteFilter() is called', function(done) {
            inject(function($q, $rootScope, apiService, filterService) {

                filterService.filters([]);

                apiService.deleteFilter = function(filter) {
                    console.log(filter);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'deleteFilter').and.callThrough();

                var testFilter = { id: 1, name: 'TestFilter' };

                filterService.deleteFilter(testFilter).then(function() {

                    expect(apiService.deleteFilter).toHaveBeenCalledWith(1);
                    done();
                });

                $rootScope.$apply();
            });
        });
    });
});