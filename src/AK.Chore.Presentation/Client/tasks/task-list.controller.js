/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.tasks.task-list.controller.js
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

/* taskListController - Angular controller for the "Task List" section.
 * @author Aashish Koirala
 */
define(['app'], function (app) {

    taskListController.$inject = [
        '$scope', '$rootScope', '$routeParams', '$location',
        'filterService', 'folderService', 'taskService', 'initializationService',
        'appState'];

    app.register.controller('taskListController', taskListController);

    function taskListController($scope, $rootScope, $routeParams, $location,
        filterService, folderService, taskService, initializationService, appState) {

        $scope.state = {
            canExport: false,
            canDoAnythingInBulk: false,
            canStartInBulk: false,
            canPauseInBulk: false,
            canResumeInBulk: false,
            canCompleteInBulk: false,
            canEnableRecurrenceInBulk: false,
            canDisableRecurrenceInBulk: false,
            canMoveInBulk: false,
            canDeleteInBulk: false
        };

        $scope.tasks = function() {
            return taskService.tasks();
        };

        $scope.isWorking = function() {
            return appState.isWorking;
        };
        
        $scope.selectedTaskCount = function() {

            var tasks = $scope.tasks() || [];

            var selectedTasks = tasks.filter(function(item) {
                return item.isSelected;
            });

            return selectedTasks.length;
        };

        $scope.handleOtherActionsDropDownToggle = function(isOpen) {

            if (!isOpen) return;

            var state = $scope.state;
            var tasks = $scope.tasks() || [];

            var selectedTasks = tasks.filter(function(item) {
                return item.isSelected;
            });

            state.canExport = selectedTasks.length > 0;
            state.canDoAnythingInBulk = selectedTasks.length > 0;
            state.canMoveInBulk = selectedTasks.length > 0;
            state.canDeleteInBulk = selectedTasks.length > 0;

            state.canStartInBulk = selectedTasks.filter(function(item) {
                return item.canStart;
            }).length > 0;
            state.canPauseInBulk = selectedTasks.filter(function(item) {
                return item.canPause;
            }).length > 0;
            state.canResumeInBulk = selectedTasks.filter(function(item) {
                return item.canResume;
            }).length > 0;
            state.canCompleteInBulk = selectedTasks.filter(function(item) {
                return item.canComplete;
            }).length > 0;

            state.canEnableRecurrenceInBulk = selectedTasks.filter(function(item) {
                return item.isRecurring && !item.recurrence.isEnabled;
            }).length > 0;
            state.canDisableRecurrenceInBulk = selectedTasks.filter(function(item) {
                return item.isRecurring && item.recurrence.isEnabled;
            }).length > 0;
        };

        $scope.search = function() {
            $location.path('/search');
        };

        $scope.add = function() {
            $location.path('/task');
        };

        $scope.reload = function() {

            var filterId = filterService.filterId();
            var useUnsavedFilter = filterService.useUnsavedFilter();

            taskService
                .loadTasks(filterId, useUnsavedFilter)
                .then(function() {
                });
        };

        $scope.start = function(task) {
            doWithTask('start', task);
        };

        $scope.pause = function(task) {
            doWithTask('pause', task);
        };

        $scope.resume = function(task) {
            doWithTask('resume', task);
        };

        $scope.complete = function(task) {
            doWithTask('complete', task);
        };

        $scope.enableRecurrence = function(task) {
            doWithTask('enableRecurrence', task);
        };

        $scope.disableRecurrence = function(task) {
            doWithTask('disableRecurrence', task);
        };

        $scope.delete = function(task) {

            if (appState.isWorking) return;

            task.isInProcess = true;
            taskService
                .deleteTask(task)
                .then(function() {
                    $rootScope.$broadcast('notifyReload');
                }, function() {
                    task.isInProcess = false;
                });
        };

        $scope.selectAll = function() {
            $scope.tasks().forEach(function(task) {
                task.isSelected = true;
            });
        };

        $scope.selectNone = function() {
            $scope.tasks().forEach(function(task) {
                task.isSelected = false;
            });
        };

        $scope.startInBulk = function() {
            doWithSelectedTasks('start', function(item) {
                return item.canStart;
            });
        };

        $scope.pauseInBulk = function() {
            doWithSelectedTasks('pause', function(item) {
                return item.canPause;
            });
        };

        $scope.resumeInBulk = function() {
            doWithSelectedTasks('resume', function(item) {
                return item.canResume;
            });
        };

        $scope.completeInBulk = function() {
            doWithSelectedTasks('complete', function(item) {
                return item.canComplete;
            });
        };

        $scope.enableRecurrenceInBulk = function() {
            doWithSelectedTasks('enableRecurrence', function(item) {
                return item.isRecurring && !item.recurrence.isEnabled;
            });
        };

        $scope.disableRecurrenceInBulk = function() {
            doWithSelectedTasks('disableRecurrence', function(item) {
                return item.isRecurring && item.recurrence.isEnabled;
            });
        };

        $scope.deleteInBulk = function() {

            if (appState.isWorking) return;

            var filterId = filterService.filterId();
            var useUnsavedFilter = filterService.useUnsavedFilter();

            taskService
                .deleteTasks(filterId, useUnsavedFilter)
                .then(function() {
                    $rootScope.$broadcast('notifyReload');
                });
        };

        $scope.$on('reloadTasks', function() {
            $scope.reload();
        });

        initialize();

        function initialize() {

            appState.currentView = 'Tasks';
            appState.isError = false;

            var filterId = $routeParams['filterId'] || filterService.filterId();
            var useUnsavedFilter = $routeParams['useUnsavedFilter'] || false;

            initializationService
                .initialize(filterId, useUnsavedFilter)
                .then(function() {
                });
        }

        function doWithTask(action, task) {

            if (appState.isWorking) return;
            
            var state = task.state;
            if (action == 'complete') task.state = 'Completed';

            task.isInProcess = true;
            var filterId = filterService.filterId();
            var useUnsavedFilter = filterService.useUnsavedFilter();

            taskService
                .doWithTaskId(action, task.id, filterId, useUnsavedFilter)
                .then(function() {
                    $rootScope.$broadcast('notifyReload');
                }, function() {
                    task.isInProcess = false;
                    if (action == 'complete') task.state = state;
                });
        }

        function doWithSelectedTasks(action, filterFunction) {

            if (appState.isWorking) return;

            var selectedTasks = ($scope.tasks() || [])
                .filter(function(item) {
                    return item.isSelected && (filterFunction == undefined || filterFunction == null || filterFunction(item));
                });

            if (selectedTasks.length == 0) return;
            if (selectedTasks.length == 1) {
                doWithTask(action, selectedTasks[0]);
                return;
            }

            selectedTasks.forEach(function(item) {
                item.isInProcess = true;
            });

            var selectedTaskIds = selectedTasks
                .map(function(item) {
                    return item.id;
                });

            var filterId = filterService.filterId();
            var useUnsavedFilter = filterService.useUnsavedFilter();

            taskService
                .doWithTaskIds(selectedTaskIds, action, filterId, useUnsavedFilter)
                .then(function() {
                    $rootScope.$broadcast('notifyReload');
                }, function() {
                    selectedTasks.forEach(function(item) {
                        item.isInProcess = true;
                    });
                });
        }        
    }    
});