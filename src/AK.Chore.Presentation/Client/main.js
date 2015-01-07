/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.main.js
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

/* main - Main entry point that bootstraps the application.
 * @author Aashish Koirala
 */
define(['jquery',
        'angular',
        'app',
        'signalRHubs',
        'banner/banner.directive',
        'tasks/filter-picker.directive',
        'tasks/folder-picker.directive',
        'criterion-editor/criterion-editor.directive',
        'user/user.service',
        'calendar/calendar.service',
        'tasks/initialization.service',
        'banner/notification.controller'],
    function($, angular, app) {

        $.get('api/link', null, function(links) {

            app.value('links', links);

            $(document).ready(function() {
                angular.bootstrap(document, ['app']);
            });
        });
    });