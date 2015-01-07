/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.calendar.calendar.controller.js
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

/* calendarController - Angular controller for the "calendar" section.
 * @author Aashish Koirala
 */
define(['app'], function (app) {

    calendarController.$inject = ['$scope', '$routeParams', 'initializationService', 'calendarService', 'folderService', 'appState'];
    app.register.controller('calendarController', calendarController);

    function calendarController($scope, $routeParams, initializationService, calendarService, folderService, appState) {

        $scope.state = {
            dayInWeek: null,
            datePickerIsOpen: false,
            folderStyleMap: {},
            currentDay: null
        };

        $scope.calendarWeek = function() {
            return calendarService.calendarWeek();
        };

        $scope.isWorking = function() {
            return appState.isWorking;
        };
        
        $scope.openDatePicker = function($event) {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.state.datePickerIsOpen = true;
        };

        $scope.getTimeStringFromHour = function(hour) {

            var suffix = 'AM';
            if (hour == 12) suffix = 'PM';
            else if (hour > 12) {
                hour -= 12;
                suffix = 'PM';
            } else if (hour == 0) hour = 12;

            var hourString = '' + hour;
            if (hourString.length < 2) hourString = '0' + hourString;

            return hourString + ':00 ' + suffix;
        };

        $scope.getDayName = function(day) {
            if (day == undefined) return '';
            return day.substring(0, 1).toUpperCase() + day.substring(1, day.length);
        };

        $scope.abbreviate = function(text, maxChars) {
            if (text == undefined) return '';
            return text.length <= maxChars ? text : text.substring(0, maxChars - 2) + '...';
        };

        $scope.getFolderStyle = function(id) {
            return $scope.state.folderStyleMap['f' + id];
        };

        $scope.showItems = function(items) {
            return items.filter(function(item) {
                return item.isBlank == undefined;
            }).length > 0;
        };

        $scope.reload = function() {
            var dayInWeek = calendarService.dayInWeek();
            calendarService
                .loadCalendarWeek(dayInWeek)
                .then(function() {
                    $scope.state.currentDay = calendarService.calendarWeek().days[0];
                });
        };

        $scope.$on('reloadCalendar', $scope.reload);
        
        initialize();

        function initialize() {

            appState.currentView = 'Calendar';
            appState.isError = false;

            initializationService
                .initialize()
                .then(function() {

                    var dayInWeek = $routeParams['dayInWeek'];
                    var reload = $routeParams['reload'] == 'reload';

                    (folderService.folders() || [])
                        .filter(function(item) {
                            return item.parentFolderId == null;
                        })
                        .forEach(function(item, index) {
                            $scope.state.folderStyleMap['f' + item.id] = '' + ((index % 5) + 1);
                        });

                    calendarService
                        .loadCalendarWeekIfNeeded(dayInWeek)
                        .then(function() {

                            $scope.state.dayInWeek = calendarService.dayInWeek();
                            $scope.state.currentDay = calendarService.calendarWeek().days[0];

                            $scope.$watch('state.dayInWeek', function(date, oldDate) {
                                if (oldDate == date) return;
                                if (date == null) date = new Date();
                                var newDay = date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate();
                                calendarService.loadCalendarWeekIfNeeded(newDay);
                            });

                            if (reload) $scope.reload();
                        });
                });
        }
    }
});