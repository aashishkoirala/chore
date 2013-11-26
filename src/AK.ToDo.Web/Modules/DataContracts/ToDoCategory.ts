/*******************************************************************************************************************************
 * AK.To|Do.Web.DataContracts.ToDoCategory
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

    // Holds information on a single To-Do item category.
    //
    export class ToDoCategory {
        public Id: string = '';
        public Name: string = '';
        public ParentId: string = '';
        public Level: number = 0;
        public IsSelected: bool = false;

        //-------------------------------------------------------------------------------------------------------------

        // Gets an "indenter" string by repeating the given character a number of times based
        // on the Level property.
        //
        // TODO: Replace with a directive, perhaps?
        //
        public getIndent(indent: string): string {
            var s: string = '';
            for (var i: number = 0; i < this.Level; i++)
                s += indent;
            return s;
        }
    }
}