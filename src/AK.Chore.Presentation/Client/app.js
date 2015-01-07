/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.app.js
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

/* app - Angular app definition and configuration. Logic to load controllers and views on demand taken from solution provided
 *       by Dan Wahlin at http://weblogs.asp.net/dwahlin/dynamically-loading-controllers-and-views-with-angularjs-and-requirejs.
 *
 * @author Aashish Koirala
 */
define(['angular', 'bootstrap', 'ngRoute', 'ngResource', 'uiBootstrap', 'akUiAngular'],
    function(angular) {

        var appState = {
            isWorking: false,
            isError: false,
            currentView: 'Tasks'
        };

        var app = angular.module('app', ['ngRoute', 'ngResource', 'ui.bootstrap', 'ak.ui']);

        app.value('appState', appState);

        config.$inject = ['$routeProvider', '$controllerProvider', '$httpProvider'];
        app.config(config);

        return app;

        function config($routeProvider, $controllerProvider, $httpProvider) {

            app.register = { controller: $controllerProvider.register };

            $routeProvider
                .when('/', resolve('tasks', 'taskList', 'task-list'))
                .when('/tasks/:filterId/:useUnsavedFilter?', resolve('tasks', 'taskList', 'task-list'))
                .when('/task/:taskId?/:returnToPath?', resolve('tasks', 'taskEdit', 'task-edit'))
                .when('/search', resolve('tasks', 'taskSearch', 'task-search'))
                .when('/import', resolve('tasks', 'taskImport', 'task-import'))
                .when('/export', resolve('tasks', 'taskExport', 'task-export'))
                .when('/move/:taskId?', resolve('tasks', 'taskMove', 'task-move'))
                .when('/calendar/:reload?/:dayInWeek?', resolve('calendar', 'calendar', 'calendar'))
                .when('/filters/:filterId?', resolve('filters', 'filters', 'filters'))
                .when('/folders/:folderId?', resolve('folders', 'folders', 'folders'))
                .when('/user', resolve('user', 'userProfile', 'user-profile'))
                .otherwise({ redirectTo: '/' });

            $httpProvider.defaults.headers.common['X-Now'] = new Date().toLocaleString();

            function resolve(folder, controllerName, fileName) {

                return {
                    templateUrl: 'Client/' + folder + '/' + fileName + '.html',
                    controller: controllerName + 'Controller',
                    resolve: {
                        load: ['$q', '$rootScope', function($q, $rootScope) {

                            var deferred = $q.defer();

                            require([folder + '/' + fileName + '.controller'], function() {
                                $rootScope.$apply(deferred.resolve);
                            });

                            return deferred.promise;
                        }]
                    }
                };
            }
        }
    });