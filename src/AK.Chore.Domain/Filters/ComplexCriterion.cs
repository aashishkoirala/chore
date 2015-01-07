/*******************************************************************************************************************************
 * AK.Chore.Domain.Filters.ComplexCriterion
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

using AK.Commons.DomainDriven;

namespace AK.Chore.Domain.Filters
{
    /// <summary>
    /// Represents a complex criterion made up of conjuncted multiple criteria.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class ComplexCriterion : Criterion, IValueObject<ComplexCriterion>
    {
        public Criterion Criterion1 { get; protected set; }
        public Criterion Criterion2 { get; protected set; }
        public Conjunction Conjunction { get; protected set; }

        protected ComplexCriterion()
        {
        }

        public ComplexCriterion(Criterion criterion1, Conjunction conjunction, Criterion criterion2)
        {
            this.Criterion1 = criterion1;
            this.Conjunction = conjunction;
            this.Criterion2 = criterion2;
        }

        public static ComplexCriterion Not(Criterion criterion)
        {
            return new ComplexCriterion(True, Conjunction.AndNot, criterion);
        }

        public bool Equals(ComplexCriterion other)
        {
            return this.Criterion1.Equals(other.Criterion1) &&
                   this.Criterion2.Equals(other.Criterion2) &&
                   this.Conjunction == other.Conjunction;
        }

        public override bool Equals(object obj)
        {
            return obj is ComplexCriterion && this.Equals(obj as ComplexCriterion);
        }

        public override int GetHashCode()
        {
            return new object[]
                {
                    this.Criterion1,
                    this.Criterion2,
                    this.Conjunction
                }.GetHashCode();
        }
    }
}