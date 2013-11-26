/*******************************************************************************************************************************
 * AK.To|Do.Web.ViewModels.ItemListViewModel
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
/// <reference path='..\DataContracts\ToDoItemListRequest.ts' />
/// <reference path='..\Services\ItemService.ts' />

module AK.ToDo.Web.ViewModels {

    // View model for the item list.
    //
    export class ItemListViewModel {

        // Public data-bound fields.
        //
        public isInitialized: bool = false;
        public focusedItem: DataContracts.ToDoItem = null;
        public categoryList: DataContracts.ToDoCategory[] = [];
        public itemListRequest: DataContracts.ToDoItemListRequest = new DataContracts.ToDoItemListRequest();
        public itemStateList: any[] = [];
        public isFilteredItemListEmpty: bool = false;
        public isErrorReceived: bool = false;
        public errorReceived: string = '';

        // Private fields.
        //
        private itemList: DataContracts.ToDoItem[] = [];

        // Injected dependencies.
        //
        private $scope: ng.IScope = null;
        private $rootScope: ng.IScope = null;
        private $timeout: ng.ITimeoutService = null;
        private itemService: Services.ItemService = null;
        private eventNames: Common.EventNames = null;

        //-------------------------------------------------------------------------------------------------------------

        constructor($scope: ng.IScope,
            $rootScope: ng.IScope,
            $routeParams: ng.IRouteParamsService,
            $timeout: ng.ITimeoutService,
            itemService: Services.ItemService,
            eventNames: Common.EventNames) {

            var self = this;

            self.$scope = $scope;
            self.$rootScope = $rootScope;
            self.$timeout = $timeout;
            self.itemService = itemService;
            self.eventNames = eventNames;
            self.itemListRequest.Type = $routeParams['itemListType'];           

            self.$rootScope.$broadcast(self.eventNames.ItemListTypeChanged, self.itemListRequest.Type);

            self.$scope.$on(self.eventNames.CategoryListReceived,
                function (event: ng.IAngularEvent, categoryList: DataContracts.ToDoCategory[]): void {
                    self.categoryList = categoryList;
                });

            self.$scope.$on(self.eventNames.NewItemSaved,
                function (event: ng.IAngularEvent, item: DataContracts.ToDoItem): void {
                    self.itemList.push(item);
                });

            self.$scope.$on(self.eventNames.ItemFocused,
                function (event: ng.IAngularEvent, item: DataContracts.ToDoItem): void {
                    self.focusedItem = item;
                });

            self.$scope.$on(self.eventNames.BulkItemOperationRequested,
                function (event: ng.IAngularEvent, bulkItemOperation: Common.BulkItemOperation): void {
                    self.performBulkAction(bulkItemOperation);
                });

            self.$rootScope.$broadcast(self.eventNames.CategoryListRequested);

            self.isErrorReceived = false;

            // This is to let the bulk edit UI be shown or hidden as the user selects or unselects
            // single or multiple items.
            //
            self.$scope.$watch('vm.getSelectedItemCount()', function (newValue: number) {
                self.$rootScope.$broadcast(self.eventNames.ItemsSelected, newValue);
            });

            // Eww, ugly member names - I know. I got lazy - see the web API for comments.
            //
            self.itemService.getItemStateList().then(function (data: any): void {
                self.itemStateList = data;
                self.itemStateList.unshift({ m_Item1: '', m_Item2: '' });
            });

            if (self.itemListRequest.Type != 'Other') self.loadItemList();
            else if (!self.isInitialized) self.isInitialized = true;
        }

        //-------------------------------------------------------------------------------------------------------------

        public loadItemList(): void {
            var self = this;

            var today: string = Common.formatDate(new Date());
            self.itemListRequest.TodayAsString = today;

            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);
            self.itemService.getItemList(self.itemListRequest)
                .then(function (data: DataContracts.ToDoItem[]): void {

                    self.itemList = [];

                    for (var i: number = 0; i < data.length; i++) {

                        var originalItem: DataContracts.ToDoItem = data[i];
                        originalItem.clone = DataContracts.ToDoItem.prototype.clone;
                        var item = self.prepareItemAfterLoad(originalItem);

                        self.itemList.push(item);
                    }

                    if (!self.isInitialized) self.isInitialized = true;

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.$rootScope.$broadcast(self.eventNames.ItemListReceived);

                }, function (error: string): void {

                    if (!self.isInitialized) self.isInitialized = true;

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.errorReceived = error;
                    self.isErrorReceived = true;
                });
        }

        //-------------------------------------------------------------------------------------------------------------

        public getFilteredItemList(): DataContracts.ToDoItem[] {

            var self = this;

            var selectedCategoryIdList = self.getSelectedCategoryIdList();
            var filteredItemList: DataContracts.ToDoItem[] = [];

            for (var i: number = 0; i < self.itemList.length; i++) {
                for (var j: number = 0; j < selectedCategoryIdList.length; j++) {
                    if (self.itemList[i].CategoryIdList.length == 0) {
                        filteredItemList.push(self.itemList[i]);
                        break;
                    }
                    if (self.itemList[i].CategoryIdList.indexOf(selectedCategoryIdList[j]) >= 0) {
                        filteredItemList.push(self.itemList[i]);
                        break;
                    }
                }
            }

            self.isFilteredItemListEmpty = filteredItemList.length == 0;
            return filteredItemList;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getSelectedItemCount(): number {
            
            var self = this;
            var filteredItemList: DataContracts.ToDoItem[] = self.getFilteredItemList();

            var count: number = 0;
            filteredItemList.forEach(function (value: DataContracts.ToDoItem): void {
                if (value.IsSelected) count++;
            });

            return count;
        }

        //-------------------------------------------------------------------------------------------------------------

        public focusItem(item: DataContracts.ToDoItem): void {
            var self = this;

            self.focusedItem = item;
            self.$rootScope.$broadcast(self.eventNames.ItemFocused, item);
        }

        //-------------------------------------------------------------------------------------------------------------

        public startItem(item: DataContracts.ToDoItem): void {

            var self = this;

            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);
            self.isErrorReceived = false;

            self.itemService.startItem(item.Id)
                .then(function (): void {

                    self.refreshItem(item);

                }, function (error: string): void {

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.errorReceived = error;
                    self.isErrorReceived = true;
                });
        }

        //-------------------------------------------------------------------------------------------------------------

        public pauseItem(item: DataContracts.ToDoItem): void {

            var self = this;

            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);
            self.isErrorReceived = false;

            self.itemService.pauseItem(item.Id)
                .then(function (): void {

                    self.refreshItem(item);

                }, function (error: string): void {

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.errorReceived = error;
                    self.isErrorReceived = true;
                });
        }

        //-------------------------------------------------------------------------------------------------------------

        public completeItem(item: DataContracts.ToDoItem): void {

            var self = this;

            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);
            self.isErrorReceived = false;

            self.itemService.completeItem(item.Id)
                .then(function (): void {

                    self.refreshItem(item, function (): void {
                        self.$timeout(function (): void {
                            var index: number = self.itemList.indexOf(item);
                            if (index > -1) self.itemList.splice(index, 1);
                        }, 2000);
                    });

                }, function (error: string): void {

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.errorReceived = error;
                    self.isErrorReceived = true;
                });
        }

        //-------------------------------------------------------------------------------------------------------------

        public deleteItem(item: DataContracts.ToDoItem): void {

            var self = this;

            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);
            self.isErrorReceived = false;

            self.itemService.deleteItem(item.Id)
                .then(function (): void {

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);

                    var index: number = self.itemList.indexOf(item);
                    if (index > -1) self.itemList.splice(index, 1);

                }, function (error: string): void {

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.errorReceived = error;
                    self.isErrorReceived = true;
                });
        }

        //-------------------------------------------------------------------------------------------------------------

        private refreshItem(item: DataContracts.ToDoItem, afterRefresh: () => void = null): void {
            var self = this;

            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);
            self.itemService.getItem(item.Id)
                .then(function (data: DataContracts.ToDoItem): void {

                    data.copyTo = DataContracts.ToDoItem.prototype.copyTo;
                    data.copyTo(item);

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);

                    if (afterRefresh != null) afterRefresh();

                }, function (error: string): void {

                    self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                    self.errorReceived = error;
                    self.isErrorReceived = true;
                });
        }

        //-------------------------------------------------------------------------------------------------------------

        private getSelectedCategoryIdList(): string[] {
            var self = this;
            var s: string[] = [];

            for (var i: number = 0; i < self.categoryList.length; i++) {
                if (self.categoryList[i].IsSelected) s.push(self.categoryList[i].Id);
            }

            return s;
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

        //-------------------------------------------------------------------------------------------------------------

        private performBulkAction(bulkItemOperation: Common.BulkItemOperation): void {
            var self = this;

            var filteredItemList: DataContracts.ToDoItem[] = self.getFilteredItemList();
            var targetItemList: DataContracts.ToDoItem[] = [];

            filteredItemList.forEach(function (item: DataContracts.ToDoItem): void {
                if (item.IsSelected) targetItemList.push(item);
            });

            if (targetItemList.length == 0) return;

            if (bulkItemOperation.OperationType == 'Update') {
                self.performBulkUpdate(targetItemList, bulkItemOperation);
                return;
            }

            var idList: string[] = targetItemList.map(function (value: DataContracts.ToDoItem): string {
                return value.Id;
            });

            var promise: ng.IPromise = null;
            switch (bulkItemOperation.OperationType) {
                case 'Start': promise = self.itemService.startItemList(idList);
                    break;
                case 'Pause': promise = self.itemService.pauseItemList(idList);
                    break;
                case 'Complete': promise = self.itemService.completeItemList(idList);
                    break;
                case 'Delete': promise = self.itemService.deleteItemList(idList);
                    break;
            }

            if (promise == null) return;

            promise.then(function (): void {

                self.loadItemList();

            }, function (error: string): void {

                self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                self.errorReceived = error;
                self.isErrorReceived = true;

            });
        }

        //-------------------------------------------------------------------------------------------------------------

        private performBulkUpdate(itemList: DataContracts.ToDoItem[], 
            bulkItemOperation: Common.BulkItemOperation): void {

            var self = this;

            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);

            var targetItemList: DataContracts.ToDoItem[] = [];
            itemList.forEach(function (item: DataContracts.ToDoItem): void {
                
                var targetItem: DataContracts.ToDoItem = item.clone();

                if (bulkItemOperation.IsScheduledStartDateUpdateRequested)
                    targetItem.ScheduledStartDateAsString = bulkItemOperation.ScheduledStartDate;

                if (bulkItemOperation.IsScheduledEndDateUpdateRequested)
                    targetItem.ScheduledEndDateAsString = bulkItemOperation.ScheduledEndDate;

                if (bulkItemOperation.IsCategoryListUpdateRequested) {
                    targetItem.CategoryIdList = [];
                    for (var key in bulkItemOperation.CategoryIdSelectedHash) {
                        if (bulkItemOperation.CategoryIdSelectedHash[key] == true)
                            targetItem.CategoryIdList.push(key);
                    }
                    targetItem.CategoryIdSelectedHash = null;
                }

                targetItemList.push(targetItem);
            });

            self.itemService.updateItemList(targetItemList).then(function (): void {

                self.loadItemList();

            }, function (error: string): void {
                self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                self.errorReceived = error;
                self.isErrorReceived = true;
            });
        }
    }
}