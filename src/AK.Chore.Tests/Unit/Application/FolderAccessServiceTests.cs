/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.FolderAccessServiceTests
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

using AK.Chore.Application.Services;
using AK.Chore.Contracts.FolderAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for FolderAccessService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class FolderAccessServiceTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void FolderAccessService_GetFoldersForUser_Interaction_Works()
        {
            var folderAccessService = this.GetTarget();

            var result = folderAccessService.GetFoldersForUser(UserId);

            Assert.IsTrue(result.IsSuccess);

            this.userRepositoryMock.Verify(x => x.Get(UserId));
            this.folderRepositoryMock.Verify(x => x.ListForUser(this.user, this.userRepositoryMock.Object));
            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void FolderAccessService_GetFolder_Interaction_Works()
        {
            var folderAccessService = this.GetTarget();

            var result = folderAccessService.GetFolder(1, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.folderRepositoryMock.Verify(x => x.Get(1, this.userRepositoryMock.Object));
            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void FolderAccessService_SaveFolder_Interaction_Works()
        {
            var folderAccessService = this.GetTarget();

            var folder = new Folder
                {
                    Name = "TestSave",
                    UserId = UserId
                };

            var result = folderAccessService.SaveFolder(folder, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.folderRepositoryMock.Verify(x => x.Save(
                It.Is<Chore.Domain.Folders.Folder>(
                    y =>
                    y.User.Equals(this.user) && y.Name == folder.Name)));

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void FolderAccessService_SaveFolders_Interaction_Works()
        {
            var folderAccessService = this.GetTarget();

            var folder = new Folder
                {
                    Name = "TestSave",
                    UserId = UserId
                };

            var result = folderAccessService.SaveFolders(new[] {folder}, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.folderRepositoryMock.Verify(x => x.Save(
                It.Is<Chore.Domain.Folders.Folder>(
                    y =>
                    y.User.Equals(this.user) && y.Name == folder.Name)));

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void FolderAccessService_DeleteFolder_Interaction_Works()
        {
            var folderAccessService = this.GetTarget();

            var result = folderAccessService.DeleteFolder(1, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.folderRepositoryMock
                .Verify(x => x.Delete(It.Is<Chore.Domain.Folders.Folder>(y => y.User.Equals(this.user)),
                                      this.userRepositoryMock.Object, this.taskRepositoryMock.Object));

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void FolderAccessService_DeleteFolders_Interaction_Works()
        {
            var folderAccessService = this.GetTarget();

            var result = folderAccessService.DeleteFolders(new[] {1}, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.folderRepositoryMock
                .Verify(x => x.Delete(It.Is<Chore.Domain.Folders.Folder>(y => y.User.Equals(this.user)),
                                      this.userRepositoryMock.Object, this.taskRepositoryMock.Object));

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void FolderAccessService_MoveFolder_Interaction_Works()
        {
            var folderAccessService = this.GetTarget();

            var result = folderAccessService.MoveFolder(1, 2, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void FolderAccessService_MoveFolders_Interaction_Works()
        {
            var folderAccessService = this.GetTarget();

            var result = folderAccessService.MoveFolders(new[] {1}, 2, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        private IFolderAccessService GetTarget()
        {
            return new FolderAccessService(
                this.dataAccessMock.Object,
                this.configMock.Object,
                this.logger,
                this.entityIdGeneratorProviderSourceMock.Object);
        }
    }
}