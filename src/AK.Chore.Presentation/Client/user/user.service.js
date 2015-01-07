/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.user.user.service.js
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

/* userService - Angular service that talks to the web API for user management.
 * @author Aashish Koirala
 */
define(['app', 'api.service'], function (app) {
    var userService = function ($q, apiService) {
        
        return {
            _userNickname: null,
            
            userNickname: function() {
                return this._userNickname;
            },
            
            loadUserNickname: function () {
                
                var deferred = $q.defer();
                var self = this;

                if (self._userNickname != null) {
                    deferred.resolve();
                    return deferred.promise;
                }

                apiService
                    .getUserNickname(1)
                    .then(function(response) {
                        self._userNickname = response.nickname;
                        deferred.resolve();
                    });

                return deferred.promise;
            },
            
            updateUserNickname: function (nickname) {

                var self = this;

                apiService
                    .updateUserNickname({ nickname: nickname })
                    .then(function() {
                        self._userNickname = nickname;
                    });
            },
            
            downloadUserData: function() {
                return apiService.getUserData();
            },
            
            uploadUserData: function(userData) {
                return apiService.updateUserData(userData);
            },
            
            deleteUserProfile: function() {
                return apiService.deleteUserProfile(0);
            }
        };
    };

    userService.$inject = ['$q', 'apiService'];
    return app.factory('userService', userService);
});