/*******************************************************************************************************************************
 * AK.Chore.Contracts.FilterAccess.RecurrenceCriterion
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

namespace AK.Chore.Contracts.FilterAccess
{
    /// <summary>
    /// Represents a criterion based on recurrence.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class RecurrenceCriterion
    {
        public RecurrenceCriterionType Type { get; set; }
        public RecurrenceCriterionPart EqualsTo { get; set; }
        public RecurrenceCriterionPart[] In { get; set; }
        public RecurrenceCriterionPart OnOrBefore { get; set; }
        public RecurrenceCriterionPart OnOrAfter { get; set; }
    }
}