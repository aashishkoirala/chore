/*******************************************************************************************************************************
 * AK.Chore.Application.Mappers.FilterMapper
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

using AK.Chore.Contracts.FilterAccess;
using AK.Chore.Domain.Filters;
using AK.Commons.DomainDriven;
using System;
using System.Globalization;
using System.Linq;
using ComplexCriterion = AK.Chore.Contracts.FilterAccess.ComplexCriterion;
using Criterion = AK.Chore.Contracts.FilterAccess.Criterion;
using Filter = AK.Chore.Contracts.FilterAccess.Filter;
using RecurrenceCriterion = AK.Chore.Contracts.FilterAccess.RecurrenceCriterion;
using SimpleCriterion = AK.Chore.Contracts.FilterAccess.SimpleCriterion;

#endregion

namespace AK.Chore.Application.Mappers
{
    /// <summary>
    /// Maps between Filter data contracts and domain objects.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class FilterMapper
    {
        public static Filter Map(this Domain.Filters.Filter filter)
        {
            if (filter == null) return null;

            return new Filter
                {
                    Id = filter.Id,
                    UserId = filter.User.Id,
                    Name = filter.Name,
                    Criterion = filter.Criterion.Map()
                };
        }

        public static Domain.Filters.Filter Map(
            this Filter filter, IEntityIdGenerator<int> idGenerator,
            Func<int, Domain.Filters.Filter> filterRetriever,
            Func<int, Domain.Users.User> userRetriever)
        {
            if (filter == null) return null;

            if (filter.Id > 0)
            {
                var mapped = filterRetriever(filter.Id);
                if (mapped == null) return null;

                mapped.Name = filter.Name;
                mapped.Criterion = Map(filter.Criterion);
                return mapped;
            }

            var user = userRetriever(filter.UserId);
            return user == null
                       ? null
                       : new Domain.Filters.Filter(idGenerator, filter.Name, user, filter.Criterion.Map());
        }

        public static Criterion Map(this Domain.Filters.Criterion criterion)
        {
            if (criterion == Domain.Filters.Criterion.True) return new Criterion {Type = CriterionType.True};

            var simpleCriterion = criterion as Domain.Filters.SimpleCriterion;
            if (simpleCriterion != null)
            {
                return new Criterion
                    {
                        Type = CriterionType.Simple,
                        Simple = new SimpleCriterion
                            {
                                FieldName = simpleCriterion.Field.ToString(),
                                OperatorName = simpleCriterion.Operator.ToString(),
                                ValueName = simpleCriterion.Value.ToString(),
                                ValueLiteral = simpleCriterion.Literal
                            }
                    };
            }

            var recurrenceCriterion = criterion as Domain.Filters.RecurrenceCriterion;
            if (recurrenceCriterion != null)
            {
                return new Criterion
                    {
                        Type = CriterionType.Recurrence,
                        Recurrence = recurrenceCriterion.Map()
                    };
            }

            var complexCriterion = criterion as Domain.Filters.ComplexCriterion;
            if (complexCriterion != null)
            {
                return new Criterion
                    {
                        Type = CriterionType.Complex,
                        Complex = new ComplexCriterion
                            {
                                Criterion1 = complexCriterion.Criterion1.Map(),
                                ConjunctionName = complexCriterion.Conjunction.ToString(),
                                Criterion2 = complexCriterion.Criterion2.Map()
                            }
                    };
            }

            throw new NotSupportedException("This type of criterion not supported.");
        }

        public static Domain.Filters.Criterion Map(this Criterion criterion)
        {
            switch (criterion.Type)
            {
                case CriterionType.Simple:
                    var field = ParseEnum<Field>(criterion.Simple.FieldName);
                    var operatorType = ParseEnum<Operator>(criterion.Simple.OperatorName);
                    var value = ParseEnum<FieldValue>(criterion.Simple.ValueName);

                    return value == FieldValue.Literal
                               ? new Domain.Filters.SimpleCriterion(field, operatorType, criterion.Simple.ValueLiteral)
                               : new Domain.Filters.SimpleCriterion(field, operatorType, value);

                case CriterionType.Recurrence:
                    return criterion.Recurrence.Map();

                case CriterionType.Complex:
                    var conjunction = ParseEnum<Conjunction>(criterion.Complex.ConjunctionName);
                    return new Domain.Filters.ComplexCriterion(criterion.Complex.Criterion1.Map(), conjunction,
                                                               criterion.Complex.Criterion2.Map());

                case CriterionType.True:
                    return Domain.Filters.Criterion.True;
            }

            throw new NotSupportedException("This type of criterion not supported.");
        }

        private static RecurrenceCriterion Map(this Domain.Filters.RecurrenceCriterion criterion)
        {
            if (criterion.RecurrenceDateEquals != null)
            {
                return new RecurrenceCriterion
                    {
                        Type = RecurrenceCriterionType.Equals,
                        EqualsTo = new RecurrenceCriterionPart
                            {
                                ValueName = criterion.RecurrenceDateEquals.Value.ToString(),
                                ValueLiteral =
                                    criterion.RecurrenceDateEquals.Literal.ToString(CultureInfo.InvariantCulture)
                            }
                    };
            }

            if (criterion.RecurrenceDateIn != null)
            {
                return new RecurrenceCriterion
                    {
                        Type = RecurrenceCriterionType.In,
                        In = criterion.RecurrenceDateIn
                                      .Select(x =>
                                              new RecurrenceCriterionPart
                                                  {
                                                      ValueName = x.Value.ToString(),
                                                      ValueLiteral = x.Literal.ToString(CultureInfo.InvariantCulture)
                                                  }).ToArray()
                    };
            }

            if (criterion.RecurrenceDateOnOrAfter == null || criterion.RecurrenceDateOnOrBefore == null)
                throw new NotSupportedException("This type of criterion not supported.");

            return new RecurrenceCriterion
                {
                    Type = RecurrenceCriterionType.BeforeAfter,
                    OnOrBefore = new RecurrenceCriterionPart
                        {
                            ValueName = criterion.RecurrenceDateOnOrBefore.Value.ToString(),
                            ValueLiteral =
                                criterion.RecurrenceDateOnOrBefore.Literal.ToString(CultureInfo.InvariantCulture)
                        },
                    OnOrAfter = new RecurrenceCriterionPart
                        {
                            ValueName = criterion.RecurrenceDateOnOrAfter.Value.ToString(),
                            ValueLiteral =
                                criterion.RecurrenceDateOnOrAfter.Literal.ToString(CultureInfo.InvariantCulture)
                        },
                };
        }

        private static Domain.Filters.RecurrenceCriterion Map(this RecurrenceCriterion criterion)
        {
            switch (criterion.Type)
            {
                case RecurrenceCriterionType.Equals:
                    return new Domain.Filters.RecurrenceCriterion(criterion.EqualsTo.Map());

                case RecurrenceCriterionType.In:
                    return new Domain.Filters.RecurrenceCriterion(criterion.In.Select(Map).ToArray());

                case RecurrenceCriterionType.BeforeAfter:
                    return new Domain.Filters.RecurrenceCriterion(criterion.OnOrBefore.Map(), criterion.OnOrAfter.Map());
            }

            throw new NotSupportedException("This type of criterion not supported.");
        }

        private static Domain.Filters.RecurrenceCriterion.LiteralOrSpecialDate Map(this RecurrenceCriterionPart part)
        {
            var value = ParseEnum<FieldValue>(part.ValueName);
            return value == FieldValue.Literal
                       ? new Domain.Filters.RecurrenceCriterion.LiteralOrSpecialDate(DateTime.Parse(part.ValueLiteral))
                       : new Domain.Filters.RecurrenceCriterion.LiteralOrSpecialDate(value);
        }

        private static TEnum ParseEnum<TEnum>(string value)
        {
            return (TEnum) Enum.Parse(typeof (TEnum), value);
        }
    }
}