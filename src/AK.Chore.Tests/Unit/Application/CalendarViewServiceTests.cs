/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.CalendarViewServiceTests
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
using AK.Chore.Application.Services;
using AK.Chore.Contracts.CalendarView;
using AK.Chore.Domain.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using Task = AK.Chore.Domain.Tasks.Task;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for CalendarViewService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class CalendarViewServiceTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void CalendarViewService_GetCalendarWeekForUser_Interaction_Works()
        {
            var expectedCalendarWeek = new CalendarWeek();

            var recurrenceGrouperMock = new Mock<IRecurrenceGrouper>();

            var calendarRecurrenceCalculatorMock = new Mock<ICalendarRecurrenceCalculator>();
            calendarRecurrenceCalculatorMock
                .Setup(
                    x =>
                    x.GetCalendarTaskRecurrenceMap(It.IsAny<IEnumerable<Task>>(), It.IsAny<DateTime>(),
                                                   It.IsAny<DateTime>()))
                .Returns(new Dictionary<int, DateTime[]>())
                .Verifiable();

            var calendarBuilderMock = new Mock<ICalendarBuilder>();
            calendarBuilderMock
                .Setup(x =>
                       x.BuildCalendarWeek(It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                                           It.IsAny<IReadOnlyCollection<Task>>(),
                                           It.IsAny<IDictionary<int, DateTime[]>>()))
                .Returns(expectedCalendarWeek)
                .Verifiable();

            var calendarViewService = new CalendarViewService(
                this.dataAccessMock.Object,
                this.configMock.Object,
                this.logger,
                this.entityIdGeneratorProviderSourceMock.Object,
                recurrenceGrouperMock.Object,
                calendarRecurrenceCalculatorMock.Object,
                calendarBuilderMock.Object);

            var result = calendarViewService.GetCalendarWeekForUser(DateTime.Today, 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreSame(expectedCalendarWeek, result.Result);

            this.taskRepositoryMock.Verify();
            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }
    }
}