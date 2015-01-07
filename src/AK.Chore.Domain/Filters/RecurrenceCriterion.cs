/*******************************************************************************************************************************
 * AK.Chore.Domain.Filters.RecurrenceCriterion
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

using AK.Commons.DomainDriven;
using System;
using System.Linq;

#endregion

namespace AK.Chore.Domain.Filters
{
    /// <summary>
    /// Represents a criterion based on recurrence.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class RecurrenceCriterion : Criterion, IValueObject<RecurrenceCriterion>
    {
        public LiteralOrSpecialDate RecurrenceDateEquals { get; protected set; }
        public LiteralOrSpecialDate[] RecurrenceDateIn { get; protected set; }
        public LiteralOrSpecialDate RecurrenceDateOnOrBefore { get; protected set; }
        public LiteralOrSpecialDate RecurrenceDateOnOrAfter { get; protected set; }

        public RecurrenceCriterion(LiteralOrSpecialDate recurrenceDateEquals)
        {
            this.RecurrenceDateEquals = recurrenceDateEquals;
        }

        public RecurrenceCriterion(LiteralOrSpecialDate[] recurrenceDateIn)
        {
            this.RecurrenceDateIn = recurrenceDateIn;
        }

        public RecurrenceCriterion(
            LiteralOrSpecialDate recurrenceDateOnOrBefore,
            LiteralOrSpecialDate recurrenceDateOnOrAfter)
        {
            this.RecurrenceDateOnOrBefore = recurrenceDateOnOrBefore;
            this.RecurrenceDateOnOrAfter = recurrenceDateOnOrAfter;
        }

        protected RecurrenceCriterion()
        {
        }

        public bool Equals(RecurrenceCriterion other)
        {
            return this.RecurrenceDateEquals == other.RecurrenceDateEquals &&
                   this.RecurrenceDateOnOrBefore == other.RecurrenceDateOnOrBefore &&
                   this.RecurrenceDateOnOrAfter == other.RecurrenceDateOnOrAfter &&
                   (this.RecurrenceDateIn == null && other.RecurrenceDateIn == null) ||
                   (this.RecurrenceDateIn != null && this.RecurrenceDateIn.SequenceEqual(other.RecurrenceDateIn));
        }

        public override bool Equals(object obj)
        {
            return obj is RecurrenceCriterion && this.Equals(obj as RecurrenceCriterion);
        }

        public override int GetHashCode()
        {
            return new object[]
                {
                    this.RecurrenceDateEquals,
                    this.RecurrenceDateIn,
                    this.RecurrenceDateOnOrAfter,
                    this.RecurrenceDateOnOrBefore
                }.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <author>Aashish Koirala</author>
        public class LiteralOrSpecialDate : IValueObject<LiteralOrSpecialDate>
        {
            public FieldValue Value { get; private set; }
            public DateTime Literal { get; private set; }

            public LiteralOrSpecialDate(FieldValue value)
            {
                if (value == FieldValue.Literal) throw new ArgumentException("Cannot use Literal.", "value");

                this.Value = value;
            }

            public LiteralOrSpecialDate(DateTime literal)
            {
                this.Literal = literal;
                this.Value = FieldValue.Literal;
            }

            public bool Equals(LiteralOrSpecialDate other)
            {
                return this.Value == other.Value && this.Literal == other.Literal;
            }

            public override bool Equals(object obj)
            {
                return obj is LiteralOrSpecialDate && this.Equals(obj as LiteralOrSpecialDate);
            }

            public static bool operator ==(LiteralOrSpecialDate o1, LiteralOrSpecialDate o2)
            {
                if (ReferenceEquals(o1, null) && ReferenceEquals(o2, null)) return true;
                if (ReferenceEquals(o1, null) || ReferenceEquals(o2, null)) return false;
                return o1.Equals(o2);
            }

            public static bool operator !=(LiteralOrSpecialDate o1, LiteralOrSpecialDate o2)
            {
                return !(o1 == o2);
            }

            public override int GetHashCode()
            {
                return new object[]
                    {
                        this.Value,
                        this.Literal
                    }.GetHashCode();
            }
        }
    }
}