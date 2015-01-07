/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.TaskExportFormatterTests
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
using System;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for TaskExportFormatter.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class TaskExportFormatterTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void TaskExportFormatter_FormatTask_Works_For_Non_Recurring()
        {
            var formatter = new TaskExportFormatter();

            var folder = new Chore.Domain.Folders.Folder(this.IdGenerator, "TestFolder", this.user);
            var task = new Chore.Domain.Tasks.Task(this.IdGenerator, "Test", folder, new DateTime(2015, 1, 5));
            var result = formatter.FormatTask(task);

            Assert.IsTrue(result.StartsWith("Test\tTestFolder\t2015-01-05\t\tNo"));
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskExportFormatter_FormatTask_Works_For_Recurring()
        {
            var formatter = new TaskExportFormatter();

            var folder = new Chore.Domain.Folders.Folder(this.IdGenerator, "TestFolder", this.user);
            var task = new Chore.Domain.Tasks.Task(this.IdGenerator, "Test", folder,
                                                   Chore.Domain.Tasks.Recurrence.Daily(2));
            var result = formatter.FormatTask(task);

            Assert.IsTrue(result.StartsWith("Test\tTestFolder\t\t\tYes\tDaily\t2\t01:00\t"));
        }
    }
}