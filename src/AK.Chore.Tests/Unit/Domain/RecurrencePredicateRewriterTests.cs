/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Domain.RecurrencePredicateRewriterTests
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

using AK.Chore.Domain.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

#endregion

namespace AK.Chore.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests for RecurrencePredicateRewriter.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class RecurrencePredicateRewriterTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Domain")]
        public void RecurrencePredicateRewriter_ReplaceRecurrenceGrouperParameter_Works()
        {
            Expression<Func<Task, bool>> input =
                x => this.recurrenceGrouper.TaskSatisfiesRecurrence(x, DateTime.Today);
            var output = this.recurrencePredicateRewriter.ReplaceRecurrenceGrouperParameter(input);
            output.Compile();
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void RecurrencePredicateRewriter_ReplaceRecurrenceGrouperCall_Works()
        {
            Expression<Func<Task, bool>> input =
                x => this.recurrenceGrouper.TaskSatisfiesRecurrence(x, DateTime.Today);
            var output = this.recurrencePredicateRewriter.ReplaceRecurrenceGrouperCall(input);
            output.Compile();
        }
    }
}