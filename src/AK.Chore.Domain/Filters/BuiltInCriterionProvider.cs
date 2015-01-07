/*******************************************************************************************************************************
 * AK.Chore.Domain.Filters.BuiltInCriterionProvider
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

using AK.Chore.Domain.Tasks;
using System;
using System.ComponentModel.Composition;
using System.Linq;

#endregion

namespace AK.Chore.Domain.Filters
{
    /// <summary>
    /// Provides built-in criteria.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IBuiltInCriterionProvider
    {
        Criterion Today { get; }
        Criterion ThisWeek { get; }
        Criterion Completed { get; }
        Criterion AllNotCompleted { get; }
        Criterion AllRecurring { get; }
    }

    [Export(typeof (IBuiltInCriterionProvider)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class BuiltInCriterionProvider : IBuiltInCriterionProvider
    {
        private readonly TaskState[] allStates;
        private readonly string openNonRecurringStatesLiteral;

        public Criterion Today { get; private set; }
        public Criterion ThisWeek { get; private set; }
        public Criterion Completed { get; private set; }
        public Criterion AllNotCompleted { get; private set; }
        public Criterion AllRecurring { get; private set; }

        public BuiltInCriterionProvider()
        {
            this.allStates = Enum
                .GetValues(typeof (TaskState))
                .Cast<TaskState>()
                .ToArray();

            this.openNonRecurringStatesLiteral = allStates
                .Where(x => x != TaskState.Completed && x != TaskState.Recurring)
                .Select(x => x.ToString())
                .Aggregate((x1, x2) => x1 + "|" + x2);

            this.Today = this.BuildTodayCriterion();
            this.ThisWeek = this.BuildThisWeekCriterion();
            this.Completed = BuildCompletedCriterion();
            this.AllNotCompleted = this.BuildAllNotCompletedCriterion();
            this.AllRecurring = BuildAllRecurringCriterion();
        }

        private Criterion BuildTodayCriterion()
        {
            var startOrEndTodayCriterion =
                new ComplexCriterion(
                    new SimpleCriterion(Field.StartDate, Operator.LessThanOrEquals, FieldValue.TodaysDate),
                    Conjunction.Or,
                    new SimpleCriterion(Field.EndDate, Operator.LessThanOrEquals, FieldValue.TodaysDate));

            var nonRecurringTodayCriterion = new ComplexCriterion(
                new SimpleCriterion(Field.State, Operator.In, openNonRecurringStatesLiteral),
                Conjunction.And, startOrEndTodayCriterion);

            var recurringTodayCriterion = new RecurrenceCriterion(
                new RecurrenceCriterion.LiteralOrSpecialDate(FieldValue.TodaysDate));

            return new ComplexCriterion(nonRecurringTodayCriterion, Conjunction.Or, recurringTodayCriterion);
        }

        private Criterion BuildThisWeekCriterion()
        {
            var startThisWeekCriterion =
                new ComplexCriterion(
                    new SimpleCriterion(Field.StartDate, Operator.LessThanOrEquals, FieldValue.ThisWeeksStartDate),
                    Conjunction.Or,
                    new SimpleCriterion(Field.StartDate, Operator.LessThanOrEquals, FieldValue.ThisWeeksEndDate));

            var endThisWeekCriterion =
                new ComplexCriterion(
                    new SimpleCriterion(Field.EndDate, Operator.LessThanOrEquals, FieldValue.ThisWeeksStartDate),
                    Conjunction.Or,
                    new SimpleCriterion(Field.EndDate, Operator.LessThanOrEquals, FieldValue.ThisWeeksEndDate));

            var startOrEndThisWeekCriterion =
                new ComplexCriterion(startThisWeekCriterion, Conjunction.Or, endThisWeekCriterion);

            var nonRecurringThisWeekCriterion = new ComplexCriterion(
                new SimpleCriterion(Field.State, Operator.In, openNonRecurringStatesLiteral),
                Conjunction.And, startOrEndThisWeekCriterion);

            var recurringThisWeekCriterion = new RecurrenceCriterion(
                new RecurrenceCriterion.LiteralOrSpecialDate(FieldValue.ThisWeeksEndDate),
                new RecurrenceCriterion.LiteralOrSpecialDate(FieldValue.ThisWeeksStartDate));

            return new ComplexCriterion(nonRecurringThisWeekCriterion, Conjunction.Or, recurringThisWeekCriterion);
        }

        private static Criterion BuildCompletedCriterion()
        {
            return new SimpleCriterion(Field.State, Operator.Equals, TaskState.Completed.ToString());
        }

        private Criterion BuildAllNotCompletedCriterion()
        {
            return new SimpleCriterion(Field.State, Operator.In, openNonRecurringStatesLiteral);
        }

        private static Criterion BuildAllRecurringCriterion()
        {
            return new SimpleCriterion(Field.State, Operator.Equals, TaskState.Recurring.ToString());
        }
    }
}