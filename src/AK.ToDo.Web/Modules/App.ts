/*******************************************************************************************************************************
 * AK.To|Do.Web.App
 * Copyright © 2013 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of To Do.
 *  
 * To Do is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * To Do is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with To Do.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

/// <reference path='Common.ts' />
/// <reference path='ViewModels\CategoriesViewModel.ts' />
/// <reference path='ViewModels\ItemEditViewModel.ts' />
/// <reference path='ViewModels\ItemListViewModel.ts' />
/// <reference path='ViewModels\ItemImportViewModel.ts' />
/// <reference path='ViewModels\HeaderViewModel.ts' />

//---------------------------------------------------------------------------------------------------------------------

// All my controller scopes have just a single "vm" property which is the view-model with
// all the model data and logic. Annotating them as this interface helps with the IntelliSense.
//
interface IViewModelScope extends ng.IScope {
    vm: any;
}

//---------------------------------------------------------------------------------------------------------------------

var services: ng.IModule = angular.module('toDoServices', ['ngResource']);

//---------------------------------------------------------------------------------------------------------------------

services.constant('resourceApiRoot', '../../api/');
services.constant('templatesDir', '../../Modules/Templates/');
services.constant('eventNames', new AK.ToDo.Web.Common.EventNames());

//---------------------------------------------------------------------------------------------------------------------

services.factory('categoryService', function (
    $resource: any, 
    $q: ng.IQService, 
    resourceApiRoot: string): AK.ToDo.Web.Services.CategoryService {

    return new AK.ToDo.Web.Services.CategoryService($resource, $q, resourceApiRoot);
});

//---------------------------------------------------------------------------------------------------------------------

services.factory('itemService', function (
    $resource: any, 
    $q: ng.IQService, 
    resourceApiRoot: string): AK.ToDo.Web.Services.ItemService {

    return new AK.ToDo.Web.Services.ItemService($resource, $q, resourceApiRoot);
});

//---------------------------------------------------------------------------------------------------------------------

services.factory('userNameService', function (
    $resource: any, 
    $q: ng.IQService, 
    resourceApiRoot: string): AK.ToDo.Web.Services.UserNameService {

    return new AK.ToDo.Web.Services.UserNameService($resource, $q, resourceApiRoot);
});

//---------------------------------------------------------------------------------------------------------------------

var app: ng.IModule = angular.module('toDoApp', ['toDoServices', 'ngRoute']);

//---------------------------------------------------------------------------------------------------------------------

app.controller('categoriesCtrl', [
    '$scope', '$rootScope', 'categoryService', 'eventNames',

    function ($scope: IViewModelScope,
        $rootScope: ng.IScope,
        categoryService: AK.ToDo.Web.Services.CategoryService,
        eventNames: AK.ToDo.Web.Common.EventNames): void {

        $scope.vm = new AK.ToDo.Web.ViewModels.CategoriesViewModel($scope, $rootScope, categoryService, eventNames);
    }]);

//---------------------------------------------------------------------------------------------------------------------

app.controller('itemEditCtrl', [
    '$scope', '$rootScope', 'itemService', 'eventNames',

    function ($scope: IViewModelScope,
        $rootScope: ng.IScope,
        itemService: AK.ToDo.Web.Services.ItemService,
        eventNames: AK.ToDo.Web.Common.EventNames): void {

        $scope.vm = new AK.ToDo.Web.ViewModels.ItemEditViewModel($scope, $rootScope, itemService, eventNames);
    }]);

//---------------------------------------------------------------------------------------------------------------------

app.controller('headerCtrl', [
    '$scope', 'itemService', 'userNameService', 'eventNames',

    function ($scope: IViewModelScope,
        itemService: AK.ToDo.Web.Services.ItemService,
        userNameService: AK.ToDo.Web.Services.UserNameService,
        eventNames: AK.ToDo.Web.Common.EventNames): void {

        $scope.vm = new AK.ToDo.Web.ViewModels.HeaderViewModel($scope, itemService, userNameService, eventNames);
    }]);

//---------------------------------------------------------------------------------------------------------------------

app.config(function ($routeProvider: ng.IRouteProviderProvider, templatesDir: string): void {

    $routeProvider.when('/Items/:itemListType', {
        templateUrl: templatesDir + 'ItemList.html',

        controller: function ($scope: IViewModelScope,
            $rootScope: ng.IScope,
            $routeParams: ng.IRouteParamsService,
            $timeout: ng.ITimeoutService,
            itemService: AK.ToDo.Web.Services.ItemService,
            eventNames: AK.ToDo.Web.Common.EventNames): void {

            $scope.vm = new AK.ToDo.Web.ViewModels.ItemListViewModel($scope,
                $rootScope, $routeParams, $timeout, itemService, eventNames);
        },

        reloadOnSearch: true

    }).when('/Import', {
        templateUrl: templatesDir + 'ItemImport.html',

        controller: function ($scope: IViewModelScope,
            $rootScope: ng.IScope,
            $timeout: ng.ITimeoutService,
            itemService: AK.ToDo.Web.Services.ItemService,
            eventNames: AK.ToDo.Web.Common.EventNames): void {

            $scope.vm = new AK.ToDo.Web.ViewModels.ItemImportViewModel(
                $scope, $rootScope, $timeout, itemService, eventNames);
        }

    }).otherwise({
        redirectTo: '/Items/Today'

    });
});