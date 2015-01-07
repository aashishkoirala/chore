/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.tasks.task.service.js
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

/* taskService - Talks to the web API for task related stuff and state maintenance.
 * @author Aashish Koirala
 */
define(['app', 'api.service'], function (app) {

    taskService.$inject = ['$q', 'apiService'];
    return app.factory('taskService', taskService);

    function taskService($q, apiService) {

        return {
            _tasks: null,

            tasks: function(value) {
                if (value != undefined) this._tasks = value;
                return this._tasks;
            },

            loadTasks: function(filterId, useUnsavedFilter) {

                var deferred = $q.defer();
                var self = this;

                apiService
                    .getTasks({
                        filterId: filterId == null ? 0 : filterId,
                        useUnsavedFilter: useUnsavedFilter
                    })
                    .then(function(response) {
                        self._tasks = response;
                        resetTasksIsSelected(self._tasks);
                        deferred.resolve(self._tasks);
                    });

                return deferred.promise;
            },

            loadTasksIfNeeded: function(filterId, useUnsavedFilter) {

                var deferred = $q.defer();
                var self = this;

                if (self._tasks != null) {
                    deferred.resolve({ tasks: self._tasks, wasNeeded: false });
                    return deferred.promise;
                }

                self.loadTasks(filterId, useUnsavedFilter)
                    .then(deferred.resolve, deferred.reject);

                return deferred.promise;
            },

            getTask: function(id, filterId, useUnsavedFilter, forceRefresh) {

                forceRefresh = forceRefresh || false;

                var deferred = $q.defer();
                var self = this;

                var existingTask = findTaskById(self._tasks, id);

                if (existingTask != null && !forceRefresh) {
                    deferred.resolve(existingTask);
                    return deferred.promise;
                }

                apiService
                    .getTask(id)
                    .then(function(task) {

                        apiService
                            .checkTaskSatisfiesFilter({
                                taskId: id,
                                filterId: filterId,
                                useUnsavedFilter: useUnsavedFilter
                            })
                            .then(function(satisfies) {
                                if (satisfies.satisfies && self._tasks != null)
                                    self._tasks.push(task);
                                task.isSelected = false;
                                deferred.resolve(task);
                            }, deferred.reject);
                    }, deferred.reject);

                return deferred.promise;
            },

            doWithTask: function(action, task, filterId, useUnsavedFilter) {

                var deferred = $q.defer();
                var self = this;

                if (action == 'update') action = task.id == 0 ? 'add' : 'update';
                action += 'Task';

                apiService[action](task)
                    .then(function(taskResponse) {

                        task.id = taskResponse.id;

                        apiService
                            .checkTaskSatisfiesFilter({
                                taskId: task.id,
                                filterId: filterId,
                                useUnsavedFilter: useUnsavedFilter
                            })
                            .then(function(satisfies) {
                                if (satisfies.satisfies)
                                    replaceTask(self._tasks, taskResponse);
                                else removeTask(self._tasks, task.id);

                                taskResponse.isSelected = false;
                                deferred.resolve(taskResponse);
                            }, deferred.reject);
                    }, deferred.reject);

                return deferred.promise;
            },

            doWithTaskId: function(action, taskId, filterId, useUnsavedFilter) {

                var deferred = $q.defer();
                var self = this;

                action += 'Task';
                action = action == 'enableRecurrenceTask' ? 'enableTaskRecurrence' : action;
                action = action == 'disableRecurrenceTask' ? 'disableTaskRecurrence' : action;

                apiService[action]({ id: taskId })
                    .then(function(taskResponse) {

                        apiService
                            .checkTaskSatisfiesFilter({
                                taskId: taskId,
                                filterId: filterId,
                                useUnsavedFilter: useUnsavedFilter
                            })
                            .then(function(satisfies) {
                                if (satisfies.satisfies)
                                    replaceTask(self._tasks, taskResponse);
                                else removeTask(self._tasks, taskId);

                                taskResponse.isSelected = false;
                                deferred.resolve(taskResponse);
                            }, deferred.reject);
                    }, deferred.reject);

                return deferred.promise;
            },

            doWithTasks: function(tasks, action, filterId, useUnsavedFilter) {

                var self = this;
                var deferred = $q.defer();
                action += 'Tasks';

                apiService[action](tasks).then(function() {
                    self
                        .loadTasks(filterId, useUnsavedFilter)
                        .then(deferred.resolve, deferred.reject);
                }, deferred.reject);

                return deferred.promise;
            },

            doWithTaskIds: function(ids, action, filterId, useUnsavedFilter) {

                var self = this;
                var deferred = $q.defer();
                action += 'Tasks';

                var idObjects = ids.map(function(id) {
                    return { id: id };
                });

                apiService[action](idObjects).then(function() {
                    self
                        .loadTasks(filterId, useUnsavedFilter)
                        .then(deferred.resolve, deferred.reject);
                }, deferred.reject);

                return deferred.promise;
            },

            deleteTask: function(task) {

                var isNewTask = task.id == 0;
                var deferred = $q.defer();
                var self = this;

                deleteTaskIfPersisted(function() {
                    var index = self._tasks.indexOf(task);
                    if (index >= 0) self._tasks.splice(index, 1);
                    deferred.resolve();
                });

                function deleteTaskIfPersisted(success) {
                    if (isNewTask) success();
                    else apiService.deleteTask(task.id).then(success, deferred.reject);
                }

                return deferred.promise;
            },

            deleteTasks: function(filterId, useUnsavedFilter) {

                var self = this;
                var deferred = $q.defer();

                var tasksToOperateOn = getSelectedTasks(self._tasks);

                if (tasksToOperateOn == null) {
                    deferred.resolve();
                    return deferred.promise;
                }
                var ids = tasksToOperateOn.map(function(item) { return item.id; });
                apiService.deleteTasks(ids).then(function() {
                    self
                        .loadTasks(filterId, useUnsavedFilter)
                        .then(deferred.resolve, deferred.reject);
                }, deferred.reject);

                return deferred.promise;
            },

            moveTask: function(id, folderId, filterId, useUnsavedFilter) {

                var self = this;
                var deferred = $q.defer();

                apiService
                    .moveTask({
                        taskIds: [id],
                        folderId: folderId
                    })
                    .then(function() {

                        var task = findTaskById(self._tasks, id);
                        if (task == null) {
                            deferred.resolve();
                            return;
                        }

                        task.folderId = folderId;
                        task.isSelected = false;

                        apiService
                            .checkTaskSatisfiesFilter({
                                taskId: id,
                                filterId: filterId,
                                useUnsavedFilter: useUnsavedFilter
                            })
                            .then(function(satisfies) {

                                if (satisfies.satisfies) replaceTask(self._tasks, task);
                                else removeTask(self._tasks, id);

                                deferred.resolve();
                            }, deferred.reject);
                    }, deferred.reject);

                return deferred.promise;
            },
            
            moveTasks: function(ids, folderId, filterId, useUnsavedFilter) {

                var self = this;
                var deferred = $q.defer();

                apiService
                    .moveTasks({ taskIds: ids, folderId: folderId })
                    .then(function() {
                        self
                            .loadTasks(filterId, useUnsavedFilter)
                            .then(deferred.resolve, deferred.reject);
                    }, deferred.reject);

                return deferred.promise;
            },
            
            exportTasks: function(ids) {

                var deferred = $q.defer();

                apiService
                    .exportTasks({ taskIds: ids })
                    .then(function(response) {
                        deferred.resolve(response.exportedData);
                    }, deferred.reject);

                return deferred.promise;
            },
            
            importTasks: function(filterId, useUnsavedFilter, importData) {

                var deferred = $q.defer();
                var self = this;

                apiService
                    .importTasks({ importData: importData })
                    .then(function (response) {

                        self
                            .loadTasks(filterId, useUnsavedFilter)
                            .then(function() {
                                deferred.resolve(response.importResults.results);
                            }, deferred.reject);

                    }, deferred.reject);

                return deferred.promise;
            }
        };

        function findTaskById(tasks, id) {

            var matchingTasks = tasks.filter(function(item) {
                return item.id == id;
            });

            return matchingTasks.length == 0 ? null : matchingTasks[0];
        }

        function getSelectedTasks(tasks) {

            var selectedTasks = tasks.filter(function(item) {
                return item.isSelected;
            });

            if (selectedTasks.length == 0) return null;
            return selectedTasks;
        }

        function replaceTask(tasks, task) {

            var taskToReplace = findTaskById(tasks, task.id);

            if (taskToReplace == null) tasks.push(task);
            else {
                var index = tasks.indexOf(taskToReplace);
                tasks.splice(index, 1, task);
            }
        }

        function removeTask(tasks, id) {

            var taskToRemove = findTaskById(tasks, id);

            if (taskToRemove != null) {
                var index = tasks.indexOf(taskToRemove);
                tasks.splice(index, 1);
            }
        }

        function resetTasksIsSelected(tasks) {
            (tasks || []).forEach(function(task) {
                task.isSelected = false;
            });
        }
    }
});