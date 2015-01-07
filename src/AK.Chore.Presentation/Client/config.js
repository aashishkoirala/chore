/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.config.js
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

/* config - RequireJS configuration.
 * @author Aashish Koirala
 */
require.config({
    paths: {
        jquery: '../lib/js/jquery?',
        bootstrap: '../lib/js/bootstrap?',
        angular: '../lib/js/angular?',
        ngRoute: '../lib/js/ng-route?',
        ngResource: '../lib/js/ng-resource?',
        uiBootstrap: '../lib/js/ui-bootstrap?',
        akUiAngular: '../lib/js/ak-ui-angular?',
        signalR: '../lib/js/signalr?',
        signalRHubs: '../signalr/hubs?'
    },
    shim: {
        jquery: { exports: 'jquery' },
        bootstrap: { deps: ['jquery'], exports: 'bootstrap' },
        angular: { deps: ['jquery'], exports: 'angular' },
        ngRoute: { deps: ['angular'], exports: 'ngRoute' },
        ngResource: { deps: ['angular'], exports: 'ngResource' },
        uiBootstrap: { deps: ['angular', 'bootstrap'], exports: 'uiBootstrap' },
        akUiAngular: { deps: ['angular'], exports: 'akUiAngular' },
        signalR: { deps: ['jquery'], exports: 'signalR' },
        signalRHubs: { deps: ['signalR'], exports: 'signalRHubs' }
    },
    deps: ['main']    
});