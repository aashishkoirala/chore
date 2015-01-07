/*******************************************************************************************************************************
 * AK.Chore.Infrastructure.Persistence.PredicateTransformationExpressionVisitor
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace AK.Chore.Infrastructure.Persistence
{
    /// <summary>
    /// Replaces expressions in a predicate while mapping from the realm of domain objects to persistence objects.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class PredicateTransformationExpressionVisitor<TFrom, TTo> : ExpressionVisitor
    {
        private readonly ParameterExpression mappedParameter = Expression.Parameter(typeof (TTo), "x");
        private readonly IReadOnlyCollection<PredicateMapper<TFrom, TTo>.Mapping> mappings;

        public PredicateTransformationExpressionVisitor(PredicateMapper<TFrom, TTo> mapper)
        {
            this.mappings = mapper.GetMappings();
        }

        public Expression<Func<TTo, bool>> Transform(Expression<Func<TFrom, bool>> predicate)
        {
            var transformedExpression = this.Visit(predicate.Body);
            return Expression.Lambda<Func<TTo, bool>>(transformedExpression, this.mappedParameter);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            PropertyInfo matchingProperty;
            if (node.Member.DeclaringType == typeof (TFrom))
            {
                matchingProperty = typeof (TTo).GetProperty(node.Member.Name);
                return Expression.MakeMemberAccess(this.mappedParameter, matchingProperty);
            }

            if (!(node.Expression is MemberExpression)) return base.VisitMember(node);

            var fromMemberOwningMemberName = (node.Expression as MemberExpression).Member.Name;

            var mapping = this.mappings
                              .SingleOrDefault(x => x.FromMemberOwningType == node.Member.DeclaringType &&
                                                    x.FromMemberOwningMemberName == fromMemberOwningMemberName);
            if (mapping == null) return base.VisitMember(node);

            if (mapping.FromMemberName == null && mapping.ToMemberName == null)
            {
                var parentProperty = typeof (Task).GetProperty(fromMemberOwningMemberName);
                matchingProperty = parentProperty.PropertyType.GetProperty(node.Member.Name);
                var parentAccessExpression = Expression.MakeMemberAccess(this.mappedParameter, parentProperty);
                return Expression.MakeMemberAccess(parentAccessExpression, matchingProperty);
            }

            if (node.Member.Name != mapping.FromMemberName) throw new NotSupportedException();

            matchingProperty = typeof (TTo).GetProperty(mapping.ToMemberName);
            return Expression.MakeMemberAccess(this.mappedParameter, matchingProperty);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node.Type == typeof (TFrom) ? this.mappedParameter : base.VisitParameter(node);
        }
    }
}