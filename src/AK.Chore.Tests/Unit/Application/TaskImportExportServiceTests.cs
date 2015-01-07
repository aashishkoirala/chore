/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.TaskImportExportServiceTests
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
using AK.Chore.Contracts.FolderAccess;
using AK.Chore.Contracts.TaskAccess;
using AK.Chore.Contracts.TaskImportExport;
using AK.Commons.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = AK.Chore.Contracts.TaskAccess.Task;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for TaskImportExportService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class TaskImportExportServiceTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void TaskImportExportService_Import_Interaction_Works()
        {
            var taskAccessServiceMock = new Mock<ITaskAccessService>();
            taskAccessServiceMock
                .Setup(x => x.SaveTasks(It.Is<Task[]>(y => y.Length == 2), UserId))
                .Returns<IReadOnlyCollection<Task>, int>(
                    (x, i) => new OperationResults<Task>(x.Select(y => new OperationResult<Task>(y))))
                .Verifiable();

            var folderAccessServiceMock = new Mock<IFolderAccessService>();
            folderAccessServiceMock
                .Setup(x => x.GetFoldersForUser(UserId))
                .Returns(
                    new OperationResult<IReadOnlyCollection<Folder>>(new[]
                        {
                            new Folder
                                {
                                    Id = 1,
                                    UserId = UserId,
                                    Name = "Test",
                                    FullPath = "Test",
                                    Folders = new Folder[0]
                                }
                        }))
                .Verifiable();

            var taskImportParserMock = new Mock<ITaskImportParser>();
            taskImportParserMock
                .Setup(x => x.ParseLines(It.Is<string[]>(y => y.Length == 2)))
                .Returns(new OperationResults<Task>(new[]
                    {
                        new OperationResult<Task>(new Task
                            {
                                Id = 1,
                                Description = "1",
                                UserId = UserId,
                                FolderPath = "Test"
                            }),
                        new OperationResult<Task>(new Task
                            {
                                Id = 2,
                                Description = "2",
                                UserId = UserId,
                                FolderPath = "Test"
                            })
                    }))
                .Verifiable();

            var taskExportFormatterMock = new Mock<ITaskExportFormatter>();

            var taskImportExportService = this.GetTarget(
                taskAccessServiceMock, folderAccessServiceMock,
                taskImportParserMock, taskExportFormatterMock);

            var result = taskImportExportService.Import("TestData\r\nTestData", UserId);

            Assert.IsTrue(result.IsSuccess);

            taskAccessServiceMock.Verify();
            folderAccessServiceMock.Verify();
            taskImportParserMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void TaskImportExportService_Export_Interaction_Works()
        {
            var taskAccessServiceMock = new Mock<ITaskAccessService>();
            var folderAccessServiceMock = new Mock<IFolderAccessService>();
            var taskImportParserMock = new Mock<ITaskImportParser>();

            var taskExportFormatterMock = new Mock<ITaskExportFormatter>();
            taskExportFormatterMock
                .Setup(x => x.FormatTask(It.IsAny<Chore.Domain.Tasks.Task>()))
                .Returns("Test")
                .Verifiable();

            var ids = new[] {1, 2};
            var folder = new Chore.Domain.Folders.Folder(this.IdGenerator, "ExportTaskTestFolder", this.user);

            this.taskRepositoryMock
                .Setup(x => x.List(ids, this.userRepositoryMock.Object, this.folderRepositoryMock.Object))
                .Returns(new[]
                    {
                        new Chore.Domain.Tasks.Task(this.IdGenerator, "Test1", folder, DateTime.Today),
                        new Chore.Domain.Tasks.Task(this.IdGenerator, "Test2", folder, DateTime.Today)
                    })
                .Verifiable();

            var taskImportExportService = this.GetTarget(
                taskAccessServiceMock, folderAccessServiceMock,
                taskImportParserMock, taskExportFormatterMock);

            var result = taskImportExportService.Export(ids, UserId);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Result.Contains("Test\r\nTest"));

            taskExportFormatterMock.Verify();
            this.taskRepositoryMock.Verify(
                x => x.List(ids, this.userRepositoryMock.Object, this.folderRepositoryMock.Object));
        }

        public ITaskImportExportService GetTarget(
            IMock<ITaskAccessService> taskAccessServiceMock,
            IMock<IFolderAccessService> folderAccessServiceMock,
            IMock<ITaskImportParser> taskImportParserMock,
            IMock<ITaskExportFormatter> taskExportFormatterMock)
        {
            return new TaskImportExportService(
                this.dataAccessMock.Object,
                this.configMock.Object,
                this.logger,
                this.entityIdGeneratorProviderSourceMock.Object,
                taskAccessServiceMock.Object,
                folderAccessServiceMock.Object,
                taskImportParserMock.Object,
                taskExportFormatterMock.Object);
        }
    }
}