/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Presentation.task.service.tests.js
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

/* Tests for taskService.
 * @author Aashish Koirala
 */
define(['app', 'ngMock', 'api.service', 'task.service'], function (app) {

    app.value('links', []);

    describe('taskService', function() {

        beforeEach(module('app'));

        it('loads tasks and makes them available via tasks() when loadTasks() is called', function(done) {
            inject(function($q, $rootScope, apiService, taskService) {

                var testTasks = [{ id: 1, description: "Test task" }];
                apiService.getTasks = function(filterId, useUnsavedFilter) {
                    console.log(filterId);
                    console.log(useUnsavedFilter);

                    var deferred = $q.defer();
                    deferred.resolve(testTasks);
                    return deferred.promise;
                };

                spyOn(apiService, 'getTasks').and.callThrough();

                taskService.loadTasks(1, true).then(function() {
                    expect(apiService.getTasks).toHaveBeenCalledWith({ filterId: 1, useUnsavedFilter: true });
                    expect(taskService.tasks()).toBe(testTasks);

                    done();
                });

                $rootScope.$apply();
            });
        });

        it('loads tasks again only if they are not loaded when loadTasksIfNeeded() is called', function(done) {
            inject(function($q, $rootScope, taskService) {

                spyOn(taskService, 'loadTasks').and.callFake(function(filterId, useUnsavedFilter) {
                    console.log(filterId);
                    console.log(useUnsavedFilter);

                    var deferred = $q.defer();
                    deferred.resolve([]);
                    return deferred.promise;
                });

                taskService.tasks([]);
                taskService.loadTasksIfNeeded(1, true).then(function() {
                    expect(taskService.loadTasks).not.toHaveBeenCalled();
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls getTask and checkTaskSatisfiesFilter on API when getTask() is called', function(done) {
            inject(function($q, $rootScope, apiService, taskService) {

                apiService.getTask = function(id) {
                    console.log(id);
                    var deferred = $q.defer();
                    deferred.resolve({ id: id, description: 'Test Task' });
                    return deferred.promise;
                };

                apiService.checkTaskSatisfiesFilter = function(request) {
                    console.log(request);
                    var deferred = $q.defer();
                    deferred.resolve({ satisfies: true });
                    return deferred.promise;
                };

                spyOn(apiService, 'getTask').and.callThrough();
                spyOn(apiService, 'checkTaskSatisfiesFilter').and.callThrough();

                taskService.tasks([]);
                taskService.getTask(1, 2, true).then(function(task) {
                    expect(task.id).toBe(1);
                    expect(task.description).toBe('Test Task');
                    expect(apiService.getTask).toHaveBeenCalledWith(1);
                    expect(apiService.checkTaskSatisfiesFilter).toHaveBeenCalledWith({ taskId: 1, filterId: 2, useUnsavedFilter: true });

                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls appropriate API method followed by checkTaskSatisfiesFilter() when doWithTask() is called', function(done) {
            inject(function($q, $rootScope, apiService, taskService) {

                apiService.fooTask = function(task) {
                    var deferred = $q.defer();
                    deferred.resolve(task);
                    return deferred.promise;
                };

                apiService.checkTaskSatisfiesFilter = function(request) {
                    console.log(request);
                    var deferred = $q.defer();
                    deferred.resolve({ satisfies: true });
                    return deferred.promise;
                };

                spyOn(apiService, 'fooTask').and.callThrough();
                spyOn(apiService, 'checkTaskSatisfiesFilter').and.callThrough();

                var testTask = { id: 1, description: 'Test' };

                taskService.tasks([]);
                taskService.doWithTask('foo', testTask, 1, true).then(function(response) {
                    expect(response).toBe(testTask);
                    expect(apiService.fooTask).toHaveBeenCalledWith(testTask);
                    expect(apiService.checkTaskSatisfiesFilter).toHaveBeenCalled();

                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls appropriate API method followed by checkTaskSatisfiesFilter() when doWithTaskId() is called', function(done) {
            inject(function($q, $rootScope, apiService, taskService) {

                var testTask = { id: 1, description: 'Test' };

                apiService.fooTask = function(task) {
                    console.log(task);
                    var deferred = $q.defer();
                    deferred.resolve(testTask);
                    return deferred.promise;
                };

                apiService.checkTaskSatisfiesFilter = function(request) {
                    console.log(request);
                    var deferred = $q.defer();
                    deferred.resolve({ satisfies: true });
                    return deferred.promise;
                };

                spyOn(apiService, 'fooTask').and.callThrough();
                spyOn(apiService, 'checkTaskSatisfiesFilter').and.callThrough();

                taskService.tasks([]);
                taskService.doWithTaskId('foo', 1, testTask, 1, true).then(function(response) {
                    expect(response).toBe(testTask);
                    expect(apiService.fooTask).toHaveBeenCalledWith({ id: 1 });
                    expect(apiService.checkTaskSatisfiesFilter).toHaveBeenCalled();

                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls appropriate API method followed by loadTasks() when doWithTasks() is called', function(done) {
            inject(function($q, $rootScope, apiService, taskService) {

                apiService.fooTasks = function(tasks) {
                    console.log(tasks);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'fooTasks').and.callThrough();
                spyOn(taskService, 'loadTasks').and.callFake(function() {
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });

                taskService.tasks([]);
                taskService.doWithTasks([], 'foo', 1, true).then(function() {
                    expect(apiService.fooTasks).toHaveBeenCalledWith([]);
                    expect(taskService.loadTasks).toHaveBeenCalled();

                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls appropriate API method followed by loadTasks() when doWithTaskIds() is called', function(done) {
            inject(function($q, $rootScope, apiService, taskService) {

                apiService.fooTasks = function(tasks) {
                    console.log(tasks);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'fooTasks').and.callThrough();
                spyOn(taskService, 'loadTasks').and.callFake(function() {
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });

                taskService.tasks([]);
                taskService.doWithTaskIds([], 'foo', 1, true).then(function() {
                    expect(apiService.fooTasks).toHaveBeenCalledWith([]);
                    expect(taskService.loadTasks).toHaveBeenCalled();

                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls deleteTask on the API when deleteTask() is called', function(done) {

            inject(function($q, $rootScope, apiService, taskService) {
                apiService.deleteTask = function(task) {
                    console.log(task);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'deleteTask').and.callThrough();

                taskService.tasks([]);
                taskService.deleteTask({ id: 1 }).then(function() {
                    expect(apiService.deleteTask).toHaveBeenCalledWith(1);
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls deleteTasks on the API when deleteTasks() is called', function(done) {

            inject(function($q, $rootScope, apiService, taskService) {
                apiService.deleteTasks = function(tasks) {
                    console.log(tasks);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'deleteTasks').and.callThrough();
                spyOn(taskService, 'loadTasks').and.callFake(function() {
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });

                taskService.tasks([{ id: 1, isSelected: true }]);
                taskService.deleteTasks(1, true).then(function() {
                    expect(apiService.deleteTasks).toHaveBeenCalled();
                    expect(taskService.loadTasks).toHaveBeenCalled();
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls moveTask on the API when moveTask() is called', function(done) {

            inject(function($q, $rootScope, apiService, taskService) {
                apiService.moveTask = function(taskIds, folderId) {
                    console.log(taskIds);
                    console.log(folderId);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'moveTask').and.callThrough();

                taskService.tasks([]);
                taskService.moveTask(1, 2, 3, true).then(function() {
                    expect(apiService.moveTask).toHaveBeenCalledWith({ taskIds: [1], folderId: 2 });
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls moveTasks on the API when moveTasks() is called', function(done) {

            inject(function($q, $rootScope, apiService, taskService) {
                apiService.moveTasks = function(tasks) {
                    console.log(tasks);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'moveTasks').and.callThrough();
                spyOn(taskService, 'loadTasks').and.callFake(function() {
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });

                taskService.tasks([{ id: 1, isSelected: true }]);
                taskService.moveTasks([1], 2, 1, true).then(function() {
                    expect(apiService.moveTasks).toHaveBeenCalled();
                    expect(taskService.loadTasks).toHaveBeenCalled();
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls exportTasks on the API when exportTasks() is called', function(done) {

            inject(function($q, $rootScope, apiService, taskService) {
                apiService.exportTasks = function(request) {
                    console.log(request.taskIds);
                    var deferred = $q.defer();
                    deferred.resolve({ exportedData: '' });
                    return deferred.promise;
                };

                spyOn(apiService, 'exportTasks').and.callThrough();

                taskService.tasks([{ id: 1, isSelected: true }]);
                taskService.exportTasks([1, 2]).then(function() {
                    expect(apiService.exportTasks).toHaveBeenCalledWith({ taskIds: [1, 2] });
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls importTasks on the API when importTasks() is called', function(done) {

            inject(function($q, $rootScope, apiService, taskService) {
                apiService.importTasks = function(request) {
                    console.log(request.importData);
                    var deferred = $q.defer();
                    deferred.resolve({ importResults: [] });
                    return deferred.promise;
                };

                spyOn(apiService, 'importTasks').and.callThrough();
                spyOn(taskService, 'loadTasks').and.callFake(function() {
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });

                taskService.tasks([{ id: 1, isSelected: true }]);
                taskService.importTasks(1, true, 'Test').then(function() {
                    expect(apiService.importTasks).toHaveBeenCalledWith({ importData: 'Test' });
                    expect(taskService.loadTasks).toHaveBeenCalled();
                    done();
                });

                $rootScope.$apply();
            });
        });
    });
});