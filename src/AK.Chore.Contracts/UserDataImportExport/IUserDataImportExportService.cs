/*******************************************************************************************************************************
 * AK.Chore.Contracts.UserDataImportExport.IUserDataImportExportService
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

using AK.Commons.Services;

namespace AK.Chore.Contracts.UserDataImportExport
{
    /// <summary>
    /// Operations to import/export all user data.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IUserDataImportExportService
    {
        /// <summary>
        /// Imports the given user data to the given user's account.
        /// </summary>
        /// <param name="userData">User data to import.</param>
        /// <param name="userId">ID of user to import to.</param>
        /// <returns>Result with status of operation.</returns>
        OperationResults Import(UserData userData, int userId);

        /// <summary>
        /// Exports the given user's data.
        /// </summary>
        /// <param name="userId">ID of user to export.</param>
        /// <returns>Result with exported user data.</returns>
        OperationResult<UserData> Export(int userId);
    }
}