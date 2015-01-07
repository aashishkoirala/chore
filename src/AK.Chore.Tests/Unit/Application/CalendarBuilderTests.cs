/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.CalendarBuilderTests
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
using AK.Commons.DomainDriven;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = AK.Chore.Domain.Tasks.Task;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for CalendarBuilder.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class CalendarBuilderTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void CalendarBuilder_BuildCalendarWeek_Generally_Works()
        {
            var calendarBuilder = new CalendarBuilder();

            var weekStart = DateTime.Today.AddDays(-6);
            var weekEnd = DateTime.Today;

            var folder = new Chore.Domain.Folders.Folder(this.IdGenerator, "Folder1", this.user);

            var tasks = new[]
                {
                    new Task(new StaticIdGenerator(1), "Task1", folder, DateTime.Today),
                    new Task(new StaticIdGenerator(2), "Task2", folder, Chore.Domain.Tasks.Recurrence.Daily(1)),
                    new Task(new StaticIdGenerator(3), "Task3", folder, Chore.Domain.Tasks.Recurrence.Hourly(1)),
                    new Task(new StaticIdGenerator(4), "Task4", folder,
                             Chore.Domain.Tasks.Recurrence.Weekly(1, new[] {DateTime.Today.DayOfWeek}))
                };

            var recurrenceMap = new Dictionary<int, DateTime[]>
                {
                    {2, Enumerable.Range(0, 6).Select(x => DateTime.Today.AddDays(x)).ToArray()},
                    {3, Enumerable.Range(0, 176).Select(x => DateTime.Today.AddHours(x)).ToArray()},
                    {4, new[] {DateTime.Today, DateTime.Today.AddDays(7)}}
                };

            var calendarWeek = calendarBuilder.BuildCalendarWeek(weekStart, weekEnd, tasks, recurrenceMap);

            Assert.AreEqual(weekStart, calendarWeek.StartDate);
            Assert.AreEqual(weekEnd, calendarWeek.EndDate);
            Assert.AreEqual(7, calendarWeek.Days.Count);

            var dayItems = calendarWeek.Days.SelectMany(x => x.DayItems).ToArray();
            var hourItems = calendarWeek.Days.SelectMany(x => x.Hours).SelectMany(x => x.Items).ToArray();

            Assert.AreEqual(1, dayItems[0].TaskId);
            Assert.AreEqual("Task1", dayItems[0].Description);
            Assert.AreEqual(2, dayItems[1].TaskId);
            Assert.AreEqual("Task2", dayItems[1].Description);
            Assert.AreEqual(4, dayItems[2].TaskId);
            Assert.AreEqual("Task4", dayItems[2].Description);

            Assert.AreEqual(24, hourItems.Length);
            foreach (var hourItem in hourItems)
            {
                Assert.AreEqual(3, hourItem.TaskId);
                Assert.AreEqual("Task3", hourItem.Description);
            }
        }

        private class StaticIdGenerator : IEntityIdGenerator<int>
        {
            private readonly int id;

            public StaticIdGenerator(int id)
            {
                this.id = id;
            }

            public int Next<TEntity>() where TEntity : IEntity<TEntity, int>
            {
                return this.id;
            }
        }
    }
}