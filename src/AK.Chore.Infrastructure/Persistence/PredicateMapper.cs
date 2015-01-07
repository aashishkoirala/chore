/*******************************************************************************************************************************
 * AK.Chore.Infrastructure.Persistence.PredicateMapper
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
    /// Maps predicates from the realm of domains to persistence objects.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class PredicateMapper<TFrom, TTo>
    {
        private readonly IDictionary<Expression, Expression> map = new Dictionary<Expression, Expression>();

        public void Map<TFromMember, TToMember>(
            Expression<Func<TFrom, TFromMember>> fromMember,
            Expression<Func<TTo, TToMember>> toMember)
        {
            this.map[fromMember] = toMember;
        }

        public IReadOnlyCollection<Mapping> GetMappings()
        {
            return (from pair in this.map
                    let fromExpression = GetMemberExpression(pair.Key)
                    let toExpression = GetMemberExpression(pair.Value)
                    select GetMapping(fromExpression, toExpression)).ToList();
        }

        private static MemberExpression GetMemberExpression(Expression expression)
        {
            if (expression is LambdaExpression) return GetMemberExpression((expression as LambdaExpression).Body);
            if (expression is UnaryExpression) return (MemberExpression) ((expression as UnaryExpression).Operand);
            return (MemberExpression) expression;
        }

        private static Mapping GetMapping(MemberExpression fromExpression, MemberExpression toExpression)
        {
            var memberExpression = fromExpression.Expression as MemberExpression;
            if (memberExpression != null)
            {
                var parentExpression = memberExpression;
                return new Mapping
                    {
                        ToMemberName = toExpression.Member.Name,
                        FromMemberName = fromExpression.Member.Name,
                        FromMemberOwningMemberName = parentExpression.Member.Name,
                        FromMemberOwningType = ((PropertyInfo) parentExpression.Member).PropertyType
                    };
            }

            if (fromExpression.Expression is ParameterExpression)
            {
                return new Mapping
                    {
                        FromMemberOwningMemberName = fromExpression.Member.Name,
                        FromMemberOwningType = ((PropertyInfo) fromExpression.Member).PropertyType
                    };
            }

            throw new NotSupportedException();
        }

        public class Mapping
        {
            public Type FromMemberOwningType { get; set; }
            public string FromMemberOwningMemberName { get; set; }
            public string FromMemberName { get; set; }
            public string ToMemberName { get; set; }
        }
    }
}