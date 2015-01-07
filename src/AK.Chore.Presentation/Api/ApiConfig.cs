/*******************************************************************************************************************************
 * AK.Chore.Presentation.Api.ApiConfig
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

using AK.Chore.Contracts.CalendarView;
using AK.Chore.Contracts.FilterAccess;
using AK.Chore.Contracts.FolderAccess;
using AK.Chore.Contracts.TaskAccess;
using AK.Chore.Contracts.TaskImportExport;
using AK.Chore.Contracts.UserDataImportExport;
using AK.Chore.Contracts.UserProfile;
using AK.Commons.Services;
using AK.Commons.Web.ResourceMapping;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

#endregion

namespace AK.Chore.Presentation.Api
{
    /// <summary>
    /// Web API configuration - resource mapping, REST configuration, etc.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class ApiConfig
    {
        public static void Configure()
        {
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                "ResourceApi", "api/{resource}/{id}",
                new {controller = "resource", id = RouteParameter.Optional});

            var serializerSettings = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings;
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            serializerSettings.DateFormatString = "MMM dd, yyyy";
            serializerSettings.Converters.Add(new StringEnumConverter {CamelCaseText = true});

            var resourceMap = ResourceMap
                .User(x => x.GetUserId())
                .ServiceFactory(ServiceFactory.GetService)
                .StatusCode(StatusCodeMap.GetStatusCode)

                .Resource<Folder, IFolderAccessService, int, EmptyQuery>("folder")
                .Get(x => x.Service.GetFolder(x.Id, x.UserId))
                .Post(x =>
                    {
                        x.Resource.Id = 0;
                        x.Resource.UserId = x.UserId;
                        return x.Service.SaveFolder(x.Resource, x.UserId);
                    })
                .Put(x => x.Service.SaveFolder(x.Resource, x.UserId))
                .Delete(x => x.Service.DeleteFolder(x.Id, x.UserId))
                .GetList(x => x.Service.GetFoldersForUser(x.UserId))
                .PostList(x => x.Service.SaveFolders(x.Resources, x.UserId))
                .PutList(x => x.Service.SaveFolders(x.Resources, x.UserId))
                .DeleteList(x => x.Service.DeleteFolders(x.Ids, x.UserId))

                .Resource<Filter, IFilterAccessService, int, EmptyQuery>("filter")
                .Get(x => x.Service.GetFilter(x.Id, x.UserId))
                .Post(x =>
                    {
                        x.Resource.Id = 0;
                        x.Resource.UserId = x.UserId;
                        return x.Service.SaveFilter(x.Resource, x.UserId);
                    })
                .Put(x => x.Service.SaveFilter(x.Resource, x.UserId))
                .Delete(x => x.Service.DeleteFilter(x.Id, x.UserId))
                .GetList(x => x.Service.GetFiltersForUser(x.UserId))
                .PostList(x => x.Service.SaveFilters(x.Resources, x.UserId))
                .PutList(x => x.Service.SaveFilters(x.Resources, x.UserId))
                .DeleteList(x => x.Service.DeleteFilters(x.Ids, x.UserId))

                .Resource<Task, ITaskFacade, int, TaskQuery>("task")
                .Get(x => x.Service.Access.GetTask(x.Id, x.UserId, x.Context.GetNow()))
                .Post(x => x.Service.AddTask(x.Resource, x.UserId))
                .Put(x => x.Service.Access.SaveTask(x.Resource, x.UserId))
                .Delete(x => x.Service.Access.DeleteTask(x.Id, x.UserId))
                .GetList(x => x.Service.GetTasks(x.Query, x.UserId, x.Context))
                .PostList(x => x.Service.AddTasks(x.Resources, x.UserId))
                .PutList(x => x.Service.Access.SaveTasks(x.Resources, x.UserId))
                .DeleteList(x => x.Service.Access.DeleteTasks(x.Ids, x.UserId))

                .Resource<TaskMoveCommand, ITaskFacade, int, EmptyQuery>("movetask")
                .Post(x => x.Service.MoveTask(x.Resource, x.UserId))

                .Resource<TaskMoveCommand, ITaskFacade, int, EmptyQuery>("movetasks")
                .Post(x => x.Service.MoveTasks(x.Resource, x.UserId))

                .Resource<Task, ITaskFacade, int, TaskQuery>("starttask")
                .Post(x => x.Service.Flow.Start(x.Resource.Id, x.UserId, x.Context.GetNow()))
                .PostList(x => x.Service.Flow.StartAll(
                    x.Resources.Select(y => y.Id).ToArray(), x.UserId, x.Context.GetNow()))

                .Resource<Task, ITaskFacade, int, TaskQuery>("pausetask")
                .Post(x => x.Service.Flow.Pause(x.Resource.Id, x.UserId, x.Context.GetNow()))
                .PostList(x => x.Service.Flow.PauseAll(
                    x.Resources.Select(y => y.Id).ToArray(), x.UserId, x.Context.GetNow()))

                .Resource<Task, ITaskFacade, int, TaskQuery>("resumetask")
                .Post(x => x.Service.Flow.Resume(x.Resource.Id, x.UserId, x.Context.GetNow()))
                .PostList(x => x.Service.Flow.ResumeAll(
                    x.Resources.Select(y => y.Id).ToArray(), x.UserId, x.Context.GetNow()))

                .Resource<Task, ITaskFacade, int, TaskQuery>("completetask")
                .Post(x => x.Service.Flow.Complete(x.Resource.Id, x.UserId, x.Context.GetNow()))
                .PostList(x => x.Service.Flow.CompleteAll(
                    x.Resources.Select(y => y.Id).ToArray(), x.UserId, x.Context.GetNow()))

                .Resource<Task, ITaskFacade, int, TaskQuery>("enabletaskrecurrence")
                .Post(x => x.Service.Flow.EnableRecurrence(x.Resource.Id, x.UserId, x.Context.GetNow()))
                .PostList(x => x.Service.Flow.EnableRecurrenceForAll(
                    x.Resources.Select(y => y.Id).ToArray(), x.UserId, x.Context.GetNow()))

                .Resource<Task, ITaskFacade, int, TaskQuery>("disabletaskrecurrence")
                .Post(x => x.Service.Flow.DisableRecurrence(x.Resource.Id, x.UserId, x.Context.GetNow()))
                .PostList(x => x.Service.Flow.DisableRecurrenceForAll(
                    x.Resources.Select(y => y.Id).ToArray(), x.UserId, x.Context.GetNow()))

                .Resource<CalendarWeek, ICalendarViewService, DateTime, EmptyQuery>("calendar")
                .Get(x => x.Service.GetCalendarWeekForUser(x.Id, x.UserId))

                .Resource<TaskSatisfiesFilterCommand, ITaskFacade, int, EmptyQuery>("tasksatisfiesfilter")
                .Post(x => x.Service.TaskSatisfiesFilter(x.Resource, x.UserId, x.Context))

                .Resource<Filter, ITaskFacade, int, EmptyQuery>("unsavedfilter")
                .Get(x => new OperationResult<Filter>(x.Context.GetFilter()))
                .GetList(x => new OperationResult<IReadOnlyCollection<Filter>>(new[] {x.Context.GetFilter()}))
                .Post(x =>
                    {
                        x.Resource.Name = "Unsaved Filter";
                        x.Resource.UserId = x.UserId;
                        x.Context.SetFilter(x.Resource);

                        return new OperationResult<Filter>(x.Resource);
                    })

                .Resource<Folder, ITaskFacade, int, EmptyQuery>("selectedfolders")
                .GetList(x =>
                    {
                        var folders = x.Context.GetFolderIds()
                                       .Select(y => new Folder {Id = y})
                                       .ToArray();

                        return new OperationResult<IReadOnlyCollection<Folder>>(folders);
                    })
                .PostList(x =>
                    {
                        x.Context.SetFolderIds(x.Resources.Select(y => y.Id).ToArray());
                        return new OperationResults<Folder>(x.Resources.Select(y => new OperationResult<Folder>(y)));
                    })

                .Resource<FolderMoveCommand, IFolderAccessService, int, EmptyQuery>("movefolder")
                .Post(x =>
                    {
                        var result = x.Service.MoveFolder(x.Resource.FolderId, x.Resource.MoveToFolderId, x.UserId);
                        return result.IsSuccess
                                   ? new OperationResult<FolderMoveCommand>(x.Resource)
                                   : new OperationResult<FolderMoveCommand>(result);
                    })

                .Resource<TaskImportCommand, ITaskImportExportService, int, EmptyQuery>("importtasks")
                .Post(x =>
                      new OperationResult<TaskImportCommand>(new TaskImportCommand
                          {
                              ImportResults = x.Service.Import(x.Resource.ImportData, x.UserId)
                          }))

                .Resource<TaskExportCommand, ITaskImportExportService, int, EmptyQuery>("exporttasks")
                .Post(x =>
                    {
                        var result = x.Service.Export(x.Resource.TaskIds, x.UserId);
                        if (!result.IsSuccess) return new OperationResult<TaskExportCommand>(result);

                        x.Resource.ExportedData = result.Result;
                        return new OperationResult<TaskExportCommand>(x.Resource);
                    })

                .Resource<User, IUserProfileService, int, EmptyQuery>("user")
                .Delete(x => x.Service.DeleteUserProfile(new User {Id = x.UserId}, x.UserId))

                .Resource<UpdateUserNicknameCommand, IUserProfileService, int, object>("usernickname")
                .Get(
                    x =>
                    new OperationResult<UpdateUserNicknameCommand>(new UpdateUserNicknameCommand
                        {
                            Nickname = x.Context.GetNickname()
                        }))
                .Put(x =>
                    {
                        var result = x.Service.UpdateNickname(
                            new User {Id = x.UserId, Nickname = x.Resource.Nickname},
                            x.UserId);

                        if (result.IsSuccess)
                        {
                            x.Context.SetNickname(x.Resource.Nickname);
                            return new OperationResult<UpdateUserNicknameCommand>(x.Resource);
                        }
                        return new OperationResult<UpdateUserNicknameCommand>(result);
                    })

                .Resource<UserData, IUserDataImportExportService, int, EmptyQuery>("userdata")
                .Get(x => x.Service.Export(x.UserId))
                .GetList(x =>
                    {
                        var result = x.Service.Export(x.UserId);
                        if (!result.IsSuccess) return new OperationResult<IReadOnlyCollection<UserData>>(result);

                        var results = new[] {result.Result};
                        return new OperationResult<IReadOnlyCollection<UserData>>(results);
                    })
                .PostList(x =>
                    {
                        var results = x.Service.Import(x.Resources.Single(), x.UserId);
                        return new OperationResults<UserData>(
                            results.Results.Select(y => new OperationResult<UserData>(y)));
                    })
                .Links("link");

            ResourceMapConfiguration.Provider = resourceMap.GetResourceProvider();

            ResourceMapConfiguration
                .Route("ResourceApi")
                .Link("getLinks", "link", HttpMethod.Get, true)
                .Link("getTask", "task", HttpMethod.Get)
                .Link("getTasks", "task", HttpMethod.Get, true)
                .Link("addTask", "task", HttpMethod.Post)
                .Link("addTasks", "task", HttpMethod.Post, true)
                .Link("updateTask", "task", HttpMethod.Put)
                .Link("updateTasks", "task", HttpMethod.Put, true)
                .Link("deleteTask", "task", HttpMethod.Delete)
                .Link("deleteTasks", "task", HttpMethod.Delete, true)
                .Link("moveTask", "movetask", HttpMethod.Post)
                .Link("moveTasks", "movetasks", HttpMethod.Post)
                .Link("getFolder", "folder", HttpMethod.Get)
                .Link("getFolders", "folder", HttpMethod.Get, true)
                .Link("addFolder", "folder", HttpMethod.Post)
                .Link("addFolders", "folder", HttpMethod.Post, true)
                .Link("updateFolder", "folder", HttpMethod.Put)
                .Link("updateFolders", "folder", HttpMethod.Put, true)
                .Link("deleteFolder", "folder", HttpMethod.Delete)
                .Link("deleteFolders", "folder", HttpMethod.Delete, true)
                .Link("moveFolder", "movefolder", HttpMethod.Post)
                .Link("getFilter", "filter", HttpMethod.Get)
                .Link("getFilters", "filter", HttpMethod.Get, true)
                .Link("addFilter", "filter", HttpMethod.Post)
                .Link("addFilters", "filter", HttpMethod.Post, true)
                .Link("updateFilter", "filter", HttpMethod.Put)
                .Link("updateFilters", "filter", HttpMethod.Put, true)
                .Link("deleteFilter", "filter", HttpMethod.Delete)
                .Link("deleteFilters", "filter", HttpMethod.Delete, true)
                .Link("startTask", "starttask", HttpMethod.Post)
                .Link("startTasks", "starttask", HttpMethod.Post, true)
                .Link("pauseTask", "pausetask", HttpMethod.Post)
                .Link("pauseTasks", "pausetask", HttpMethod.Post, true)
                .Link("resumeTask", "resumetask", HttpMethod.Post)
                .Link("resumeTasks", "resumetask", HttpMethod.Post, true)
                .Link("completeTask", "completetask", HttpMethod.Post)
                .Link("completeTasks", "completetask", HttpMethod.Post, true)
                .Link("enableTaskRecurrence", "enabletaskrecurrence", HttpMethod.Post)
                .Link("enableTasksRecurrence", "enabletaskrecurrence", HttpMethod.Post, true)
                .Link("disableTaskRecurrence", "disabletaskrecurrence", HttpMethod.Post)
                .Link("disableTasksRecurrence", "disabletaskrecurrence", HttpMethod.Post, true)
                .Link("getCalendar", "calendar", HttpMethod.Get)
                .Link("checkTaskSatisfiesFilter", "tasksatisfiesfilter", HttpMethod.Post)
                .Link("getUnsavedFilter", "unsavedfilter", HttpMethod.Get)
                .Link("setUnsavedFilter", "unsavedfilter", HttpMethod.Post)
                .Link("getSelectedFolders", "selectedfolders", HttpMethod.Get, true)
                .Link("setSelectedFolders", "selectedfolders", HttpMethod.Post, true)
                .Link("importTasks", "importtasks", HttpMethod.Post)
                .Link("exportTasks", "exporttasks", HttpMethod.Post)
                .Link("getUserNickname", "usernickname", HttpMethod.Get)
                .Link("updateUserNickname", "usernickname", HttpMethod.Put)
                .Link("getUserData", "userdata", HttpMethod.Get, true)
                .Link("updateUserData", "userdata", HttpMethod.Post, true)
                .Link("deleteUserProfile", "user", HttpMethod.Delete);
        }
    }
}