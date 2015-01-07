/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Domain.RecurrenceTests
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

using System;
using AK.Chore.Domain.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace AK.Chore.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests for Recurrence.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class RecurrenceTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Domain")]
        public void Recurrence_Creation_Hourly_Works()
        {
            var recurrence = Recurrence.Hourly(2);
            Assert.AreEqual(recurrence.Type, RecurrenceType.Hourly);
            Assert.AreEqual(recurrence.Interval, 2);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Recurrence_Creation_Daily_Works()
        {
            var recurrence = Recurrence.Daily(2);
            Assert.AreEqual(recurrence.Type, RecurrenceType.Daily);
            Assert.AreEqual(recurrence.Interval, 2);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Recurrence_Creation_Weekly_Works()
        {
            var recurrence = Recurrence.Weekly(2, new[] {DayOfWeek.Monday, DayOfWeek.Friday});
            Assert.AreEqual(recurrence.Type, RecurrenceType.Weekly);
            Assert.AreEqual(recurrence.Interval, 2);
            Assert.AreEqual(recurrence.DaysOfWeek[0], DayOfWeek.Monday);
            Assert.AreEqual(recurrence.DaysOfWeek[1], DayOfWeek.Friday);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Recurrence_Creation_Monthly_Works()
        {
            var recurrence = Recurrence.Monthly(2, 10);
            Assert.AreEqual(recurrence.Type, RecurrenceType.Monthly);
            Assert.AreEqual(recurrence.Interval, 2);
            Assert.AreEqual(recurrence.DayOfMonth, 10);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Recurrence_Creation_Yearly_Works()
        {
            var recurrence = Recurrence.Yearly(10, 4);
            Assert.AreEqual(recurrence.Type, RecurrenceType.Yearly);
            Assert.AreEqual(recurrence.Interval, 10);
            Assert.AreEqual(recurrence.MonthOfYear, 4);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Recurrence_Creation_NonRecurring_Works()
        {
            var recurrence = Recurrence.NonRecurring();
            Assert.AreEqual(recurrence.Type, RecurrenceType.NonRecurring);
        }

        [TestMethod, TestCategory("Unit.Domain")]
        public void Recurrence_Creation_Enabled_Disabled_Works()
        {
            var originalRecurrence = Recurrence.NonRecurring();
            Assert.IsTrue(originalRecurrence.IsEnabled);
            var recurrence = Recurrence.Disabled(originalRecurrence);
            Assert.IsFalse(recurrence.IsEnabled);
            recurrence = Recurrence.Enabled(recurrence);
            Assert.IsTrue(recurrence.IsEnabled);
            Assert.AreEqual(recurrence, originalRecurrence);
        }
    }
}