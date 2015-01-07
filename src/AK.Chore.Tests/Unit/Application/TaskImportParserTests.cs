/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.TaskImportParserTests
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

using AK.Chore.Application.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for TaskImportParser.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class TaskImportParserTests
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void TaskImportParser_ParseLines_Generally_Works()
        {
            var parser = new TaskImportParser();

            var lines = new[]
                {
                    "Description\tFolder\tFinish By\tStart By\tRecurring\tRecurrence Type\tRecurrence Interval" +
                    "\tRecurrence Duration\tRecurrence Time of Day\tRecurrence Day of Month\t" +
                    "Recurrence Days of Week\tRecurrence Month of Year",
                    "Test task\tPersonal\t2015-01-05\t\tNo\t\t\t\t\t\t\t\t",
                    "Test recurring task\tPersonal\t\t\tYes\tHourly\t3\t02:00\t00:00\t0"
                };

            var result = parser.ParseLines(lines);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(lines.Length - 1, result.Results.Count);
        }
    }
}