/*******************************************************************************************************************************
 * AK.To|Do.Web.DataContracts.ToDoItem
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

module AK.ToDo.Web.DataContracts {

    // Holds information on a single To-Do item.
    // The reason I'm using all the "AsString" properties for dates and so on are because the default serialized values
    // for dates by the Web API does not play nice with the HTML5 date control I'm using.
    //
    // TODO: Do something so we eliminate the need for this whole "AsString" business.
    //
    export class ToDoItem {

        public Id: string = '';
        public Description: string = '';
        public StateAsString: string = 'NotStarted';
        public ScheduledStartDateAsString: string = '';
        public ScheduledEndDateAsString: string = Common.formatDate(new Date());
        public ActualStartDateAsString: string = '';
        public ActualEndDateAsString: string = '';
        public CategoryIdList: string[] = [];
        public CategoryIdSelectedHash = {};
        public IsSelected: bool = false;
        public IsLate: bool = false;

        //-------------------------------------------------------------------------------------------------------------

        public getSummaryDescription(): string {

            var state: string = '';
            switch (this.StateAsString) {

                case 'NotStarted':
                    state = this.ScheduledStartDateAsString == '' ? '' : 'Not Started';
                    break;

                case 'InProgress':
                    state = 'In Progress (started on ' + this.ActualStartDateAsString + ')';
                    break;

                case 'Paused':
                    state = 'Paused (started on ' + this.ActualStartDateAsString + ')';
                    break;

                case 'Done':
                    state = 'Done on ' + this.ActualEndDateAsString;
                    break;
            }

            if (state != '') state += ', ';
            state += 'Due ' + this.ScheduledEndDateAsString;
            if (this.ScheduledStartDateAsString != '') state += ', Start by ' + this.ScheduledStartDateAsString;

            return state;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getDescriptionCssClass(): string {
            // TODO: Do this less hackily with directives.

            return this.IsLate ? "late-item" : "";
        }

        //-------------------------------------------------------------------------------------------------------------

        public clone(): ToDoItem {
            var self = this;
            var item = new ToDoItem();

            for (var key in self) {
                item[key] = self[key];
            }

            return item;
        }

        //-------------------------------------------------------------------------------------------------------------

        public copyTo(targetItem: ToDoItem) {
            var self = this;

            for (var key in self) {
                targetItem[key] = self[key];
            }
        }
    }
}