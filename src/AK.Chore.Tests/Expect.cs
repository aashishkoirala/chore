/*******************************************************************************************************************************
 * AK.Chore.Tests.Expect
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

using AK.Commons.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

#endregion

namespace AK.Chore.Tests
{
    /// <summary>
    /// Expands upon the MS-Test Assert mechanism to provide more convenience testing methods.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class Expect
    {
        /// <summary>
        /// Executes the given action and asserts that the given type of exception is thrown.
        /// </summary>
        /// <typeparam name="TException">Exception to be thrown.</typeparam>
        /// <param name="action">Action to be executed.</param>
        public static void Error<TException>(Action action)
        {
            var failMessage = string.Format("Was expecting exception of type {0} to be thrown, but didn't.",
                                            typeof (TException).Name);

            try
            {
                action();
                Assert.Fail(failMessage);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.GetType(), typeof (TException), failMessage);
            }
        }

        /// <summary>
        /// Executes the given action and asserts that the given type of exception is thrown. The
        /// exception must be of type ReasonedException.
        /// </summary>
        /// <typeparam name="TException">Exception to be thrown.</typeparam>
        /// <typeparam name="TErrorCode">Type of errorCode parameter.</typeparam>
        /// <param name="errorCode">ErrorCode to be expected from the thrown exception.</param>
        /// <param name="action">Action to be executed.</param>
        public static void Error<TException, TErrorCode>(TErrorCode errorCode, Action action)
            where TErrorCode : struct
            where TException : ReasonedException<TErrorCode>
        {
            var failMessage = string.Format(
                "Was expecting exception of type {0} to be thrown with error code {1}, but didn't.",
                typeof (TException).Name, errorCode);

            try
            {
                action();
                Assert.Fail(failMessage);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.GetType(), typeof (TException), failMessage);
                Assert.IsNotNull(ex as ReasonedException<TErrorCode>, failMessage);
                Assert.AreEqual((ex as ReasonedException<TErrorCode>).Reason, errorCode, failMessage);
            }
        }
    }
}