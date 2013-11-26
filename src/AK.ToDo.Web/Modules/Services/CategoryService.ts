/*******************************************************************************************************************************
 * AK.To|Do.Web.Services.CategoryService
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
/// <reference path='..\DataContracts\ToDoCategory.ts' />

module AK.ToDo.Web.Services {

    // Operations to retrieve, store or delete To-Do item categories.
    //
    export class CategoryService {

        private categoryResource: Common.IResource = null;
        private $q: ng.IQService = null;

        //-------------------------------------------------------------------------------------------------------------

        constructor($resource: any, $q: ng.IQService, resourceApiRoot: string) {
            
            var self = this;

            self.$q = $q;
            self.categoryResource = $resource(resourceApiRoot + 'category/:id', { id: '@id' }, { 
                update: { method: 'PUT' } 
            });
        }

        //-------------------------------------------------------------------------------------------------------------

        public getCategory(id: string): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            var category: DataContracts.ToDoCategory = self.categoryResource.get({ id: id },
                function (): void {
                    deferred.resolve(category);
                }, function (response: Common.WebApiErrorResponse): void {
                    deferred.reject(response.data.Message);
                });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getCategoryList(): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            var categoryList: DataContracts.ToDoCategory[] = self.categoryResource.query(null,
                function (): void {
                    deferred.resolve(categoryList);
                }, function (response: Common.WebApiErrorResponse): void {
                    deferred.reject(response.data.Message);
                });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public createCategory(category: DataContracts.ToDoCategory): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            var createdCategory: DataContracts.ToDoCategory = self.categoryResource.save(category,
                function (): void {
                    deferred.resolve(createdCategory);
                }, function (response: Common.WebApiErrorResponse): void {
                    deferred.reject(response.data.Message);
                });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public updateCategory(category: DataContracts.ToDoCategory): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            self.categoryResource.update(category, function (): void {
                deferred.resolve();
            }, function (response: any): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }

        //-------------------------------------------------------------------------------------------------------------

        public deleteCategory(id: string): ng.IPromise {

            var self = this;
            var deferred: ng.IDeferred = self.$q.defer();

            self.categoryResource.delete ({ id: id }, function (): void {
                deferred.resolve();
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }
    }
}