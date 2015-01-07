/*******************************************************************************************************************************
 * AK.Chore.Application.Aspects.CatchToReturnAttribute
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

using AK.Chore.Contracts;
using AK.Chore.Domain;
using AK.Commons;
using AK.Commons.Aspects;
using AK.Commons.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace AK.Chore.Application.Aspects
{
    /// <summary>
    /// Catches exceptions in service methods and wraps them in OperationResult.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class CatchToReturnAttribute : Attribute, IErrorAspect
    {
        private readonly string message;

        public int Order
        {
            get { return 1; }
        }

        public CatchToReturnAttribute(string message = null)
        {
            this.message = message;
        }

        public bool Execute(
            MemberInfo memberInfo,
            IDictionary<string, object> parameters,
            ref Exception ex,
            ref object returnValue)
        {
            var method = memberInfo as MethodInfo;
            if (method == null) return true;

            var result = ex is DomainValidationException ? GeneralResult.InvalidRequest : GeneralResult.Error;

            AppEnvironment.Logger.Error(ex);
            if (method.ReturnType == typeof (OperationResult))
            {
                returnValue = new OperationResult(result, this.message);
                return false;
            }

            var resultType = typeof (OperationResult<>).MakeGenericType(method.ReturnType.GetGenericArguments()[0]);
            var constructor = resultType.GetConstructor(new[] {typeof (Enum), typeof (string)});
            if (constructor == null) return true;

            returnValue = constructor.Invoke(new object[] {result, this.message});
            return false;
        }
    }
}