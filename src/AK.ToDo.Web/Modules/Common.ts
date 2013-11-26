/*******************************************************************************************************************************
 * AK.To|Do.Web.Common
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

/// <reference path='..\Scripts\angular-1.0.d.ts' />

module AK.ToDo.Web.Common {

    // Helper structure used for UI validation.
    //
    export class ValidationResults {
        public IsValid: bool = false;
        public Message: string = '';

        constructor(isValid: bool, message: string) {
            this.IsValid = isValid;
            this.Message = message;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    // A way to strong name all events that are passed around within the Angular controllers.
    // An instance of this is injected as a constant into controllers that need it.
    //
    export class EventNames {
        public CategoryListRequested: string = 'CategoryListRequested';
        public CategoryListReceived: string = 'CategoryListReceived';
        public ItemListTypeChanged: string = 'ItemListTypeChanged';
        public ItemFocused: string = 'ItemFocused';
        public NewItemSaved: string = 'NewItemSaved';
        public ItemListReceived: string = 'ItemListReceived';
        public ResponseAwaiting: string = 'ResponseAwaiting';
        public ResponseReceived: string = 'ResponseReceived';
        public ItemsSelected: string = 'ItemsSelected';
        public BulkItemOperationRequested: string = 'BulkItemOperationRequested';
    }

    //-----------------------------------------------------------------------------------------------------------------

    // Helper structure to store bulk item operation parameters.
    //
    export class BulkItemOperation {
        public OperationType: string = '';
        public IsScheduledEndDateUpdateRequested: bool = false;
        public IsScheduledStartDateUpdateRequested: bool = false;
        public IsCategoryListUpdateRequested: bool = false;
        public ScheduledEndDate: string = '';
        public ScheduledStartDate: string = '';
        public CategoryIdSelectedHash = {};
    }

    //-----------------------------------------------------------------------------------------------------------------

    // All my Web API operations return an error response with a "data.Message" - i.e. the default as provided by
    // ASP.NET Web API.
    //
    export class WebApiErrorResponseData {
        public Message: string = '';
    }

    //-----------------------------------------------------------------------------------------------------------------

    // All my Web API operations return an error response with a "data.Message" - i.e. the default as provided by
    // ASP.NET Web API.
    //
    export class WebApiErrorResponse {
        public data: WebApiErrorResponseData = null;
    }

    //-----------------------------------------------------------------------------------------------------------------

    // I could not find a TypeScript declaration for an Angular resource - so I made up my own which also includes
    // any custom methods that I am using.
    //
    export interface IResource {
        get(params: any, success: () => void, error: (response: WebApiErrorResponse) => void): any;
        query(params: any, success: () => void, error: (response: WebApiErrorResponse) => void): any;
        save(params: any, success: () => void, error: (response: WebApiErrorResponse) => void): any;
        update(params: any, success: () => void, error: (response: WebApiErrorResponse) => void): void;
        delete(params: any, success: () => void, error: (response: WebApiErrorResponse) => void): void;
        execute(params: any, success: () => void, error: (response: WebApiErrorResponse) => void): void;
    }

    //-----------------------------------------------------------------------------------------------------------------

    // Formats date as "yyyy-mm-dd". I know there are probably better ways to do this, but this works.
    // See my note on all those "AsString" members in the data contracts also.
    //
    export var formatDate = function (date: Date): string {
        var dd = date.getDate();
        var ddText = dd.toString();
        if (dd < 10) ddText = '0' + ddText;
        var mm = date.getMonth() + 1;
        var mmText = mm.toString();
        if (mm < 10) mmText = '0' + mmText;
        var yyyy = date.getFullYear().toString();
        return yyyy + '-' + mmText + '-' + ddText;
    };
}