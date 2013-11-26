/*******************************************************************************************************************************
 * AK.To|Do.Web.ViewModels.ItemImportViewModel
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

    // View model for the "Import" section.
    //
    export class ItemImportViewModel {

        // Public data-bound fields.
        //
        public importData: string = null;
        public isErrorReceived: bool = false;
        public errorReceived: string = '';
        public isSuccess: bool = false;

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
            $timeout: ng.ITimeoutService,
            itemService: Services.ItemService,
            eventNames: Common.EventNames) {

            var self = this;

            self.$scope = $scope;
            self.$rootScope = $rootScope;
            self.$timeout = $timeout;
            self.itemService = itemService;
            self.eventNames = eventNames;
        }

        //-------------------------------------------------------------------------------------------------------------

        public importItems(): void {
            
            var self = this;
            self.isErrorReceived = false;
            self.isSuccess = false;
            self.$rootScope.$broadcast(self.eventNames.ResponseAwaiting);

            self.itemService.importItems(self.importData).then(function (): void {

                self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                self.isSuccess = true;
                self.importData = '';

                self.$timeout(function (): void {
                    self.isSuccess = false;
                    location.href = '#Items/Today';
                }, 2000);

            }, function (error: string): void {

                self.$rootScope.$broadcast(self.eventNames.ResponseReceived);
                self.isErrorReceived = true;
                self.errorReceived = error;
            });
        }
    }
}