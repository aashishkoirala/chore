/*******************************************************************************************************************************
 * AK.Chore.Domain.Tasks.TaskGrouper
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
using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace AK.Chore.Domain.Tasks
{
    /// <summary>
    /// Groups tasks based on criteria.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface ITaskGrouper
    {
        Action<Expression<Func<Task, bool>>> PredicateBuilt { get; set; }

        IReadOnlyCollection<Task> LoadForCriterion(
            Criterion criterion, User user, DateTime today,
            IUserRepository userRepository,
            IFolderRepository folderRepository,
            ITaskRepository taskRepository,
            IReadOnlyCollection<Folder> folders = null);

        bool TaskSatisfiesCriterion(
            Task task, Criterion criterion, User user, DateTime today,
            IReadOnlyCollection<Folder> folders = null);
    }

    [Export(typeof (ITaskGrouper)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class TaskGrouper : ITaskGrouper
    {
        private readonly IRecurrencePredicateRewriter recurrencePredicateRewriter;

        [ImportingConstructor]
        public TaskGrouper([Import] IRecurrencePredicateRewriter recurrencePredicateRewriter)
        {
            this.recurrencePredicateRewriter = recurrencePredicateRewriter;
        }

        public Action<Expression<Func<Task, bool>>> PredicateBuilt { get; set; }

        public IReadOnlyCollection<Task> LoadForCriterion(
            Criterion criterion,
            User user,
            DateTime today,
            IUserRepository userRepository,
            IFolderRepository folderRepository,
            ITaskRepository taskRepository,
            IReadOnlyCollection<Folder> folders = null)
        {
            var predicate = this.BuildPredicate(criterion, user, folders ?? new Folder[0], today);
            return taskRepository.ListForPredicate(predicate, userRepository, folderRepository);
        }

        public bool TaskSatisfiesCriterion(Task task, Criterion criterion, User user, DateTime today,
                                           IReadOnlyCollection<Folder> folders = null)
        {
            if (task == null)
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.TaskGrouperSatisfactionComputationError,
                    new {Reason = "Task not specified"});
            }

            if (criterion == null)
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.TaskGrouperSatisfactionComputationError,
                    new {Reason = "Criterion not specified"});
            }

            if (user == null)
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.TaskGrouperSatisfactionComputationError,
                    new {Reason = "User not specified"});
            }

            folders = folders ?? new Folder[0];

            var predicate = this.BuildPredicate(criterion, user, folders, today);
            predicate = this.recurrencePredicateRewriter.ReplaceRecurrenceGrouperParameter(predicate);
            var func = predicate.Compile();

            return func(task);
        }

        private Expression<Func<Task, bool>> BuildPredicate(
            Criterion criterion,
            User user,
            IReadOnlyCollection<Folder> folders,
            DateTime today)
        {
            if (criterion == null)
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.TaskGrouperPredicateBuildError,
                    new {Reason = "Criterion not specified"});
            }

            if (user == null)
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.TaskGrouperPredicateBuildError,
                    new {Reason = "User not specified"});
            }

            if (folders == null)
            {
                throw new DomainValidationException(
                    DomainValidationErrorCode.TaskGrouperPredicateBuildError,
                    new {Reason = "Folders not specified"});
            }

            var parameterExpression = Expression.Parameter(typeof (Task), "x");
            var filterPredicate = this.BuildPredicateForCriterion(parameterExpression, criterion, user, today);
            var folderPredicate = BuildPredicateForFolders(parameterExpression, folders);

            var predicate = CombinePredicates(parameterExpression, filterPredicate, folderPredicate);
            if (this.PredicateBuilt != null) this.PredicateBuilt(predicate);

            return predicate;
        }

        private Expression<Func<Task, bool>> BuildPredicateForCriterion(
            ParameterExpression parameterExpression, Criterion criterion, User user, DateTime today)
        {
            Expression<Func<Task, bool>> predicate;
            if (criterion.Equals(Criterion.True)) predicate = x => true;
            else
            {
                var predicateExpression = this.ProcessCriterion(parameterExpression, criterion, today);
                predicate = Expression.Lambda<Func<Task, bool>>(predicateExpression, parameterExpression);
            }

            return CombinePredicates(
                parameterExpression, predicate,
                BuildPredicateForUser(parameterExpression, user.Id));
        }

        private static Expression<Func<Task, bool>> BuildPredicateForUser(
            ParameterExpression parameterExpression, int userId)
        {
            var userPropertyExpression = Expression.Property(parameterExpression, "User");
            var userIdPropertyExpression = Expression.Property(userPropertyExpression, "Id");
            var userIdExpression = Expression.Constant(userId);
            var predicateExpression = Expression.Equal(userIdPropertyExpression, userIdExpression);

            return Expression.Lambda<Func<Task, bool>>(predicateExpression, parameterExpression);
        }

        private static Expression<Func<Task, bool>> BuildPredicateForFolders(
            ParameterExpression parameterExpression, IReadOnlyCollection<Folder> folders)
        {
            if (!folders.Any()) return x => true;

            var folderIdArray = folders.Select(x => x.Id).ToArray();
            var folderIdArrayExpression = Expression.NewArrayInit(
                typeof (int),
                folderIdArray.Select(x => Expression.Constant(x)));

            var containsMethod = typeof (Enumerable)
                .GetMethods()
                .First(x => x.Name == "Contains" && x.IsGenericMethod)
                .MakeGenericMethod(typeof (int));

            var folderPropertyExpression = Expression.Property(parameterExpression, "Folder");
            var folderIdPropertyExpression = Expression.Property(folderPropertyExpression, "Id");

            var containsCallExpression = Expression.Call(
                null, containsMethod, folderIdArrayExpression, folderIdPropertyExpression);

            return Expression.Lambda<Func<Task, bool>>(containsCallExpression, parameterExpression);
        }

        private static Expression<Func<Task, bool>> CombinePredicates(
            ParameterExpression parameterExpression,
            Expression<Func<Task, bool>> predicate1,
            Expression<Func<Task, bool>> predicate2,
            Func<Expression, Expression, BinaryExpression> combiner = null)
        {
            combiner = combiner ?? Expression.And;
            var combinedExpression = combiner(predicate1.Body, predicate2.Body);

            return Expression.Lambda<Func<Task, bool>>(combinedExpression, parameterExpression);
        }

        private Expression ProcessCriterion(ParameterExpression parameterExpression, Criterion criterion, DateTime today)
        {
            if (criterion is ComplexCriterion)
                return this.ProcessComplexCriterion(parameterExpression, criterion as ComplexCriterion, today);

            if (criterion is SimpleCriterion)
                return ProcessSimpleCriterion(parameterExpression, criterion as SimpleCriterion, today);

            if (criterion is RecurrenceCriterion)
                return ProcessRecurrenceCriterion(parameterExpression, criterion as RecurrenceCriterion, today);

            throw new ArgumentException("Invalid Criterion type", "criterion");
        }

        private Expression ProcessComplexCriterion(ParameterExpression parameterExpression, ComplexCriterion criterion,
                                                   DateTime today)
        {
            var expression1 = this.ProcessCriterion(parameterExpression, criterion.Criterion1, today);
            var expression2 = this.ProcessCriterion(parameterExpression, criterion.Criterion2, today);

            switch (criterion.Conjunction)
            {
                case Conjunction.And:
                    return Expression.And(expression1, expression2);

                case Conjunction.Or:
                    return Expression.Or(expression1, expression2);

                case Conjunction.AndNot:
                    return Expression.And(expression1, Expression.Not(expression2));

                case Conjunction.OrNot:
                    return Expression.Or(expression1, Expression.Not(expression2));
            }

            throw new ArgumentException("Invalid conjunction on criterion", "criterion");
        }

        private static Expression ProcessSimpleCriterion(Expression parameterExpression, SimpleCriterion criterion,
                                                         DateTime today)
        {
            var leftExpression = Expression.Property(parameterExpression, criterion.Field.ToString());

            object value = null;
            Type type = null;
            switch (criterion.Value)
            {
                case FieldValue.TodaysDate:
                    value = (DateTime?) today;
                    type = typeof (DateTime?);
                    break;

                case FieldValue.ThisWeeksStartDate:
                    value = (DateTime?) ComputeThisWeeksStartDate(today);
                    type = typeof (DateTime?);
                    break;

                case FieldValue.ThisWeeksEndDate:
                    value = (DateTime?) ComputeThisWeeksStartDate(today).AddDays(7);
                    type = typeof (DateTime?);
                    break;

                case FieldValue.Literal:
                    value = ParseLiteral(criterion.Field, criterion.Literal, criterion.Operator == Operator.In, out type);
                    break;
            }

            if (value == null) throw new InvalidOperationException("Invalid value on criterion");

            var rightExpression = Expression.Constant(
                value,
                criterion.Operator == Operator.In ? type.MakeArrayType() : type);

            switch (criterion.Operator)
            {
                case Operator.Equals:
                    return Expression.Equal(leftExpression, rightExpression);

                case Operator.DoesNotEqual:
                    return Expression.NotEqual(leftExpression, rightExpression);

                case Operator.LessThan:
                    return Expression.LessThan(leftExpression, rightExpression);

                case Operator.LessThanOrEquals:
                    return Expression.LessThanOrEqual(leftExpression, rightExpression);

                case Operator.GreaterThan:
                    return Expression.GreaterThan(leftExpression, rightExpression);

                case Operator.GreaterThanOrEquals:
                    return Expression.GreaterThanOrEqual(leftExpression, rightExpression);

                case Operator.Like:
                    return BuildLikeClause(leftExpression, value.ToString());

                case Operator.In:
                    return BuildInClause(value, leftExpression);
            }

            throw new InvalidOperationException("Invalid operator on criterion");
        }

        private static Expression ProcessRecurrenceCriterion(Expression parameterExpression,
                                                             RecurrenceCriterion criterion, DateTime today)
        {
            var recurrenceGrouperType = typeof (IRecurrenceGrouper);
            var recurrenceGrouperParameter = Expression.Parameter(recurrenceGrouperType, "recurrenceGrouper");
            const string methodName = "TaskSatisfiesRecurrence";

            if (criterion.RecurrenceDateEquals != null)
            {
                var value = ConvertLiteralOrSpecial(criterion.RecurrenceDateEquals, today);
                var method = recurrenceGrouperType.GetMethod(methodName, new[] {typeof (Task), typeof (DateTime)});

                return Expression.Call(
                    recurrenceGrouperParameter, method, parameterExpression,
                    Expression.Constant(value));
            }

            if (criterion.RecurrenceDateIn != null)
            {
                var value = criterion.RecurrenceDateIn.Select(x => ConvertLiteralOrSpecial(x, today)).ToArray();
                var method = recurrenceGrouperType.GetMethod(methodName, new[] {typeof (Task), typeof (DateTime[])});

                var valueExpression = value.Cast<object>().Select(Expression.Constant).Cast<Expression>().ToArray();

                return Expression.Call(
                    recurrenceGrouperParameter, method, parameterExpression,
                    Expression.NewArrayInit(typeof (DateTime), valueExpression));
            }

            if (criterion.RecurrenceDateOnOrBefore != null && criterion.RecurrenceDateOnOrAfter != null)
            {
                var value1 = ConvertLiteralOrSpecial(criterion.RecurrenceDateOnOrBefore, today);
                var value2 = ConvertLiteralOrSpecial(criterion.RecurrenceDateOnOrAfter, today);
                var method = recurrenceGrouperType.GetMethod(methodName, new[]
                    {
                        typeof (Task),
                        typeof (DateTime),
                        typeof (DateTime)
                    });

                return Expression.Call(
                    recurrenceGrouperParameter, method, parameterExpression,
                    Expression.Constant(value1), Expression.Constant(value2));
            }

            throw new ArgumentException("Invalid criterion parameters", "criterion");
        }

        private static DateTime ConvertLiteralOrSpecial(RecurrenceCriterion.LiteralOrSpecialDate literalOrSpecialDate,
                                                        DateTime today)
        {
            switch (literalOrSpecialDate.Value)
            {
                case FieldValue.TodaysDate:
                    return today;
                case FieldValue.ThisWeeksStartDate:
                    return ComputeThisWeeksStartDate(today);
                case FieldValue.ThisWeeksEndDate:
                    return ComputeThisWeeksStartDate(today).AddDays(7);
                case FieldValue.Literal:
                    return literalOrSpecialDate.Literal;
            }

            throw new ArgumentException("Invalid Value.", "literalOrSpecialDate");
        }

        private static Expression BuildInClause(object array, Expression itemExpression)
        {
            var typedArray = (Array) Convert.ChangeType(array, itemExpression.Type.MakeArrayType());
            var expressions = (from object item in typedArray
                               select Expression.Constant(item, itemExpression.Type))
                .Cast<Expression>()
                .ToArray();

            var arrayInitExpression = Expression.NewArrayInit(itemExpression.Type, expressions);

            var containsMethod = typeof (Enumerable)
                .GetMethods()
                .First(x => x.Name == "Contains" && x.IsGenericMethod)
                .MakeGenericMethod(itemExpression.Type);

            return Expression.Call(null, containsMethod, arrayInitExpression, itemExpression);
        }

        private static Expression BuildLikeClause(Expression stringExpression, string value)
        {
            // ReSharper disable PossiblyMistakenUseOfParamsMethod

            var stringType = typeof (string);
            var valueExpression = Expression.Constant(value.Trim('%'));

            if (value.StartsWith("%") && value.EndsWith("%"))
            {
                var containsMethod = stringType.GetMethod("Contains");
                return Expression.Call(stringExpression, containsMethod, valueExpression);
            }

            if (value.StartsWith("%"))
            {
                var endsWithMethod = stringType.GetMethod("EndsWith");
                return Expression.Call(stringExpression, endsWithMethod, valueExpression);
            }

            if (value.EndsWith("%"))
            {
                var startsWithMethod = stringType.GetMethod("StartsWith");
                return Expression.Call(stringExpression, startsWithMethod, valueExpression);
            }

            var equalsMethod = stringType.GetMethod("Equals", new[] {typeof (string), typeof (StringComparison)});
            return Expression.Call(stringExpression, equalsMethod, valueExpression,
                                   Expression.Constant(StringComparison.OrdinalIgnoreCase));

            // ReSharper restore PossiblyMistakenUseOfParamsMethod
        }

        private static DateTime ComputeThisWeeksStartDate(DateTime today)
        {
            var daysToSubtract = new Dictionary<DayOfWeek, int>
                {
                    {DayOfWeek.Sunday, 0},
                    {DayOfWeek.Monday, 1},
                    {DayOfWeek.Tuesday, 2},
                    {DayOfWeek.Wednesday, 3},
                    {DayOfWeek.Thursday, 4},
                    {DayOfWeek.Friday, 5},
                    {DayOfWeek.Saturday, 6}
                };

            return today.AddDays(-1*daysToSubtract[today.DayOfWeek]);
        }

        private static object ParseLiteral(Field field, string literal, bool isArray, out Type type)
        {
            if (isArray)
            {
                var items = literal.Split('|');
                Array array = null;
                type = null;
                var index = 0;
                foreach (var item in items)
                {
                    var parsedItem = ParseLiteral(field, item, false, out type);
                    if (array == null) array = Array.CreateInstance(type, items.Length);

                    array.SetValue(parsedItem, index);
                    index++;
                }

                return array;
            }

            switch (field)
            {
                case Field.StartDate:
                case Field.EndDate:
                    type = typeof (DateTime?);
                    return (DateTime?) DateTime.Parse(literal);

                case Field.StartTime:
                case Field.EndTime:
                    type = typeof (TimeSpan?);
                    return (TimeSpan?) TimeSpan.Parse(literal);

                case Field.State:
                    type = typeof (TaskState);
                    return Enum.Parse(typeof (TaskState), literal);

                case Field.Description:
                    type = typeof (string);
                    return literal;
            }

            throw new ArgumentException("Invalid field", "field");
        }
    }
}
