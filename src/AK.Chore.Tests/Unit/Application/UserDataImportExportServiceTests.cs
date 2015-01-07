/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.UserDataImportExportServiceTests
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
using AK.Chore.Contracts.FilterAccess;
using AK.Chore.Contracts.UserDataImportExport;
using AK.Chore.Domain.Tasks;
using AK.Commons.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for UserDataImportExportService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class UserDataImportExportServiceTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void UserDataImportExportService_Import_Interaction_Works()
        {
            var filter = new UserFilter {Name = "Filter1", Criterion = new Criterion {Type = CriterionType.True}};
            var folder = new UserFolder
                {
                    Name = "Folder1",
                    Tasks = new[] {new UserTask {Description = "TestTask", EndDate = DateTime.Today}},
                    Folders = new UserFolder[0]
                };

            var userData = new UserData
                {
                    Nickname = "TestNickName",
                    Filters = new[] {filter},
                    Folders = new[] {folder}
                };

            var taskGrouperMock = new Mock<ITaskGrouper>();

            var validatorMock = new Mock<IUserDataImportValidator>();
            validatorMock
                .Setup(x => x.Validate(userData, this.user))
                .Returns(new OperationResults(new[] {new OperationResult()}))
                .Verifiable();

            var userDataImportExportService = this.GetTarget(taskGrouperMock, validatorMock);

            var results = userDataImportExportService.Import(userData, UserId);

            Assert.IsTrue(results.IsSuccess);

            validatorMock.Verify();
            this.userRepositoryMock.Verify(x => x.Get(UserId), Times.Exactly(2));
            this.filterRepositoryMock.Verify(x => x.ListForUser(this.user));
            this.folderRepositoryMock.Verify(x => x.ListForUser(this.user, this.userRepositoryMock.Object));
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void UserDataImportExportService_Export_Interaction_Works()
        {
            var taskGrouperMock = new Mock<ITaskGrouper>();
            var validatorMock = new Mock<IUserDataImportValidator>();

            var userDataImportExportService = this.GetTarget(taskGrouperMock, validatorMock);

            var result = userDataImportExportService.Export(UserId);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("TestNickName", result.Result.Nickname);
        }

        private IUserDataImportExportService GetTarget(
            IMock<ITaskGrouper> taskGrouperMock, IMock<IUserDataImportValidator> validatorMock)
        {
            return new UserDataImportExportService(
                this.dataAccessMock.Object,
                this.configMock.Object,
                this.logger,
                this.entityIdGeneratorProviderSourceMock.Object,
                taskGrouperMock.Object,
                validatorMock.Object);
        }
    }
}