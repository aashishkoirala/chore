/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Domain.BuiltInCriterionProviderTests
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

using AK.Chore.Domain.Filters;
using AK.Chore.Domain.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace AK.Chore.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests for BuiltInCriterionProvider.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class BuiltInCriterionProviderTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Domain")]
        public void BuiltInCriterionProvider_Today_Works()
        {
            var today = this.builtInCriterionProvider.Today as ComplexCriterion;
            Assert.IsNotNull(today);
            Assert.AreEqual(today.Conjunction, Conjunction.Or);

            var nonRecurring = today.Criterion1 as ComplexCriterion;
            Assert.IsNotNull(nonRecurring);
            Assert.AreEqual(nonRecurring.Conjunction, Conjunction.And);

            var states = nonRecurring.Criterion1 as SimpleCriterion;
            Assert.IsNotNull(states);
            Assert.AreEqual(states.Operator, Operator.In);
            Assert.AreEqual(states.Field, Field.State);
            Assert.AreEqual(states.Value, FieldValue.Literal);
            Assert.AreEqual(states.Literal, "NotStarted|InProgress|Paused");

            var startEnd = nonRecurring.Criterion2 as ComplexCriterion;
            Assert.IsNotNull(startEnd);
            Assert.AreEqual(startEnd.Conjunction, Conjunction.Or);

            var start = startEnd.Criterion1 as SimpleCriterion;
            Assert.IsNotNull(start);
            Assert.AreEqual(start.Operator, Operator.LessThanOrEquals);
            Assert.AreEqual(start.Field, Field.StartDate);
            Assert.AreEqual(start.Value, FieldValue.TodaysDate);

            var end = startEnd.Criterion2 as SimpleCriterion;
            Assert.IsNotNull(end);
            Assert.AreEqual(end.Operator, Operator.LessThanOrEquals);
            Assert.AreEqual(end.Field, Field.EndDate);
            Assert.AreEqual(end.Value, FieldValue.TodaysDate);

            var recurring = today.Criterion2 as RecurrenceCriterion;
            Assert.IsNotNull(recurring);
            Assert.IsNotNull(recurring.RecurrenceDateEquals);
            Assert.AreEqual(recurring.RecurrenceDateEquals.Value, FieldValue.TodaysDate);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void BuiltInCriterionProvider_ThisWeek_Works()
        {
            var thisWeek = this.builtInCriterionProvider.ThisWeek as ComplexCriterion;

            Assert.IsNotNull(thisWeek);
            Assert.AreEqual(thisWeek.Conjunction, Conjunction.Or);

            var nonRecurring = thisWeek.Criterion1 as ComplexCriterion;
            Assert.IsNotNull(nonRecurring);
            Assert.AreEqual(nonRecurring.Conjunction, Conjunction.And);

            var states = nonRecurring.Criterion1 as SimpleCriterion;
            Assert.IsNotNull(states);
            Assert.AreEqual(states.Operator, Operator.In);
            Assert.AreEqual(states.Field, Field.State);
            Assert.AreEqual(states.Value, FieldValue.Literal);
            Assert.AreEqual(states.Literal, "NotStarted|InProgress|Paused");

            var startEnd = nonRecurring.Criterion2 as ComplexCriterion;
            Assert.IsNotNull(startEnd);
            Assert.AreEqual(startEnd.Conjunction, Conjunction.Or);

            var start = startEnd.Criterion1 as ComplexCriterion;
            Assert.IsNotNull(start);
            Assert.AreEqual(start.Conjunction, Conjunction.Or);

            var startWeekStart = start.Criterion1 as SimpleCriterion;
            Assert.IsNotNull(startWeekStart);
            Assert.AreEqual(startWeekStart.Operator, Operator.LessThanOrEquals);
            Assert.AreEqual(startWeekStart.Field, Field.StartDate);
            Assert.AreEqual(startWeekStart.Value, FieldValue.ThisWeeksStartDate);

            var startWeekEnd = start.Criterion2 as SimpleCriterion;
            Assert.IsNotNull(startWeekEnd);
            Assert.AreEqual(startWeekEnd.Operator, Operator.LessThanOrEquals);
            Assert.AreEqual(startWeekEnd.Field, Field.StartDate);
            Assert.AreEqual(startWeekEnd.Value, FieldValue.ThisWeeksEndDate);

            var end = startEnd.Criterion2 as ComplexCriterion;
            Assert.IsNotNull(end);
            Assert.AreEqual(end.Conjunction, Conjunction.Or);

            var endWeekStart = end.Criterion1 as SimpleCriterion;
            Assert.IsNotNull(endWeekStart);
            Assert.AreEqual(endWeekStart.Operator, Operator.LessThanOrEquals);
            Assert.AreEqual(endWeekStart.Field, Field.EndDate);
            Assert.AreEqual(endWeekStart.Value, FieldValue.ThisWeeksStartDate);

            var endWeekEnd = end.Criterion2 as SimpleCriterion;
            Assert.IsNotNull(endWeekEnd);
            Assert.AreEqual(endWeekEnd.Operator, Operator.LessThanOrEquals);
            Assert.AreEqual(endWeekEnd.Field, Field.EndDate);
            Assert.AreEqual(endWeekEnd.Value, FieldValue.ThisWeeksEndDate);

            var recurring = thisWeek.Criterion2 as RecurrenceCriterion;
            Assert.IsNotNull(recurring);
            Assert.IsNotNull(recurring.RecurrenceDateOnOrAfter);
            Assert.IsNotNull(recurring.RecurrenceDateOnOrBefore);
            Assert.AreEqual(recurring.RecurrenceDateOnOrAfter.Value, FieldValue.ThisWeeksStartDate);
            Assert.AreEqual(recurring.RecurrenceDateOnOrBefore.Value, FieldValue.ThisWeeksEndDate);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void BuiltInCriterionProvider_Completed_Works()
        {
            var completed = this.builtInCriterionProvider.Completed as SimpleCriterion;

            Assert.IsNotNull(completed);
            Assert.AreEqual(completed.Operator, Operator.Equals);
            Assert.AreEqual(completed.Field, Field.State);
            Assert.AreEqual(completed.Value, FieldValue.Literal);
            Assert.AreEqual(completed.Literal, TaskState.Completed.ToString());
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void BuiltInCriterionProvider_AllNotCompleted_Works()
        {
            var allNotCompleted = this.builtInCriterionProvider.AllNotCompleted as SimpleCriterion;

            Assert.IsNotNull(allNotCompleted);
            Assert.AreEqual(allNotCompleted.Operator, Operator.In);
            Assert.AreEqual(allNotCompleted.Field, Field.State);
            Assert.AreEqual(allNotCompleted.Value, FieldValue.Literal);
            Assert.AreEqual(allNotCompleted.Literal, "NotStarted|InProgress|Paused");
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void BuiltInCriterionProvider_AllRecurring_Works()
        {
            var allRecurring = this.builtInCriterionProvider.AllRecurring as SimpleCriterion;

            Assert.IsNotNull(allRecurring);
            Assert.AreEqual(allRecurring.Operator, Operator.Equals);
            Assert.AreEqual(allRecurring.Field, Field.State);
            Assert.AreEqual(allRecurring.Value, FieldValue.Literal);
            Assert.AreEqual(allRecurring.Literal, TaskState.Recurring.ToString());
        }
    }
}