/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.calendar.calendar.service.js
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

/* calendarService - Angular service that handles calendar retrieval and state maintenance.
 * @author Aashish Koirala
 */
define(['app', 'api.service'], function (app) {

    calendarService.$inject = ['$q', 'apiService'];
    return app.factory('calendarService', calendarService);

    function calendarService($q, apiService) {

        return {
            _dayInWeek: null,
            _calendarWeek: null,

            calendarWeek: function() {
                return this._calendarWeek;
            },

            dayInWeek: function(value) {
                if (value != undefined && this._dayInWeek != value) {
                    this._dayInWeek = value;
                    this._calendarWeek = null;
                }
                return this._dayInWeek;
            },

            loadCalendarWeek: function(dayInWeek) {

                var deferred = $q.defer();
                var self = this;

                if (dayInWeek == null) {
                    var date = new Date();
                    dayInWeek = date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate();
                }

                self._dayInWeek = dayInWeek;

                apiService
                    .getCalendar(dayInWeek)
                    .then(function(response) {

                        self._calendarWeek = response;
                        self._dayInWeek = response.startDate;
                        padWithEmpties(self._calendarWeek);
                        deferred.resolve();

                    }, deferred.reject);

                return deferred.promise;
            },

            loadCalendarWeekIfNeeded: function(dayInWeek) {

                var deferred = $q.defer();
                var self = this;

                self.dayInWeek(dayInWeek);

                if (self._calendarWeek != null) deferred.resolve();
                else self.loadCalendarWeek(dayInWeek).then(deferred.resolve, deferred.reject);

                return deferred.promise;
            }
        };

        function padWithEmpties(calendarWeek) {

            var maxDayItems = 1;
            var i, j, day;
            for (i = 0; i < calendarWeek.days.length; i++) {
                day = calendarWeek.days[i];

                if (day.dayItems.length > maxDayItems) maxDayItems = day.dayItems.length;
            }

            for (i = 0; i < calendarWeek.days.length; i++) {
                day = calendarWeek.days[i];

                var emptiesRequired = maxDayItems - day.dayItems.length;
                if (emptiesRequired == maxDayItems) emptiesRequired++;
                for (j = 0; j < emptiesRequired; j++)
                    day.dayItems.push({ isBlank: true });
            }
        }
    }
});