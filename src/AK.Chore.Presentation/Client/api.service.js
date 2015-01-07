/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.api.service.js
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

/* apiService - Angular service that talks to the web API based on link information given.
 * @author Aashish Koirala
 */
define(['app'], function(app) {

    apiService.$inject = ['$q', '$resource', '$http', 'links', 'appState'];
    return app.factory('apiService', apiService);

    function apiService($q, $resource, $http, links, appState) {

        var errorMessages = {
            FilterDoesNotExist: 'That filter does not exist.',
            FilterAlreadyExists: 'A filter by that name already exists.',
            CannotDeleteOnlyFilter: 'You have only one filter left - you cannot delete that.',
            FolderDoesNotExist: 'That folder does not exist.',
            FolderAlreadyExists: 'A folder by that name already exists.',
            CannotMoveToChildFolder: 'You cannot move a folder to one of its children.',
            CannotDeleteOnlyRootFolder: 'You have only one root folder left - you cannot delete that.',
            TaskDoesNotExist: 'That task does not exist.',
            TaskAlreadyExists: 'A task with that exact description already exists in the same folder.',
            TaskCouldNotBeMapped: 'I think your task has certain invalid combination of stuff- we cannot let it through. Sorry.'
        };
        
        var service = {};

        links.forEach(function(linkItem) {

            service[linkItem.rel] = (function(link) {
                return function(request) {

                    appState.isError = false;
                    appState.isWorking = true;

                    var deferred = $q.defer();

                    var resource = $resource(link.href, null, {
                        save: { method: 'POST', isArray: link.isArray },
                        update: { method: 'PUT', isArray: link.isArray }
                    });

                    var mapping = null;
                    var tail = link.href.substr(link.href.length - 3, 3);
                    if (tail == ':id') mapping = { id: request };

                    switch (link.method) {
                    case 'GET':
                        if (link.isArray) resource.query(request, success, error);
                        else resource.get(mapping, success, error);
                        break;
                    case 'POST':
                        resource.save(request, success, error);
                        break;
                    case 'PUT':
                        resource.update(request, success, error);
                        break;
                    case 'DELETE':
                        if (link.isArray) {
                            $http({
                                url: link.href,
                                method: 'DELETE',
                                data: request
                            }).then(success, error);
                        } else resource.delete(mapping, success, error);
                        break;
                    }

                    return deferred.promise;

                    function success(response) {
                        appState.isWorking = false;
                        appState.isError = false;
                        deferred.resolve(response);
                    }

                    function error(response) {

                        var errorCode = response.headers()["x-errorcode"];
                        var errorMessage = null;
                        if (errorCode != undefined) errorMessage = errorMessages[errorCode];
                        if (errorMessage == undefined) errorMessage = null;

                        appState.isWorking = false;
                        appState.errorMessage = errorMessage;
                        appState.isError = true;

                        deferred.reject();
                    }
                };
            })(linkItem);
        });

        return service;
    }
});