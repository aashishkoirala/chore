/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.tasks.task-edit.controller.js
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

/* taskEditController - Angular controller for the "Edit Task" section.
 * @author Aashish Koirala
 */
define(['app'], function (app) {

    taskEditController.$inject = [
        '$scope', '$rootScope', '$routeParams', '$location',
        'taskService', 'filterService', 'folderService', 'initializationService', 'appState'];

    app.register.controller('taskEditController', taskEditController);

    function taskEditController($scope, $rootScope, $routeParams, $location, taskService, filterService, folderService, initializationService, appState) {

        $scope.lookup = {
            recurrenceTypes: ['Hourly', 'Daily', 'Weekly', 'Monthly', 'Yearly'],
            recurrenceTypeLabels: {
                Hourly: 'hour(s)',
                Daily: 'day(s)',
                Weekly: 'week(s)',
                Monthly: 'month(s)',
                Yearly: 'year(s)'
            },
            daysOfWeek: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
            months: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December']
        };

        $scope.lookup.monthsNumbered = $scope.lookup.months.map(function(item, index) {
            return { number: index + 1, name: item };
        });
        
        $scope.state = {
            folders: [],
            startDatePickerIsOpen: false,
            endDatePickerIsOpen: false,
            startTimeIsSet: false,
            endTimeIsSet: false,
            recurrenceTimeOfDayIsSet: false,
            recurrenceDayOfMonthIsSet: false,
            recurrenceMonthOfYearIsSet: false,
            recurrenceDurationHours: 0,
            recurrenceDurationMinutes: 0,
            daysAreSet: [false, false, false, false, false, false, false],
            validationMessages: [],
            returnToPath: '/tasks'
        };

        $scope.task = {};

        $scope.getMonthNumber = function(month) {
            return $scope.lookup.months.indexOf(month) + 1;
        };

        $scope.isWorking = function() {
            return appState.isWorking;
        };
        
        $scope.openStartDatePicker = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.state.startDatePickerIsOpen = true;
        };

        $scope.openEndDatePicker = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.state.endDatePickerIsOpen = true;
        };

        $scope.save = function() {

            var task = copyTask($scope.task);
            assignTaskBasedOnState(task, $scope.state, $scope.lookup.daysOfWeek);
            prepareTaskForSaving(task, $scope.state);

            $scope.state.validationMessages = [];
            var validationResults = validateTask(task, $scope.state);
            if (!validationResults.valid) {
                $scope.state.validationMessages = validationResults.messages;
                return;
            }

            var filterId = filterService.filterId();
            var useUnsavedFilter = filterService.useUnsavedFilter();

            taskService
                .doWithTask('update', task, filterId, useUnsavedFilter)
                .then(function () {

                    $rootScope.$broadcast('notifyReload');

                    var returnToPath = $scope.returnToPath;
                    if (returnToPath == '/calendar') returnToPath  += '/reload';

                    $location.path(returnToPath);
                });
        };

        $scope.cancel = function() {
            $location.path($scope.returnToPath);
        };

        $scope.$watch('task.isRecurring', function(newValue, oldValue) {

            if (newValue == oldValue) return;
            if (oldValue == null) return;

            if (!newValue && $scope.task.endDate == null)
                $scope.task.endDate = convertDateToString(new Date(), $scope.lookup.months);

            if (newValue && $scope.task.recurrence.type == null)
                $scope.task.recurrence.type = 'Hourly';
        });

        initialize();

        function initialize() {

            appState.currentView = 'Tasks';
            appState.isError = false;
            
            var id = $routeParams['taskId'];
            $scope.returnToPath = '/' + ($routeParams['returnToPath'] || 'tasks');

            initializationService
                .initialize()
                .then(function() {

                    $scope.state.folders = folderService.folders();

                    if (id == undefined || id == null || id == 0) {
                        var folderId = $scope.state.folders[0].id;
                        $scope.task = createNewTask(folderId);
                        assignDefaultValuesToTask($scope.task, $scope.lookup.months);
                        assignStateBasedOnTask($scope.task, $scope.state, $scope.lookup.daysOfWeek);
                        return;
                    }

                    var filterId = filterService.filterId();
                    var useUnsavedFilter = filterService.useUnsavedFilter();

                    taskService
                        .getTask(id, filterId, useUnsavedFilter)
                        .then(function(task) {

                            $scope.task = copyTask(task);
                            assignDefaultValuesToTask($scope.task, $scope.lookup.months);
                            assignStateBasedOnTask($scope.task, $scope.state, $scope.lookup.daysOfWeek);
                        });
                });
        }

        function createNewTask(folderId) {
            return {
                id: 0,
                userId: 0,
                folderId: folderId,
                description: '',
                startDate: null,
                startTime: null,
                endDate: new Date(),
                endTime: null,
                state: 'NotStarted',
                recurrence: {
                    isEnabled: true,
                    type: 'NonRecurring',
                    interval: 1,
                    timeOfDay: null,
                    daysOfWeek: [],
                    dayOfMonth: 1,
                    monthOfYear: 0,
                    duration: '01:00'
                },
                isMundane: false,
                isRecurring: false,
                isLate: false,
                canStart: false,
                canPause: false,
                canResume: false,
                canComplete: false,
                now: new Date(),
                dateOrRecurrenceSummary: null
            };
        }

        function copyTask(task) {
            var key;

            var copiedTask = {};
            for (key in task) copiedTask[key] = task[key];

            copiedTask.recurrence = {};
            for (key in task.recurrence) copiedTask.recurrence[key] = task.recurrence[key];

            return copiedTask;
        }

        function assignDefaultValuesToTask(task, months) {

            task.description = task.description || '';
            task.endDate = task.endDate || convertDateToString(new Date(), months);
            if (task.startDate == undefined) task.startDate = null;
            if (task.endTime == undefined) task.endTime = null;
            if (task.startTime == undefined) task.startTime = null;
            if (task.isRecurring == undefined || task.isRecurring == null) task.isRecurring = false;
            if (task.isMundane == undefined || task.isMundane == null) task.isMundane = false;

            var r = task.recurrence;
            r.type = r.type || 'Hourly';
            r.daysOfWeek = r.daysOfWeek || [];
            r.duration = r.duration || '01:00';

            if (r.isEnabled == undefined || r.isEnabled == null)
                r.isEnabled = true;
            if (r.interval == undefined || r.interval == null || r.interval < 1)
                r.interval = 1;
            if (r.timeOfDay == undefined)
                r.timeOfDay = null;
            if (r.dayOfMonth == undefined || r.dayOfMonth == null || r.dayOfMonth < 1 || r.dayOfMonth > 31)
                r.dayOfMonth = null;
            if (r.monthOfYear == undefined || r.monthOfYear == null || r.monthOfYear < 1 || r.monthOfYear > 12)
                r.monthOfYear = null;

            if (task.isRecurring) {
                task.endDate = null;
                task.endTime = null;
                task.startDate = null;
                task.startTime = null;
            } else {
                r.type = null;
                task.isMundane = false;
            }
        }

        function assignStateBasedOnTask(task, state, daysOfWeek) {

            task.endTime = convertStringToTime(task.endTime);
            task.startTime = convertStringToTime(task.startTime);
            task.recurrence.timeOfDay = convertStringToTime(task.recurrence.timeOfDay);

            var recurrenceDuration = convertStringToTime(task.recurrence.duration);
            if (recurrenceDuration == null) {
                recurrenceDuration = new Date();
                recurrenceDuration.setHours(1);
                recurrenceDuration.setMinutes(0);
            }

            state.endTimeIsSet = task.endTime != null;
            state.startTimeIsSet = task.startTime != null;
            state.recurrenceTimeOfDayIsSet = task.recurrence.timeOfDay != null;
            state.recurrenceDayOfMonthIsSet = task.recurrence.dayOfMonth != null;
            state.recurrenceMonthOfYearIsSet = task.recurrence.monthOfYear != null;
            state.recurrenceDurationHours = recurrenceDuration.getHours();
            state.recurrenceDurationMinutes = recurrenceDuration.getMinutes();

            var daysOfWeekLowerCased = daysOfWeek.map(function(item) {
                return item.toLowerCase();
            });
            
            for (var i = 0; i < state.daysAreSet.length; i++)
                state.daysAreSet[i] = task.recurrence.daysOfWeek.indexOf(daysOfWeekLowerCased[i]) >= 0;
        }

        function assignTaskBasedOnState(task, state, daysOfWeek) {

            task.endTime = state.endTimeIsSet ? convertTimeToString(task.endTime || new Date()) : null;
            task.startTime = state.startTimeIsSet ? convertTimeToString(task.startTime || new Date()) : null;

            task.recurrence.timeOfDay = state.recurrenceTimeOfDayIsSet ? convertTimeToString(task.recurrence.timeOfDay) : null;
            if (!state.recurrenceDayOfMonthIsSet) task.recurrence.dayOfMonth = null;
            if (!state.recurrenceMonthOfYearIsSet) task.recurrence.monthOfYear = null;

            if (state.recurrenceDurationHours == null && state.recurrenceDurationMinutes == null)
                task.recurrence.duration = null;
            else task.recurrence.duration = (state.recurrenceDurationHours || 0) + ':' + (state.recurrenceDurationMinutes || 0);

            task.recurrence.daysOfWeek = [];
            for (var i = 0; i < state.daysAreSet.length; i++)
                if (state.daysAreSet[i]) task.recurrence.daysOfWeek.push(daysOfWeek[i].toLowerCase());
        }

        function prepareTaskForSaving(task, state) {

            var r = task.recurrence;

            if (task.isRecurring) {
                task.endDate = null;
                task.startDate = null;
                task.endTime = null;
                task.startTime = null;

                if (r.interval == undefined || r.interval == null) r.interval = 1;
                if (r.duration == undefined || r.duration == null) r.duration = '01:00';
                if (r.daysOfWeek == undefined || r.daysOfWeek == null) r.daysOfWeek = [];
                if (!state.recurrenceTimeOfDayIsSet && (r.timeOfDay == undefined || r.timeOfDay == null))
                    r.timeOfDay = '00:00';
                if (!state.recurrenceDayOfMonthIsSet && (r.dayOfMonth == undefined || r.dayOfMonth == null))
                    r.dayOfMonth = 1;
                if (!state.recurrenceMonthOfYearIsSet && (r.monthOfYear == undefined || r.monthOfYear == null))
                    r.monthOfYear = 0;

            } else {

                task.isMundane = false;
                var isEnabled = r.isEnabled;
                for (var key in r) r[key] = null;
                r.isEnabled = isEnabled;
                r.type = 'NonRecurring';
                r.duration = '00:00';
                r.timeOfDay = '00:00';
                r.interval = 0;
                r.daysOfWeek = [];
                r.dayOfMonth = 0;
                r.monthOfYear = 0;

                if (task.endDate == null) {
                    task.endTime = null;
                    task.startDate = null;
                }
                if (task.startDate == null) {
                    task.startTime = null;
                }
            }

            task.now = new Date();
        }

        function validateTask(task, state) {

            var result = { valid: true, messages: [] };

            if (task.isRecurring) {

                var r = task.recurrence;
                if (state.recurrenceTimeOfDayIsSet && (r.timeOfDay == undefined || r.timeOfDay == null))
                    result.messages.push('What time of day should this task repeat?');
                if (state.recurrenceDayOfMonthIsSet && (r.dayOfMonth == undefined || r.dayOfMonth == null))
                    result.messages.push('What day of the month should this task repeat or start?');
                if (state.recurrenceMonthOfYearIsSet && (r.monthOfYear == undefined || r.monthOfYear == null))
                    result.messages.push('What month of the year should this task repeat or start?');

            } else {

                if (task.description == undefined || task.description == null || task.description.trim() == '')
                    result.messages.push('You must say what the task actually is!');
                if (task.endDate == undefined || task.endDate == null)
                    result.messages.push('The task must have a finish-by date.');
            }

            result.valid = result.messages.length == 0;
            return result;
        }

        function convertDateToString(date, months) {
            return months[date.getMonth()].substring(0, 3) + ' ' + date.getDate() + ', ' + date.getFullYear();
        }

        function convertTimeToString(time) {
            return time.getHours() + ':' + time.getMinutes();
        }

        function convertStringToTime(timeAsString) {
            
            if (timeAsString == null) return null;

            var indexOfColon = timeAsString.indexOf(':');
            var hourAsString = timeAsString.substring(0, indexOfColon);
            var minutesAsString = timeAsString.substring(indexOfColon + 1, indexOfColon + 3);

            var hours = parseInt(hourAsString);
            var minutes = parseInt(minutesAsString);

            var time = new Date();
            time.setHours(hours);
            time.setMinutes(minutes);

            return time;
        }
    }
});