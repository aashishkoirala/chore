/*******************************************************************************************************************************
 * AK.Chore.Application.Helpers.UserDataImportValidator
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

using AK.Chore.Application.Mappers;
using AK.Chore.Contracts.UserDataImportExport;
using AK.Chore.Domain;
using AK.Chore.Domain.Filters;
using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Users;
using AK.Commons.DomainDriven;
using AK.Commons.Exceptions;
using AK.Commons.Services;
using System;
using System.ComponentModel.Composition;

#endregion

namespace AK.Chore.Application.Helpers
{
    /// <summary>
    /// Validates user data for import.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IUserDataImportValidator
    {
        /// <summary>
        /// Validates user data for import.
        /// </summary>
        /// <param name="userData">User data being imported.</param>
        /// <param name="user">Fully loaded User domain object.</param>
        /// <returns>Results with validation status.</returns>
        OperationResults Validate(UserData userData, User user);
    }

    [Export(typeof (IUserDataImportValidator)), PartCreationPolicy(CreationPolicy.Shared)]
    public class UserDataImportValidator : IUserDataImportValidator
    {
        private readonly IEntityIdGenerator<int> idGenerator = new TemporaryIdGenerator();

        // ReSharper disable ObjectCreationAsStatement

        public OperationResults Validate(UserData userData, User user)
        {
            try
            {
                return this.ValidateInternal(userData, user);
            }
            catch (Exception ex)
            {
                var result = new OperationResult(UserDataImportExportResult.ImportValidationError, null, ex.Message);
                return new OperationResults(new[] {result});
            }
        }

        private OperationResults ValidateInternal(UserData userData, User user)
        {
            var results = new OperationResults();

            try
            {
                user.Nickname = userData.Nickname;
            }
            catch (DomainValidationException ex)
            {
                results.Results.Add(new OperationResult(ex.Reason, user.Id, ex.Message));
            }

            foreach (var filter in userData.Filters)
            {
                var criterion = filter.Criterion.Map();
                try
                {
                    new Filter(this.idGenerator, filter.Name, user, criterion);
                }
                catch (DomainValidationException ex)
                {
                    results.Results.Add(new OperationResult(ex.Reason, filter.Name, ex.Message));
                    throw;
                }
            }

            foreach (var folder in userData.Folders) this.ValidateFolder(folder, user, null, results);

            return results;
        }

        private void ValidateFolder(UserFolder userFolder, User user, Folder parent, OperationResults results)
        {
            var folder = parent != null
                             ? new Folder(this.idGenerator, userFolder.Name, parent)
                             : new Folder(this.idGenerator, userFolder.Name, user);

            foreach (var task in userFolder.Tasks)
            {
                try
                {
                    task.Map(folder, this.idGenerator);
                }
                catch (DomainValidationException ex)
                {
                    results.Results.Add(new OperationResult(ex.Reason, task.Description, ex.Message));
                }
                catch (GeneralException ex)
                {
                    results.Results.Add(new OperationResult(ex.Reason, task.Description, ex.Message));
                }
            }

            foreach (var child in userFolder.Folders) this.ValidateFolder(child, user, folder, results);
        }

        // ReSharper restore ObjectCreationAsStatement

        private class TemporaryIdGenerator : IEntityIdGenerator<int>
        {
            private int id;

            public int Next<TEntity>() where TEntity : IEntity<TEntity, int>
            {
                return this.id++;
            }
        }
    }
}