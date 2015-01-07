/*******************************************************************************************************************************
 * AK.Chore.Contracts.FilterAccess.Criterion
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
    /// Represents a criterion.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Criterion
    {
        public CriterionType Type { get; set; }
        public SimpleCriterion Simple { get; set; }
        public ComplexCriterion Complex { get; set; }
        public RecurrenceCriterion Recurrence { get; set; }
    }
}