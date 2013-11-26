/*******************************************************************************************************************************
 * AK.To|Do.Web.ViewModels.CategoriesViewModel
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
/// <reference path='..\Services\CategoryService.ts' />

module AK.ToDo.Web.ViewModels {

    // View model that supports the "Categories" section.
    //
    export class CategoriesViewModel {

        // Public data-bound fields.
        //
        public isInitialized: bool = false;
        public categoryList: DataContracts.ToDoCategory[] = [];
        public newCategory: DataContracts.ToDoCategory = new DataContracts.ToDoCategory();
        public editedCategory: DataContracts.ToDoCategory = null;
        public isErrorReceived: bool = false;
        public errorReceived: string = '';

        // Injected dependencies.
        //
        private $scope: ng.IScope = null;
        private $rootScope: ng.IScope = null;
        private categoryService: Services.CategoryService = null;
        private eventNames: Common.EventNames = null;
        
        //-------------------------------------------------------------------------------------------------------------

        constructor($scope: ng.IScope,
            $rootScope: ng.IScope,
            categoryService: Services.CategoryService,
            eventNames: Common.EventNames) {

            var self = this;

            self.$scope = $scope;
            self.$rootScope = $rootScope;
            self.categoryService = categoryService;
            self.eventNames = eventNames;

            // When any other controller requests the list of categories, load list of categories from server if needed
            // and then broadcast it out.
            //
            self.$scope.$on(self.eventNames.CategoryListRequested, function (event: ng.IAngularEvent): void {
                if (self.categoryList.length == 0) self.loadCategoryList();
                else self.$rootScope.$broadcast(self.eventNames.CategoryListReceived, self.categoryList);
            });
        }
        
        //-------------------------------------------------------------------------------------------------------------

        public createCategory(): void {
            var self = this;

            self.isErrorReceived = false;
            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);

            self.categoryService.createCategory(self.newCategory)
                .then(function (): void {
                    self.loadCategoryList();

                    self.newCategory = new DataContracts.ToDoCategory();
                    self.newCategory.Name = '';

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);

                }, function (error: string): void {

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.errorReceived = error;
                    self.isErrorReceived = true;
                });
        }
        
        //-------------------------------------------------------------------------------------------------------------

        public updateCategory(): void {
            var self = this;

            self.isErrorReceived = false;
            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);

            self.categoryService.updateCategory(self.editedCategory)
                .then(function (): void {

                    self.loadCategoryList();
                    self.editedCategory = null;

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);

                }, function (error: string): void {

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.errorReceived = error;
                    self.isErrorReceived = true;
                });
        }
        
        //-------------------------------------------------------------------------------------------------------------

        public deleteCategory(id: string): void {
            var self = this;

            self.isErrorReceived = false;
            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);

            self.categoryService.deleteCategory(id)
                .then(function (): void {

                    self.loadCategoryList();
                    self.editedCategory = null;

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);

                }, function (error: string): void {

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.errorReceived = error;
                    self.isErrorReceived = true;
                });
        }
        
        //-------------------------------------------------------------------------------------------------------------

        private loadCategoryList(): void {
            var self = this;

            self.isErrorReceived = false;
            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);

            self.categoryService.getCategoryList()
                .then(function (data: DataContracts.ToDoCategory[]): void {

                    self.categoryList = [];

                    data.forEach(function (receivedCategory: DataContracts.ToDoCategory): void {

                        var category: DataContracts.ToDoCategory = new DataContracts.ToDoCategory();

                        category.Id = receivedCategory.Id;
                        category.ParentId = receivedCategory.ParentId;
                        category.Name = receivedCategory.Name;
                        category.Level = receivedCategory.Level;
                        category.IsSelected = true;

                        self.categoryList.push(category);
                    });

                    if (!self.isInitialized) self.isInitialized = true;

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.$rootScope.$broadcast(self.eventNames.CategoryListReceived, self.categoryList);

                }, function (error: string): void {

                    if (!self.isInitialized) self.isInitialized = true;

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.errorReceived = error;
                    self.isErrorReceived = true;
                });
        }
    }
}