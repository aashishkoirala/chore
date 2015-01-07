/*******************************************************************************************************************************
 * AK.Chore.Tests.ExpressionEqualityTestUtility
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

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace AK.Chore.Tests
{
    /// <summary>
    /// Checks if expressions are equal for our purposes.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal static class ExpressionEqualityTestUtility
    {
        public static bool IsPracticallyEqualTo(this Expression expression1, Expression expression2)
        {
            var visitor1 = new StackExpressionVisitor();
            visitor1.Visit(expression1);
            var stack1 = visitor1.Expressions;

            var visitor2 = new StackExpressionVisitor();
            visitor2.Visit(expression2);
            var stack2 = visitor2.Expressions;

            return AreExpressionStacksEqual(stack1, stack2);
        }

        private static bool AreExpressionStacksEqual(
            Stack<Expression> stack1, Stack<Expression> stack2)
        {
            if (stack1.Count != stack2.Count) return false;
            var count = stack1.Count;

            for (var i = 0; i < count; i++)
            {
                if (!AreExpressionNodesEqual(stack1.Pop(), stack2.Pop()))
                    return false;
            }

            return true;
        }

        private static bool AreExpressionNodesEqual(Expression expression1, Expression expression2)
        {
            if (expression1.NodeType != expression2.NodeType) return false;

            if (expression1.NodeType == ExpressionType.Constant)
            {
                return AreConstantExpressionNodesEqual(
                    (ConstantExpression) expression1, (ConstantExpression) expression2);
            }

            return true;
        }

        private static bool AreConstantExpressionNodesEqual(
            ConstantExpression expression1, ConstantExpression expression2)
        {
            if (expression1.Value == null && expression2.Value == null) return true;
            if (expression1.Value == null || expression2.Value == null) return false;

            return expression1.Value.Equals(expression2.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <author>Aashish Koirala</author>
        private class StackExpressionVisitor : ExpressionVisitor
        {
            private static readonly IDictionary<ExpressionType, ExpressionType> equivalence = new Dictionary
                <ExpressionType, ExpressionType>
                {
                    {ExpressionType.AndAlso, ExpressionType.And},
                    {ExpressionType.OrElse, ExpressionType.Or}
                };

            private readonly Stack<Expression> expressions = new Stack<Expression>();

            public Stack<Expression> Expressions
            {
                get { return this.expressions; }
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                ExpressionType nodeType;
                if (!equivalence.TryGetValue(node.NodeType, out nodeType))
                    nodeType = node.NodeType;

                var operation = Expression.Constant(nodeType);

                this.Visit(node.Left);
                this.expressions.Push(operation);
                this.Visit(node.Right);

                return node;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                var value = node.Value;
                if (value != null && value.GetType().Name.Contains("<>c__DisplayClass"))
                    value = value.GetType().GetFields()[0].GetValue(value);

                node = Expression.Constant(value);
                this.expressions.Push(node);

                return base.VisitConstant(node);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression != null && node.Expression.NodeType == ExpressionType.Constant &&
                    ((ConstantExpression) node.Expression).Value != null &&
                    ((ConstantExpression) node.Expression).Value.GetType().Name.Contains("<>c__DisplayClass"))
                {
                    this.Visit(node.Expression);
                    return node;
                }

                var propertyInfo = node.Member as PropertyInfo;
                if (propertyInfo != null && propertyInfo.CanRead && propertyInfo.GetGetMethod().IsStatic)
                {
                    this.Visit(Expression.Constant(propertyInfo.GetValue(null)));
                    return node;
                }

                this.expressions.Push(node);
                return base.VisitMember(node);
            }
        }
    }
}