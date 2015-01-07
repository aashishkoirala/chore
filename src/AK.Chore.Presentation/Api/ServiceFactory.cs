/*******************************************************************************************************************************
 * AK.Chore.Presentation.Api.ServiceFactory
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

using AK.Commons;
using AK.Commons.Aspects;
using System;
using System.Linq;
using System.Reflection;

#endregion

namespace AK.Chore.Presentation.Api
{
    /// <summary>
    /// Instantiates services.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class ServiceFactory
    {
        public static object GetService(Type type)
        {
            return typeof (ServiceFactory)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(x => x.Name == "GetService" && x.IsGenericMethod)
                .MakeGenericMethod(type)
                .Invoke(null, null);
        }

        public static T GetService<T>() where T : class
        {
            var service = AppEnvironment.Composer.Resolve<T>();
            service = AspectHelper.Wrap(service);

            return service;
        }
    }
}