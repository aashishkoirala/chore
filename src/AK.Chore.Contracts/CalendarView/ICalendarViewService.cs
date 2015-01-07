/*******************************************************************************************************************************
 * AK.Chore.Contracts.CalendarView.ICalendarViewService
 * Copyright © 2014-2015 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of CHORE.
 *  
 * CHORE is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * CHORE is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with CHORE.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

#region Namespace Imports

using AK.Commons.Services;
using System;

#endregion

namespace AK.Chore.Contracts.CalendarView
{
    /// <summary>
    /// Operations related to calendar functionality.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ICalendarViewService
    {
        /// <summary>
        /// Gets a calendar week with all tasks within that week for the given user.
        /// </summary>
        /// <param name="dayInWeek">Date reperesenting any day within the week.</param>
        /// <param name="userId">User ID.</param>
        /// <returns>Result with calendar week information.</returns>
        OperationResult<CalendarWeek> GetCalendarWeekForUser(DateTime dayInWeek, int userId);
    }
}