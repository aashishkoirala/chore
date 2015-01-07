/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Presentation.calendar.service.tests.js
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

/* Tests for calendarService.
 * @author Aashish Koirala
 */
define(['app', 'ngMock', 'api.service', 'calendar.service'], function(app) {

    app.value('links', []);

    describe('calendarService', function() {

        beforeEach(module('app'));

        it('calls getCalendar() on the API and assigns dayInWeek and calendarWeek when loadCalendarWeek is called', function(done) {
            inject(function($q, $rootScope, apiService, calendarService) {

                var testDayInWeek = '2015-01-06';
                var testWeek = { days: [], startDate: testDayInWeek };

                apiService.getCalendar = function(dayInWeek) {
                    console.log(dayInWeek);
                    var deferred = $q.defer();
                    deferred.resolve(testWeek);
                    return deferred.promise;
                };

                spyOn(apiService, 'getCalendar').and.callThrough();

                calendarService.loadCalendarWeek(testDayInWeek).then(function() {
                    expect(calendarService.dayInWeek()).toBe(testDayInWeek);
                    expect(calendarService.calendarWeek()).toBe(testWeek);

                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls loadCalendarWeek() only if calendarWeek is not already loaded, but sets dayInWeek anyway when loadCalendarWeekIfNeeded() is called', function(done) {
            inject(function($q, $rootScope, calendarService) {

                var testDayInWeek = '2015-01-06';

                spyOn(calendarService, 'loadCalendarWeek').and.callFake(function(dayInWeek) {
                    console.log(dayInWeek);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });

                spyOn(calendarService, 'dayInWeek').and.callFake(function(dayInWeek) {
                    console.log(dayInWeek);
                    calendarService._calendarWeek = {};
                });

                calendarService.loadCalendarWeekIfNeeded(testDayInWeek).then(function() {
                    expect(calendarService.dayInWeek).toHaveBeenCalledWith(testDayInWeek);
                    expect(calendarService.loadCalendarWeek).not.toHaveBeenCalled();

                    done();
                });

                $rootScope.$apply();
            });
        });
    });
});