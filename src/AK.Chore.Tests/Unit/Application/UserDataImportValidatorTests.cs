/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.UserDataImportValidatorTests
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
using AK.Chore.Contracts.FilterAccess;
using AK.Chore.Contracts.UserDataImportExport;
using AK.Chore.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for UserDataImportValidator.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class UserDataImportValidatorTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void UserDataImportValidator_Validate_Works_For_Valid()
        {
            var filter = new UserFilter {Name = "Filter1", Criterion = new Criterion {Type = CriterionType.True}};
            var tasks = new[]
                {
                    new UserTask {Description = "Task1", EndDate = DateTime.Today},
                    new UserTask {Description = "Task2", EndDate = DateTime.Today}
                };
            var folder = new UserFolder {Name = "Folder1", Folders = new UserFolder[0], Tasks = tasks};

            var userData = new UserData
                {
                    Nickname = "TestNickname",
                    Filters = new[] {filter},
                    Folders = new[] {folder}
                };

            var validator = new UserDataImportValidator();

            var results = validator.Validate(userData, this.user);
            Assert.IsTrue(results.IsSuccess);
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void UserDataImportValidator_Validate_Works_For_Invalid()
        {
            var filter = new UserFilter {Name = "Filter1", Criterion = new Criterion {Type = CriterionType.True}};
            var tasks = new[]
                {
                    new UserTask {Description = "Task1", EndDate = DateTime.Today},
                    new UserTask {Description = "Task1", EndDate = DateTime.Today},
                    new UserTask {Description = "Task2", EndTime = TimeSpan.FromHours(4)}
                };
            var folder = new UserFolder {Name = "Folder1", Folders = new UserFolder[0], Tasks = tasks};

            var userData = new UserData
                {
                    Nickname = "TestNickname",
                    Filters = new[] {filter},
                    Folders = new[] {folder}
                };

            var validator = new UserDataImportValidator();

            var results = validator.Validate(userData, this.user);

            Assert.IsFalse(results.IsSuccess);
            Assert.AreEqual(2, results.Results.Count);
            Assert.IsTrue(results.Results.All(x => !x.IsSuccess));
            Assert.AreEqual(results.Results.First().ErrorCode,
                            DomainValidationErrorCode.FolderAttemptToAddExistingTask.ToString());
            Assert.IsTrue(results.Results.Last().Message.Contains("End date not set"));
        }
    }
}