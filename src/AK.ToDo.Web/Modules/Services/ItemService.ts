/*******************************************************************************************************************************
 * AK.To|Do.Web.Services.ItemService
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

/// <reference path='..\Common.ts' />
/// <reference path='..\DataContracts\ToDoItem.ts' />
/// <reference path='..\DataContracts\ToDoItemListRequest.ts' />

module AK.ToDo.Web.Services {

    // Operations around items. Handles communication with all REST APIs built around items.
    //
    // TODO: Decompose further so it is symmetrical with the actual services in the backend.
    //
    export class ItemService {

        private itemResource: Common.IResource = null;
        private itemsResource: Common.IResource = null;
        private itemListTypeResource: Common.IResource = null;
        private itemStateResource: Common.IResource = null;
        private startItemCommandResource: Common.IResource = null;
        private pauseItemCommandResource: Common.IResource = null;
        private completeItemCommandResource: Common.IResource = null;
        private startItemsCommandResource: Common.IResource = null;
        private pauseItemsCommandResource: Common.IResource = null;
        private completeItemsCommandResource: Common.IResource = null;
        private itemImportResource: Common.IResource = null;

        private $q: ng.IQService = null;

        //-------------------------------------------------------------------------------------------------------------

        constructor($resource: any, $q: ng.IQService, resourceApiRoot: string) {

            var self = this;

            self.$q = $q;

            self.itemResource = $resource(resourceApiRoot + 'item/:id', { 
                id: '@id' 
            }, { 
                update: { 
                    method: 'PUT' 
                } 
            });

            self.itemsResource = $resource(resourceApiRoot + 'items/', { 
                state: '@state', 
                scheduledStart: '@scheduledStart', 
                scheduledEnd: '@scheduledEnd', 
                actualStart: '@actualStart',
                actualEnd: '@actualEnd',
                categoryIdList: '@categoryIdList' 
            }, { 
                update: { 
                    method: 'PUT', 
                    isArray: true 
                } 
            });

            self.itemListTypeResource = $resource(resourceApiRoot + 'itemlisttype/', null);
            self.itemStateResource = $resource(resourceApiRoot + 'itemstate/', null);

            self.startItemCommandResource = $resource(resourceApiRoot + 'command/item/start/:id', { 
                id: '@id' 
            }, { 
                execute: { 
                    method: 'PUT' 
                } 
            });

            self.pauseItemCommandResource = $resource(resourceApiRoot + 'command/item/pause/:id', { 
                id: '@id' 
            }, { 
                execute: { 
                    method: 'PUT' 
                } 
            });

            self.completeItemCommandResource = $resource(resourceApiRoot + 'command/item/complete/:id', { 
                id: '@id' 
            }, { 
                execute: { 
                    method: 'PUT' 
                } 
            });

            self.startItemsCommandResource = $resource(resourceApiRoot + 'command/items/start', null, {
                execute: { 
                    method: 'PUT', 
                    isArray: true 
                } 
            });

            self.pauseItemsCommandResource = $resource(resourceApiRoot + 'command/items/pause', null, { 
                execute: { 
                    method: 'PUT', 
                    isArray: true 
                } 
            });

            self.completeItemsCommandResource = $resource(resourceApiRoot + 'command/items/complete', null, { 
                execute: { 
                    method: 'PUT', 
                    isArray: true 
                } 
            });

            self.itemImportResource = $resource(resourceApiRoot + 'itemimport/', null, { 
                save: { 
                    method: 'POST', 
                    isArray: true 
                } 
            });
        }

        //-------------------------------------------------------------------------------------------------------------

        public getItemStateList(): ng.IPromise {

            var self = this;
            var deferred = self.$q.defer();

            var itemStateList: any = self.itemStateResource.query(null, function (): void {
                deferred.resolve(itemStateList);
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getItemListTypeList(): ng.IPromise {

            var self = this;
            var deferred = self.$q.defer();

            var itemListTypeList: any = self.itemListTypeResource.query(null, function (): void {
                deferred.resolve(itemListTypeList);
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getItem(id: string): ng.IPromise {

            var self = this;
            var deferred = self.$q.defer();

            var item: DataContracts.ToDoItem = self.itemResource.get({ id: id }, function (): void {
                deferred.resolve(item);
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getItemList(request: DataContracts.ToDoItemListRequest): ng.IPromise {
            
            var self = this;
            var deferred = self.$q.defer();

                var itemList: DataContracts.ToDoItem[] = self.itemsResource.query(request,
                function (): void {
                    deferred.resolve(itemList);
                }, function (response: Common.WebApiErrorResponse): void {
                    deferred.reject(response.data.Message);
                });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public createItem(item: DataContracts.ToDoItem): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            var createdItem: DataContracts.ToDoItem = self.itemResource.save(item, function (): void {
                deferred.resolve(createdItem);
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public updateItem(item: DataContracts.ToDoItem): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            self.itemResource.update(item, function (): void {
                deferred.resolve();
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public updateItemList(itemList: DataContracts.ToDoItem[]): ng.IPromise {

            var self = this;
            var deferred = self.$q.defer();

            self.itemsResource.update(itemList, function (): void {
                deferred.resolve();
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public deleteItem(id: string): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            self.itemResource.delete({ id: id }, function (): void {
                deferred.resolve();
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public deleteItemList(idList: string[]): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            var idListParam: string = idList.join('|');

            self.itemsResource.delete({ idList: idListParam }, function (): void {
                deferred.resolve();
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public startItem(id: string): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            self.startItemCommandResource.execute({ id: id }, function (): void {
                deferred.resolve();
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public startItemList(idList: string[]): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            self.startItemsCommandResource.execute({ IdList: idList }, function (): void {
                deferred.resolve();
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public pauseItem(id: string): ng.IPromise {
            
            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            self.pauseItemCommandResource.execute({ id: id }, function (): void {
                deferred.resolve();
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public pauseItemList(idList: string[]): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            self.pauseItemsCommandResource.execute({ IdList: idList }, function (): void {
                deferred.resolve();
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public completeItem(id: string): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            self.completeItemCommandResource.execute({ id: id }, function (): void {
                deferred.resolve();
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public completeItemList(idList: string[]): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            self.completeItemsCommandResource.execute({ IdList: idList }, function (): void {
                deferred.resolve();
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public importItems(importData: string): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            self.itemImportResource.save({ importData: importData }, function (): void {
                deferred.resolve();
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }
    }
}