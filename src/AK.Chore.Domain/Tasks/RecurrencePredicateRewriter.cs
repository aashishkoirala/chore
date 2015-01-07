/*******************************************************************************************************************************
 * AK.Chore.Domain.Tasks.RecurrencePredicateRewriter
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
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace AK.Chore.Domain.Tasks
{
    /// <summary>
    /// Rewrites recurrence grouper parameter or calls to recurrence grouper in task predicates.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IRecurrencePredicateRewriter
    {
        Expression<Func<Task, bool>> ReplaceRecurrenceGrouperParameter(Expression<Func<Task, bool>> predicate);
        Expression<Func<Task, bool>> ReplaceRecurrenceGrouperCall(Expression<Func<Task, bool>> predicate);
    }

    [Export(typeof (IRecurrencePredicateRewriter)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class RecurrencePredicateRewriter : IRecurrencePredicateRewriter
    {
        private readonly IRecurrenceGrouper recurrenceGrouper;

        [ImportingConstructor]
        public RecurrencePredicateRewriter([Import] IRecurrenceGrouper recurrenceGrouper)
        {
            this.recurrenceGrouper = recurrenceGrouper;
        }

        public Expression<Func<Task, bool>> ReplaceRecurrenceGrouperParameter(Expression<Func<Task, bool>> predicate)
        {
            var parameter = predicate.Parameters.First();
            var visitor = new Visitor(
                Mode.ReplaceRecurrenceGrouperParameter,
                parameter,
                this.recurrenceGrouper);
            var transformedExpression = visitor.Visit(predicate.Body);

            return Expression.Lambda<Func<Task, bool>>(transformedExpression, predicate.Parameters);
        }

        public Expression<Func<Task, bool>> ReplaceRecurrenceGrouperCall(Expression<Func<Task, bool>> predicate)
        {
            var parameter = predicate.Parameters.First();
            var visitor = new Visitor(
                Mode.ReplaceRecurrenceGrouperCall,
                parameter,
                this.recurrenceGrouper);
            var transformedExpression = visitor.Visit(predicate.Body);

            return Expression.Lambda<Func<Task, bool>>(transformedExpression, predicate.Parameters);
        }

        private class Visitor : ExpressionVisitor
        {
            private readonly Mode mode;
            private readonly IRecurrenceGrouper recurrenceGrouper;
            private readonly ParameterExpression parameter;

            public Visitor(Mode mode, ParameterExpression parameter, IRecurrenceGrouper recurrenceGrouper)
            {
                this.mode = mode;
                this.parameter = parameter;
                this.recurrenceGrouper = recurrenceGrouper;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name != "TaskSatisfiesRecurrence" ||
                    node.Method.DeclaringType != typeof (IRecurrenceGrouper))
                {
                    return base.VisitMethodCall(node);
                }

                switch (this.mode)
                {
                    case Mode.ReplaceRecurrenceGrouperParameter:
                        return Expression.Call(Expression.Constant(this.recurrenceGrouper), node.Method, node.Arguments);

                    case Mode.ReplaceRecurrenceGrouperCall:
                        {
                            if (this.parameter == null)
                                throw new InvalidOperationException("Invalid predicate being operated on.");

                            return this.ReplaceRecurrenceGrouperCall();
                        }
                }

                return base.VisitMethodCall(node);
            }

            private Expression ReplaceRecurrenceGrouperCall()
            {
                var stateProperty = Expression.Property(this.parameter, "State");
                var recurringState = Expression.Constant(TaskState.Recurring);
                var stateFilter = Expression.Equal(stateProperty, recurringState);

                var recurrenceProperty = Expression.Property(this.parameter, "Recurrence");
                var recurrenceEnabledProperty = Expression.Property(recurrenceProperty, "IsEnabled");
                var recurrenceTypeProperty = Expression.Property(recurrenceProperty, "Type");
                var nonRecurringType = Expression.Constant(RecurrenceType.NonRecurring);
                var recurrenceTypeFilter = Expression.NotEqual(recurrenceTypeProperty, nonRecurringType);

                return Expression.And(Expression.And(stateFilter, recurrenceEnabledProperty), recurrenceTypeFilter);
            }
        }

        private enum Mode
        {
            ReplaceRecurrenceGrouperParameter,
            ReplaceRecurrenceGrouperCall
        }
    }
}