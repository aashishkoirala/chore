/*******************************************************************************************************************************
 * AK.To|Do.Web.ViewModels.ItemEditViewModel
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
/// <reference path='..\Services\ItemService.ts' />

module AK.ToDo.Web.ViewModels {

    // View model for the "Edit/Create Item" section.
    //
    export class ItemEditViewModel {

        // Public data-bound fields.
        //
        public isInitialized: bool = false;
        public newItem: DataContracts.ToDoItem = new DataContracts.ToDoItem();
        public editedItem: DataContracts.ToDoItem = null;
        public categoryList: DataContracts.ToDoCategory[] = [];
        public isErrorReceived: bool = false;
        public errorReceived: string = '';
        public isInBulkMode: bool = false;
        public bulkItemOperation: Common.BulkItemOperation = new Common.BulkItemOperation();

        // Private fields.
        //
        private focusedItem: DataContracts.ToDoItem = null;

        // Injected dependencies.
        //
        private $scope: ng.IScope = null;
        private $rootScope: ng.IScope = null;
        private itemService: Services.ItemService = null;
        private eventNames: Common.EventNames = null;

        //-------------------------------------------------------------------------------------------------------------

        constructor($scope: ng.IScope,
            $rootScope: ng.IScope,
            itemService: Services.ItemService,
            eventNames: Common.EventNames) {

            var self = this;

            self.$scope = $scope;
            self.$rootScope = $rootScope;
            self.itemService = itemService;
            self.eventNames = eventNames;

            self.$scope.$on(self.eventNames.CategoryListReceived, function (event: ng.IAngularEvent,
                categoryList: DataContracts.ToDoCategory[]): void {
                self.categoryList = categoryList;

                if (!self.isInitialized) self.isInitialized = true;
            });

            // When an item in the list is selected, create a copy of that for editing so
            // we don't update the list in real time as we make changes.
            //
            self.$scope.$on(self.eventNames.ItemFocused,
                function (event: ng.IAngularEvent, item: DataContracts.ToDoItem): void {
                    self.focusedItem = item;
                    if (self.focusedItem == null) {
                        self.editedItem = null;
                        return;
                    }
                    self.editedItem = self.focusedItem.clone();
                });

            self.$scope.$on(self.eventNames.ItemListReceived, function (): void {
                self.$rootScope.$broadcast(self.eventNames.CategoryListRequested);
            });

            // Whenever items are selected, show or hide the bulk edit UI based on whether
            // more than 1 item is selected.
            //
            self.$scope.$on(self.eventNames.ItemsSelected,
                function (event: ng.IAngularEvent, numberOfItemsSelected: number): void {
                    self.isInBulkMode = numberOfItemsSelected > 1;
                    if (self.isInBulkMode) self.cancelEditing();
                });
        }

        //-------------------------------------------------------------------------------------------------------------

        public validateItem(item: DataContracts.ToDoItem): Common.ValidationResults {
            
            if (item == null) return new Common.ValidationResults(false, "No task selected.");

            if (item.Description == null || item.Description.trim() == '')
                return new Common.ValidationResults(false, "Please enter the task description.");
            
            if (item.ScheduledEndDateAsString == null || item.ScheduledEndDateAsString.trim() == '')
                return new Common.ValidationResults(false, "Please enter the due date.");

            return new Common.ValidationResults(true, "Task ready to be created/saved.");
        }

        //-------------------------------------------------------------------------------------------------------------

        public validateBulkItemOperation(): Common.ValidationResults {
            
            var self = this;

            if (self.bulkItemOperation.IsScheduledEndDateUpdateRequested && 
                (self.bulkItemOperation.ScheduledEndDate == null || self.bulkItemOperation.ScheduledEndDate.trim() == '')) {

                return new Common.ValidationResults(false, "Please enter the due date.");
            }

            return new Common.ValidationResults(true, "Tasks ready to be updated.");
        }

        //-------------------------------------------------------------------------------------------------------------

        public saveNewItem(): void {

            var self = this;

            var validationResults: Common.ValidationResults = self.validateItem(self.newItem);

            if (!validationResults.IsValid) return;

            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);

            var itemToSend: DataContracts.ToDoItem = self.prepareItemForSave(self.newItem);

            self.itemService.createItem(itemToSend)
                .then(function (createdItem: DataContracts.ToDoItem): void {

                    createdItem.clone = DataContracts.ToDoItem.prototype.clone;

                    self.newItem = new DataContracts.ToDoItem();
                    var item = self.prepareItemAfterLoad(createdItem);

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.$rootScope.$broadcast(self.eventNames.NewItemSaved, item);

                }, function (error: string): void {

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.errorReceived = error;
                    self.isErrorReceived = true;
                });
        }

        //-------------------------------------------------------------------------------------------------------------

        public updateItem(): void {

            var self = this;

            var validationResults: Common.ValidationResults = self.validateItem(self.editedItem);

            if (!validationResults.IsValid) return;

            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);

            var itemToSend: DataContracts.ToDoItem = self.prepareItemForSave(self.editedItem);

            self.itemService.updateItem(itemToSend)
                .then(function (): void {

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.editedItem.copyTo(self.focusedItem);
                    self.cancelEditing();

                }, function (error: string): void {

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.errorReceived = error;
                    self.isErrorReceived = true;
                });
        }

        //-------------------------------------------------------------------------------------------------------------

        public cancelEditing(): void {
            var self = this;

            self.editedItem = null;
            self.focusedItem = null;
            self.$rootScope.$broadcast(self.eventNames.ItemFocused, null);
        }

        //-------------------------------------------------------------------------------------------------------------

        public performBulkAction(operationType: string): void {
            var self = this;

            if (!self.validateBulkItemOperation().IsValid) return;

            self.bulkItemOperation.OperationType = operationType;
            self.$rootScope.$broadcast(self.eventNames.BulkItemOperationRequested, self.bulkItemOperation);
        }

        //-------------------------------------------------------------------------------------------------------------

        private prepareItemForSave(originalItem: DataContracts.ToDoItem):
            DataContracts.ToDoItem {

            var item: DataContracts.ToDoItem = originalItem.clone();
            item.CategoryIdList = [];

            if (item.CategoryIdSelectedHash == null || item.CategoryIdSelectedHash == undefined)
                return item;

            for (var key in item.CategoryIdSelectedHash) {
                if (item.CategoryIdSelectedHash[key] == true)
                    item.CategoryIdList.push(key);
            }

            item.CategoryIdSelectedHash = null;
            originalItem.CategoryIdList = item.CategoryIdList;

            return item;
        }

        //-------------------------------------------------------------------------------------------------------------

        private prepareItemAfterLoad(originalItem: DataContracts.ToDoItem):
            DataContracts.ToDoItem {

            var item: DataContracts.ToDoItem = originalItem.clone();

            if (item.CategoryIdList == null || item.CategoryIdList == undefined)
                return item;

            item.CategoryIdList.forEach(function (id: string): void {
                item.CategoryIdSelectedHash[id] = true;
            });

            return item;
        }
    }
}