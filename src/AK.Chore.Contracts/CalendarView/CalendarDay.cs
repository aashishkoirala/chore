/*******************************************************************************************************************************
 * AK.Chore.Contracts.CalendarView.CalendarDay
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

using System;
using System.Collections.Generic;

#endregion

namespace AK.Chore.Contracts.CalendarView
{
    /// <summary>
    /// Represents a day in a calendar week.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public IReadOnlyCollection<CalendarItem> DayItems { get; set; }
        public IReadOnlyCollection<CalendarHour> Hours { get; set; }
    }
}