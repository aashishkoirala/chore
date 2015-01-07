/*******************************************************************************************************************************
 * AK.Chore.Domain.Filters.SimpleCriterion
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
    /// Represents a simple criterion - one that is based on the value of a field.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class SimpleCriterion : Criterion, IValueObject<SimpleCriterion>
    {
        public Field Field { get; protected set; }
        public Operator Operator { get; protected set; }
        public FieldValue Value { get; protected set; }
        public string Literal { get; protected set; }

        protected SimpleCriterion()
        {
        }

        public SimpleCriterion(Field field, Operator operatorType, FieldValue value)
        {
            this.Field = field;
            this.Operator = operatorType;
            this.Value = value;
            this.Literal = string.Empty;
        }

        public SimpleCriterion(Field field, Operator operatorType, string literal)
        {
            this.Field = field;
            this.Operator = operatorType;
            this.Value = FieldValue.Literal;
            this.Literal = literal;
        }

        public bool Equals(SimpleCriterion other)
        {
            return this.Field == other.Field &&
                   this.Operator == other.Operator &&
                   this.Value == other.Value &&
                   this.Literal == other.Literal;
        }

        public override bool Equals(object obj)
        {
            return obj is SimpleCriterion && this.Equals(obj as SimpleCriterion);
        }

        public override int GetHashCode()
        {
            return new object[]
                {
                    this.Field,
                    this.Operator,
                    this.Value,
                    this.Literal
                }.GetHashCode();
        }
    }
}