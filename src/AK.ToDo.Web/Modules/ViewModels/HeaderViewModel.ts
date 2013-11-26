/*******************************************************************************************************************************
 * AK.To|Do.Web.ViewModels.HeaderViewModel
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
/// <reference path='..\Services\ItemService.ts' />
/// <reference path='..\Services\UserNameService.ts' />

module AK.ToDo.Web.ViewModels {

    // View model for the header section.
    //
    export class HeaderViewModel {

        // Public data-bound fields.
        //
        public isInitialized: bool = false;
        public isWaitingForResponse: bool = false;
        public isErrorReceived: bool = false;
        public errorReceived: string = '';
        public itemListTypeList: any = null;
        public userName: string = '';

        // Private fields.
        //
        private selectedItemListType: string = '';

        // Injected dependencies.
        //
        private $resource: any = null;
        
        //-------------------------------------------------------------------------------------------------------------

        constructor($scope: ng.IScope, 
            itemService: Services.ItemService, 
            userNameService: Services.UserNameService,
            eventNames: Common.EventNames) {

            var self = this;

            $scope.$on(eventNames.ItemListTypeChanged,
                function (event: ng.IAngularEvent, itemListType: string): void {
                    self.selectedItemListType = itemListType;
                });

            // Show the "Working..." thingy when someone is about to do something
            // against the server.
            //
            $scope.$on(eventNames.ResponseAwaiting, function (): void {
                self.isWaitingForResponse = true;
            });

            // Hide the "Working..." thingy when someone is done doing something
            // against the server.
            //
            $scope.$on(eventNames.ResponseReceived, function (): void {
                self.isWaitingForResponse = false;
            });

            // Load item list types and also the currently logged in user name.

            userNameService.getUserName().then(function (data: string): void {
                self.userName = data;
            });

            self.isErrorReceived = false;
            var wasWaitingForResponse = self.isWaitingForResponse;
            self.isWaitingForResponse = true;

            itemService.getItemListTypeList().then(function (data: any): void {

                if (!self.isInitialized) self.isInitialized = true;

                self.isWaitingForResponse = wasWaitingForResponse;
                self.itemListTypeList = data;

            }, function (error: string) {

                if (!self.isInitialized) self.isInitialized = true;

                self.isWaitingForResponse = wasWaitingForResponse;
                self.isErrorReceived = true;
                self.errorReceived = error;
            });
        }
        
        //-------------------------------------------------------------------------------------------------------------

        public getTextBasedOnItemListTypeIsActive(
            itemListTypeToCompare: string,
            textIfIsActive: string,
            textOtherwise): string {

            // TODO: Replace this with a directive, perhaps?
            //
            return (this.selectedItemListType == itemListTypeToCompare) ? textIfIsActive : textOtherwise;
        }
    }
}