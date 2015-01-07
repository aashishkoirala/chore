/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.folders.folders.service.js
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

'use strict';

/* folderService - Talks to the web API for folder related stuff and state maintenance.
 * @author Aashish Koirala
 */
define(['app', 'api.service'], function (app) {

    folderService.$inject = ['$q', 'apiService'];
    return app.factory('folderService', folderService);

    function folderService($q, apiService) {

        return {
            _folders: null,
            _selectedFolderIds: null,

            folders: function(value) {
                if (value != undefined) {
                    this._folders = value;
                    assignFolderSelectedFlag(this._folders, this._selectedFolderIds);
                }
                return this._folders;
            },

            selectedFolderIds: function(value) {
                if (value != undefined) {
                    this._selectedFolderIds = value || [];
                    assignFolderSelectedFlag(this._folders, this._selectedFolderIds);
                }
                return this._selectedFolderIds;
            },

            loadFolders: function() {

                var deferred = $q.defer();
                var self = this;

                apiService
                    .getFolders()
                    .then(function(response) {
                        self._folders = flattenHierarchy(response);

                        apiService
                            .getSelectedFolders()
                            .then(function(selectedFolders) {
                                self._selectedFolderIds = selectedFolders.map(function(item) {
                                    return item.id;
                                });
                                assignSelectedAndResolve();
                            }, function() {
                                self._selectedFolderIds = null;
                                assignSelectedAndResolve();
                            });
                    });

                return deferred.promise;

                function assignSelectedAndResolve() {
                    assignFolderSelectedFlag(self._folders, self._selectedFolderIds);
                    deferred.resolve();
                }
            },

            loadFoldersIfNeeded: function() {

                var deferred = $q.defer();

                if (this._folders != null) deferred.resolve();
                else this.loadFolders().then(deferred.resolve);

                return deferred.promise;
            },

            applyFolderSelection: function () {
                
                var ids = (this._selectedFolderIds || []).map(function(id) {
                    return { id: id };
                });
                
                return apiService.setSelectedFolders(ids);
            },

            getFolderPath: function(id) {

                var self = this;
                var folders = self._folders || [];

                var matchingFolders = folders.filter(function(item) {
                    return item.id == id;
                });

                return (matchingFolders.length == 0) ? 'Unknown' : matchingFolders[0].fullPath;
            },
            
            addFolder: function(folder) {

                var deferred = $q.defer();
                var self = this;

                apiService
                    .addFolder(folder)
                    .then(function() {
                        self.loadFolders().then(deferred.resolve, deferred.reject);
                    }, deferred.reject);

                return deferred.promise;
            },
            
            updateFolder: function(folder) {

                var deferred = $q.defer();
                var self = this;

                apiService
                    .updateFolder(folder)
                    .then(function () {
                        var existingFolder = findFolderById(self._folders, folder.id);
                        if (existingFolder != null) {
                            existingFolder.name = folder.name;
                            existingFolder.treeName = getIndentation(existingFolder.level) + folder.name;
                        }
                        deferred.resolve();
                    }, deferred.reject);

                return deferred.promise;
            },
            
            deleteFolder: function(folder) {

                var deferred = $q.defer();
                var self = this;

                apiService
                    .deleteFolder(folder.id)
                    .then(function() {
                        self.loadFolders().then(deferred.resolve, deferred.reject);
                    }, deferred.reject);

                return deferred.promise;
            },
            
            moveFolder: function(folder, targetFolderId) {

                var deferred = $q.defer();
                var self = this;

                apiService
                    .moveFolder({ folderId: folder.id, moveToFolderId: targetFolderId })
                    .then(function() {
                        self.loadFolders().then(deferred.resolve, deferred.reject);
                    }, deferred.reject);

                return deferred.promise;
            }
        };

        function assignFolderSelectedFlag(folders, selectedFolderIds) {

            var selectedFolderIdsOrEmpty = selectedFolderIds || [];

            (folders || []).forEach(function(folder) {
                folder.isSelected = (selectedFolderIdsOrEmpty.indexOf(folder.id) >= 0);
            });
        }

        function getIndentation(charCount) {

            var indentation = '';

            for (var i = 0; i < charCount; i++) indentation += '-';
            if (charCount > 0) indentation += ' ';

            return indentation;
        }

        function flattenHierarchy(folders, level) {

            level = level || 0;
            var flattenedList = [];

            for (var i = 0; i < folders.length; i++) {
                folders[i].level = level;
                folders[i].treeName = getIndentation(level) + folders[i].name;

                flattenedList.push(folders[i]);

                var childList = flattenHierarchy(folders[i].folders, level + 1);

                for (var j = 0; j < childList.length; j++)
                    flattenedList.push(childList[j]);
            }

            return flattenedList;
        }
        
        function findFolderById(folders, id) {
            var matches = folders.filter(function (item) {
                return item.id == id;
            });
            return matches.length == 0 ? null : matches[0];
        }
    }
});