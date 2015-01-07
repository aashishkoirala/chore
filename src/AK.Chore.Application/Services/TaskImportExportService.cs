/*******************************************************************************************************************************
 * AK.Chore.Application.Services.TaskImportExportService
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

using AK.Chore.Application.Aspects;
using AK.Chore.Application.Helpers;
using AK.Chore.Contracts.FolderAccess;
using AK.Chore.Contracts.TaskAccess;
using AK.Chore.Contracts.TaskImportExport;
using AK.Chore.Domain.Folders;
using AK.Chore.Domain.Tasks;
using AK.Chore.Domain.Users;
using AK.Commons;
using AK.Commons.Configuration;
using AK.Commons.DataAccess;
using AK.Commons.DomainDriven;
using AK.Commons.Logging;
using AK.Commons.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Folder = AK.Chore.Contracts.FolderAccess.Folder;
using Task = AK.Chore.Contracts.TaskAccess.Task;

#endregion

namespace AK.Chore.Application.Services
{
    /// <summary>
    /// Service implementation - ITaskImportExportService.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ITaskImportExportService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class TaskImportExportService : ServiceBase, ITaskImportExportService
    {
        private static readonly string[] fieldNames = new[]
            {
                "Description",
                "Folder",
                "Finish By",
                "Start By",
                "Recurring",
                "Recurrence Type",
                "Recurrence Interval",
                "Recurrence Duration",
                "Recurrence Time of Day",
                "Recurrence Day of Month",
                "Recurrence Days of Week",
                "Recurrence Month of Year"
            };

        private readonly ITaskAccessService taskAccessService;
        private readonly IFolderAccessService folderAccessService;
        private readonly ITaskImportParser taskImportParser;
        private readonly ITaskExportFormatter taskExportFormatter;

        [ImportingConstructor]
        public TaskImportExportService(
            [Import] IAppDataAccess appDataAccess,
            [Import] IAppConfig appConfig,
            [Import] IAppLogger logger,
            [Import] IProviderSource<IEntityIdGeneratorProvider> entityIdGeneratorProvider,
            [Import] ITaskAccessService taskAccessService,
            [Import] IFolderAccessService folderAccessService,
            [Import] ITaskImportParser taskImportParser,
            [Import] ITaskExportFormatter taskExportFormatter)
            : base(appDataAccess, appConfig, logger, entityIdGeneratorProvider)
        {
            this.taskAccessService = taskAccessService;
            this.folderAccessService = folderAccessService;
            this.taskImportParser = taskImportParser;
            this.taskExportFormatter = taskExportFormatter;
        }

        [CatchToReturnMany("There was a problem importing those tasks.")]
        public OperationResults<Task> Import(string importData, int userId)
        {
            if (string.IsNullOrWhiteSpace(importData))
            {
                var result = new OperationResult<Task>(TaskImportExportResult.NoTasksToImport);
                return new OperationResults<Task>(new[] {result});
            }

            var lines = importData.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

            var results = this.taskImportParser.ParseLines(lines);
            var validResults = results.Results.Where(x => x.IsSuccess).ToArray();

            var invalidResults = results
                .Results
                .Where(x => !x.IsSuccess)
                .Concat(this.AssignFoldersToTasksToImport(validResults, userId))
                .ToArray();

            validResults = validResults.Except(invalidResults).ToArray();

            var tasksToSave = validResults.Select(x => x.Result).ToArray();
            var saveResults = this.taskAccessService.SaveTasks(tasksToSave, userId);
            var combinedResults = saveResults.Results.Concat(invalidResults);

            return new OperationResults<Task>(combinedResults);
        }

        [CatchToReturn("There was a problem exporting those tasks.")]
        public OperationResult<string> Export(IReadOnlyCollection<int> ids, int userId)
        {
            if (!ids.Any()) return new OperationResult<string>(TaskImportExportResult.NoTasksToExport);

            var exportedData = this.Db.Execute<ITaskRepository, IUserRepository, IFolderRepository, string>(
                (taskRepository, userRepository, folderRepository) =>
                taskRepository
                    .List(ids.ToArray(), userRepository, folderRepository)
                    .Where(x => x.User.Id == userId)
                    .Select(this.taskExportFormatter.FormatTask)
                    .Aggregate((x1, x2) => x1 + Environment.NewLine + x2));

            var header = fieldNames.Aggregate((x1, x2) => x1 + "\t" + x2);
            exportedData = header + Environment.NewLine + exportedData;

            return new OperationResult<string>(exportedData);
        }

        private IEnumerable<OperationResult<Task>> AssignFoldersToTasksToImport(
            OperationResult<Task>[] validResults, int userId)
        {
            var folderPaths = validResults
                .Select(x => x.Result.FolderPath)
                .Distinct()
                .ToArray();

            var folderPathToIdMap = this.GetFolderPathToIdMap(folderPaths, userId);

            foreach (var result in validResults)
            {
                var perhapsFolderId = folderPathToIdMap.LookFor(result.Result.FolderPath);
                if (!perhapsFolderId.IsThere)
                {
                    result.IsSuccess = false;
                    result.ErrorCode = TaskImportExportResult.CannotImportTask.ToString();

                    yield return result;
                }
                else
                {
                    result.Result.Id = 0;
                    result.Result.UserId = userId;
                    result.Result.FolderId = perhapsFolderId;
                }
            }
        }

        private IDictionary<string, int> GetFolderPathToIdMap(IEnumerable<string> folderPaths, int userId)
        {
            var foldersResult = this.folderAccessService.GetFoldersForUser(userId);

            return !foldersResult.IsSuccess
                       ? new Dictionary<string, int>()
                       : FlattenFolderHierarchy(foldersResult.Result)
                             .Where(x => folderPaths.Contains(x.FullPath))
                             .ToDictionary(x => x.FullPath, x => x.Id);
        }

        private static IEnumerable<Folder> FlattenFolderHierarchy(IEnumerable<Folder> folders)
        {
            var foldersEnumerated = folders.ToArray();

            foreach (var folder in foldersEnumerated)
            {
                yield return folder;
                foreach (var child in FlattenFolderHierarchy(folder.Folders))
                    yield return child;
            }
        }
    }
}