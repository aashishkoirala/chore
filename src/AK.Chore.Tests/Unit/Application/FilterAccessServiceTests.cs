/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Application.FilterAccessServiceTests
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
using AK.Chore.Contracts.FilterAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

#endregion

namespace AK.Chore.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for FilterAccessService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [TestClass]
    public class FilterAccessServiceTests : TestBase
    {
        [TestMethod, TestCategory("Unit.Application")]
        public void FilterAccessService_GetFiltersForUser_Interaction_Works()
        {
            var filterAccessService = this.GetTarget();

            var result = filterAccessService.GetFiltersForUser(UserId);

            Assert.IsTrue(result.IsSuccess);

            this.userRepositoryMock.Verify(x => x.Get(UserId));
            this.filterRepositoryMock.Verify(x => x.ListForUser(this.user));
            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void FilterAccessService_GetFilter_Interaction_Works()
        {
            var filterAccessService = this.GetTarget();

            var result = filterAccessService.GetFilter(1, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.filterRepositoryMock.Verify(x => x.Get(1, this.userRepositoryMock.Object));
            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void FilterAccessService_SaveFilter_Interaction_Works()
        {
            var filterAccessService = this.GetTarget();

            var filter = new Filter
                {
                    Name = "TestSave",
                    UserId = UserId,
                    Criterion = new Criterion {Type = CriterionType.True}
                };

            var result = filterAccessService.SaveFilter(filter, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.filterRepositoryMock.Verify(x => x.Save(
                It.Is<Chore.Domain.Filters.Filter>(
                    y =>
                    y.User.Equals(this.user) && y.Name == filter.Name &&
                    y.Criterion == Chore.Domain.Filters.Criterion.True)));

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void FilterAccessService_SaveFilters_Interaction_Works()
        {
            var filterAccessService = this.GetTarget();

            var filter = new Filter
                {
                    Name = "TestSave",
                    UserId = UserId,
                    Criterion = new Criterion {Type = CriterionType.True}
                };

            var result = filterAccessService.SaveFilters(new[] {filter}, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.filterRepositoryMock.Verify(x => x.Save(
                It.Is<Chore.Domain.Filters.Filter>(
                    y =>
                    y.User.Equals(this.user) && y.Name == filter.Name &&
                    y.Criterion == Chore.Domain.Filters.Criterion.True)));

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void FilterAccessService_DeleteFilter_Interaction_Works()
        {
            var filterAccessService = this.GetTarget();

            var result = filterAccessService.DeleteFilter(1, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.filterRepositoryMock.Verify(x => x.Delete(
                It.Is<Chore.Domain.Filters.Filter>(y => y.User.Equals(this.user))));

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        [TestMethod, TestCategory("Unit.Application")]
        public void FilterAccessService_DeleteFilters_Interaction_Works()
        {
            var filterAccessService = this.GetTarget();

            var result = filterAccessService.DeleteFilters(new[] {1}, UserId);

            Assert.IsTrue(result.IsSuccess);

            this.filterRepositoryMock.Verify(x => x.Delete(
                It.Is<Chore.Domain.Filters.Filter>(y => y.User.Equals(this.user))));

            this.dataAccessMock.Verify();
            this.configMock.Verify();
            this.unitOfWorkMock.Verify();
            this.unitOfWorkFactoryMock.Verify();
        }

        private IFilterAccessService GetTarget()
        {
            return new FilterAccessService(
                this.dataAccessMock.Object,
                this.configMock.Object,
                this.logger,
                this.entityIdGeneratorProviderSourceMock.Object);
        }
    }
}