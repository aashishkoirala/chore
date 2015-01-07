/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Presentation.user.service.tests.js
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

/* Tests for userService.
 * @author Aashish Koirala
 */
define(['app', 'ngMock', 'api.service', 'user.service'], function (app) {

    app.value('links', []);

    describe('userService', function() {

        beforeEach(module('app'));

        it('loads user nickname and makes it available via userNickname() when loadUserNickname() is called', function(done) {
            inject(function($q, $rootScope, apiService, userService) {

                apiService.getUserNickname = function(id) {
                    console.log(id);
                    var deferred = $q.defer();
                    deferred.resolve({ nickname: 'TestName' });
                    return deferred.promise;
                };

                userService.loadUserNickname().then(function() {
                    expect(userService.userNickname()).toBe('TestName');
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls getUserData() on the API when downloadUserData() is called', function(done) {
            inject(function($q, $rootScope, apiService, userService) {

                apiService.getUserData = function() {
                    var deferred = $q.defer();
                    deferred.resolve({});
                    return deferred.promise;
                };

                spyOn(apiService, 'getUserData').and.callThrough();

                userService.downloadUserData().then(function() {
                    expect(apiService.getUserData).toHaveBeenCalled();
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls updateUserData() on the API when uploadUserData() is called', function(done) {
            inject(function($q, $rootScope, apiService, userService) {

                var testUserData = {};

                apiService.updateUserData = function(userData) {
                    console.log(userData);
                    var deferred = $q.defer();
                    deferred.resolve({});
                    return deferred.promise;
                };

                spyOn(apiService, 'updateUserData').and.callThrough();

                userService.uploadUserData(testUserData).then(function() {
                    expect(apiService.updateUserData).toHaveBeenCalledWith(testUserData);
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls deleteUserProfile() on the API when deleteUserProfile() is called', function(done) {
            inject(function($q, $rootScope, apiService, userService) {

                apiService.deleteUserProfile = function(id) {
                    console.log(id);
                    var deferred = $q.defer();
                    deferred.resolve({});
                    return deferred.promise;
                };

                spyOn(apiService, 'deleteUserProfile').and.callThrough();

                userService.deleteUserProfile().then(function() {
                    expect(apiService.deleteUserProfile).toHaveBeenCalledWith(0);
                    done();
                });

                $rootScope.$apply();
            });
        });
    });
});