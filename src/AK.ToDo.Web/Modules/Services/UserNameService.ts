/*******************************************************************************************************************************
 * AK.To|Do.Web.Services.UserNameService
 * Copyright © 2013 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of To Do.
 *  
 * To Do is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * To Do is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with To Do.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

/// <reference path='..\Common.ts' />

module AK.ToDo.Web.Services {

    // Operations against the user-name API.
    //
    export class UserNameService {

        private userNameResource: Common.IResource = null;
        private $q: ng.IQService = null;

        //-------------------------------------------------------------------------------------------------------------

        constructor($resource: any, $q: ng.IQService, resourceApiRoot: string) {

            var self = this;

            self.$q = $q;
            self.userNameResource = $resource(resourceApiRoot + 'username/', null);
        }

        //-------------------------------------------------------------------------------------------------------------

        public getUserName(): ng.IPromise {

            var self = this;
            var deferred = self.$q.defer();

            var userName: any = self.userNameResource.get(null, function (): void {
                deferred.resolve(userName.UserName);
            }, function (response: Common.WebApiErrorResponse): void {
                deferred.reject(response.data.Message);
            });

            return deferred.promise;
        }
   }
}