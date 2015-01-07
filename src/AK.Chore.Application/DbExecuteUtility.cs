/*******************************************************************************************************************************
 * AK.Chore.Application.DbExecuteUtility
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

using AK.Commons.DataAccess;
using AK.Commons.DomainDriven;
using System;

#endregion

namespace AK.Chore.Application
{
    /// <summary>
    /// Convenience wrappers for DDD-aware Unit-of-Work factory execute methods.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class DbExecuteUtility
    {
        public static void Execute<T>(this IUnitOfWorkFactory db, Action<T> action)
            where T : IDomainRepository
        {
            db.With<T>()
              .Execute(map => action(map.Get<T>()));
        }

        public static void Execute<T1, T2>(this IUnitOfWorkFactory db, Action<T1, T2> action)
            where T1 : IDomainRepository
            where T2 : IDomainRepository
        {
            db.With<T1>()
              .With<T2>()
              .Execute(map => action(map.Get<T1>(), map.Get<T2>()));
        }

        public static void Execute<T1, T2, T3>(this IUnitOfWorkFactory db, Action<T1, T2, T3> action)
            where T1 : IDomainRepository
            where T2 : IDomainRepository
            where T3 : IDomainRepository
        {
            db.With<T1>()
              .With<T2>()
              .With<T3>()
              .Execute(map => action(map.Get<T1>(), map.Get<T2>(), map.Get<T3>()));
        }

        public static void Execute<T1, T2, T3, T4>(this IUnitOfWorkFactory db, Action<T1, T2, T3, T4> action)
            where T1 : IDomainRepository
            where T2 : IDomainRepository
            where T3 : IDomainRepository
            where T4 : IDomainRepository
        {
            db.With<T1>()
              .With<T2>()
              .With<T3>()
              .With<T4>()
              .Execute(map => action(map.Get<T1>(), map.Get<T2>(), map.Get<T3>(), map.Get<T4>()));
        }

        public static TResult Execute<T, TResult>(this IUnitOfWorkFactory db, Func<T, TResult> action)
            where T : IDomainRepository
        {
            return db.With<T>()
                     .Execute(map => action(map.Get<T>()));
        }

        public static TResult Execute<T1, T2, TResult>(this IUnitOfWorkFactory db, Func<T1, T2, TResult> action)
            where T1 : IDomainRepository
            where T2 : IDomainRepository
        {
            return db.With<T1>()
                     .With<T2>()
                     .Execute(map => action(map.Get<T1>(), map.Get<T2>()));
        }

        public static TResult Execute<T1, T2, T3, TResult>(this IUnitOfWorkFactory db, Func<T1, T2, T3, TResult> action)
            where T1 : IDomainRepository
            where T2 : IDomainRepository
            where T3 : IDomainRepository
        {
            return db.With<T1>()
                     .With<T2>()
                     .With<T3>()
                     .Execute(map => action(map.Get<T1>(), map.Get<T2>(), map.Get<T3>()));
        }

        public static TResult Execute<T1, T2, T3, T4, TResult>(this IUnitOfWorkFactory db,
                                                               Func<T1, T2, T3, T4, TResult> action)
            where T1 : IDomainRepository
            where T2 : IDomainRepository
            where T3 : IDomainRepository
            where T4 : IDomainRepository
        {
            return db.With<T1>()
                     .With<T2>()
                     .With<T3>()
                     .With<T4>()
                     .Execute(map => action(map.Get<T1>(), map.Get<T2>(), map.Get<T3>(), map.Get<T4>()));
        }
    }
}