/*******************************************************************************************************************************
 * AK.To|Do.Web.DataContracts.ToDoItemListRequest
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

module AK.ToDo.Web.DataContracts {

    // Holds information on a request for listing To-Do items.
    // The reason I'm using all the "AsString" properties for dates and so on are because the default serialized values
    // for dates by the Web API does not play nice with the HTML5 date control I'm using.
    //
    // TODO: Do something so we eliminate the need for this whole "AsString" business.
    //
    export class ToDoItemListRequest {

        public Type: string = '';
        public StateAsString: string = '';
        public ScheduledStartDateAsString: string = '';
        public ScheduledEndDateAsString: string = '';
        public ActualStartDateAsString: string = '';
        public ActualEndDateAsString: string = '';
        public CategoryIdListAsString: string[] = [];
        public TodayAsString: string = '';
    }
}