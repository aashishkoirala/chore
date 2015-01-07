/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.CalendarRecurrenceCalculatorTests
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
using Task = AK.Chore.Domain.Tasks.Task;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for CalendarRecurrenceCalculator.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class CalendarRecurrenceCalculatorTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void CalendarRecurrenceCalculator_GetCalendarTaskRecurrenceMap_Generally_Works()
        {
            var calendarRecurrenceCalculator = new CalendarRecurrenceCalculator();

            var date = new DateTime(2015, 1, 5);
            var weekStart = date.AddDays(-6);
            var weekEnd = date;

            var folder = new Chore.Domain.Folders.Folder(this.IdGenerator, "Folder1", this.user);

            var tasks = new[]
                {
                    new Task(new StaticIdGenerator(1), "Task1", folder, Chore.Domain.Tasks.Recurrence.Daily(1)),
                    new Task(new StaticIdGenerator(2), "Task2", folder, Chore.Domain.Tasks.Recurrence.Hourly(1)),
                    new Task(new StaticIdGenerator(3), "Task3", folder,
                             Chore.Domain.Tasks.Recurrence.Weekly(1, new[] {DateTime.Today.DayOfWeek}))
                };

            var recurrenceMap = calendarRecurrenceCalculator.GetCalendarTaskRecurrenceMap(tasks, weekStart, weekEnd);

            Assert.AreEqual(3, recurrenceMap.Count);
            Assert.AreEqual(7, recurrenceMap[1].Length);
            Assert.AreEqual(145, recurrenceMap[2].Length);
            Assert.AreEqual(1, recurrenceMap[3].Length);
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