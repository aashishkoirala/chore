/*******************************************************************************************************************************
 * AK.Chore.Domain.Tasks.RecurrenceGrouper
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
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace AK.Chore.Domain.Tasks
{
    /// <summary>
    /// Builds predicates and checks task-satisfaction for recurrence dates.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IRecurrenceGrouper
    {
        Expression<Func<Task, bool>> BuildPredicate(DateTime recurrenceDateEqualTo);
        Expression<Func<Task, bool>> BuildPredicate(DateTime[] recurrenceDateIn);
        Expression<Func<Task, bool>> BuildPredicate(DateTime recurrenceDateOnOrBefore, DateTime recurrenceDateOnOrAfter);
        bool TaskSatisfiesRecurrence(Task task, DateTime recurrenceDateEqualTo);
        bool TaskSatisfiesRecurrence(Task task, DateTime[] recurrenceDateIn);
        bool TaskSatisfiesRecurrence(Task task, DateTime recurrenceDateOnOrBefore, DateTime recurrenceDateOnOrAfter);
    }

    [Export(typeof (IRecurrenceGrouper)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class RecurrenceGrouper : IRecurrenceGrouper
    {
        public Expression<Func<Task, bool>> BuildPredicate(DateTime recurrenceDateEqualTo)
        {
            return BuildPredicateWithParameter(recurrenceDateEqualTo, null);
        }

        public Expression<Func<Task, bool>> BuildPredicate(DateTime[] recurrenceDateIn)
        {
            var parameterExpression = Expression.Parameter(typeof (Task), "x");
            var predicates = recurrenceDateIn
                .Select(x => BuildPredicateWithParameter(x, parameterExpression))
                .ToArray();

            return CombinePredicates(parameterExpression, Expression.Or, predicates);
        }

        public Expression<Func<Task, bool>> BuildPredicate(
            DateTime recurrenceDateOnOrBefore, DateTime recurrenceDateOnOrAfter)
        {
            var dates = new List<DateTime>();
            var date = recurrenceDateOnOrAfter;
            while (date <= recurrenceDateOnOrBefore)
            {
                dates.Add(date);
                date = date.AddDays(1);
            }

            return this.BuildPredicate(dates.ToArray());
        }

        public bool TaskSatisfiesRecurrence(Task task, DateTime recurrenceDateEqualTo)
        {
            return this.BuildPredicate(recurrenceDateEqualTo).Compile()(task);
        }

        public bool TaskSatisfiesRecurrence(Task task, DateTime[] recurrenceDateIn)
        {
            return this.BuildPredicate(recurrenceDateIn).Compile()(task);
        }

        public bool TaskSatisfiesRecurrence(Task task, DateTime recurrenceDateOnOrBefore,
                                            DateTime recurrenceDateOnOrAfter)
        {
            return this.BuildPredicate(recurrenceDateOnOrBefore, recurrenceDateOnOrAfter).Compile()(task);
        }

        private static Expression<Func<Task, bool>> BuildPredicateWithParameter(
            DateTime recurrenceDateEqualTo, ParameterExpression parameterExpression)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator

            Expression<Func<Task, bool>> predicate =
                x =>
                (x.Recurrence.Type == RecurrenceType.Hourly &&
                 recurrenceDateEqualTo.Month >= x.Recurrence.MonthOfYear &&
                 recurrenceDateEqualTo.Day >= x.Recurrence.DayOfMonth) ||
                (x.Recurrence.Type == RecurrenceType.Daily &&
                 MatchesDailyRecurrence(recurrenceDateEqualTo, x.Recurrence)) ||
                (x.Recurrence.Type == RecurrenceType.Weekly &&
                 MatchesWeeklyRecurrence(recurrenceDateEqualTo, x.Recurrence) &&
                 x.Recurrence.DaysOfWeek.Contains(recurrenceDateEqualTo.DayOfWeek)) ||
                (x.Recurrence.Type == RecurrenceType.Monthly &&
                 MatchesMonthlyRecurrence(recurrenceDateEqualTo, x.Recurrence) &&
                 x.Recurrence.DayOfMonth == recurrenceDateEqualTo.Day) ||
                (x.Recurrence.Type == RecurrenceType.Yearly &&
                 MatchesYearlyRecurrence(recurrenceDateEqualTo, x.Recurrence) &&
                 x.Recurrence.MonthOfYear == recurrenceDateEqualTo.Month &&
                 x.Recurrence.DayOfMonth == recurrenceDateEqualTo.Day);

            // ReSharper restore CompareOfFloatsByEqualityOperator

            if (parameterExpression != null)
            {
                var visitor = new Visitor(parameterExpression);
                var expression = visitor.Visit(predicate.Body);
                predicate = Expression.Lambda<Func<Task, bool>>(expression, parameterExpression);
            }

            return predicate;
        }

        private static bool MatchesDailyRecurrence(DateTime recurrenceDate, Recurrence recurrence)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator

            var days = recurrenceDate.Subtract(
                new DateTime(recurrenceDate.Year,
                             recurrence.MonthOfYear, recurrence.DayOfMonth)).TotalDays;

            if (days < 0) days += 365;

            return days%recurrence.Interval == 0;

            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        private static bool MatchesWeeklyRecurrence(DateTime recurrenceDate, Recurrence recurrence)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator

            var configuredDate = new DateTime(recurrenceDate.Year, recurrence.MonthOfYear, recurrence.DayOfMonth);

            var offset = (int) recurrenceDate.DayOfWeek - (int) configuredDate.DayOfWeek;
            configuredDate = configuredDate.AddDays(offset);

            var days = recurrenceDate.Subtract(configuredDate).TotalDays;
            if (days < 0) days += 365;

            var weeks = days/7;
            return weeks%recurrence.Interval == 0;

            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        private static bool MatchesMonthlyRecurrence(DateTime recurrenceDate, Recurrence recurrence)
        {
            var configuredDate = new DateTime(recurrenceDate.Year, recurrence.MonthOfYear, recurrence.DayOfMonth);

            if (recurrenceDate.Year < configuredDate.Year) return false;

            var months = (recurrenceDate.Year - configuredDate.Year)*12;
            months += (recurrenceDate.Month - configuredDate.Month);

            return months%recurrence.Interval == 0;
        }

        private static bool MatchesYearlyRecurrence(DateTime recurrenceDate, Recurrence recurrence)
        {
            var configuredDate = new DateTime(recurrenceDate.Year, recurrence.MonthOfYear, recurrence.DayOfMonth);

            if (recurrenceDate.Year < configuredDate.Year) return false;

            var months = (recurrenceDate.Year - configuredDate.Year)*12;
            months += (recurrenceDate.Month - configuredDate.Month);

            var years = months/12;

            return years%recurrence.Interval == 0;
        }

        private static Expression<Func<Task, bool>> CombinePredicates(
            ParameterExpression parameterExpression,
            Func<Expression, Expression, BinaryExpression> combiner,
            params Expression<Func<Task, bool>>[] predicates)
        {
            if (!predicates.Any()) return x => true;

            var combinedExpression = predicates.Select(x => x.Body).Aggregate(combiner);

            return Expression.Lambda<Func<Task, bool>>(combinedExpression, parameterExpression);
        }

        private class Visitor : ExpressionVisitor
        {
            private readonly ParameterExpression parameterExpression;

            public Visitor(ParameterExpression parameterExpression)
            {
                this.parameterExpression = parameterExpression;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node.Name == "x" ? this.parameterExpression : base.VisitParameter(node);
            }
        }
    }
}