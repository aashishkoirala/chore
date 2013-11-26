/*******************************************************************************************************************************
 * AK.To|Do.Contracts.Services.Data.ItemContracts.ToDoItemState
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

namespace AK.ToDo.Contracts.Services.Data.ItemContracts
{
    /// <summary>
    /// Different states a To-Do item can be in.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public enum ToDoItemState
    {
        NotStarted,
        InProgress,
        Paused,
        Done
    }
}