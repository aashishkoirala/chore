/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.folders.folders.controller.js
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

/* foldersController - Angular controller for the "Folders" section.
 * @author Aashish Koirala
 */
define(['app'], function (app) {

    foldersController.$inject = ['$scope', '$rootScope', '$routeParams', 'initializationService', 'folderService', 'appState'];
    app.register.controller('foldersController', foldersController);

    function foldersController($scope, $rootScope, $routeParams, initializationService, folderService, appState) {

        $scope.folders = [];
        $scope.state = {
            selectedFolder: null,
            targetFolders: null
        };

        $scope.isWorking = function() {
            return appState.isWorking;
        };
        
        $scope.range = function(number) {
            return new Array(number);
        };

        $scope.cancel = function() {
            assignFolders(null);
            $scope.state.selectedFolder = null;
        };

        $scope.save = function() {

            if ($scope.state.selectedFolder == null ||
                $scope.state.selectedFolder.name == null ||
                $scope.state.selectedFolder.name.trim() == '') return;

            var folder = {
                id: $scope.state.selectedFolder.id,
                name: $scope.state.selectedFolder.name,
                parentFolderId: $scope.state.selectedFolder.parentFolderId
            };

            if (folder.id != 0) {
                var folders = folderService.folders();
                var existingFolder = findFolderById(folders, folder.id);

                if (existingFolder != null) folder.userId = existingFolder.userId;
            }

            var promise = folder.id == 0 ? folderService.addFolder(folder) : folderService.updateFolder(folder);
            promise.then(function() {
                $rootScope.$broadcast('notifyReload');
                $scope.cancel();
            });
        };

        $scope.add = function() {

            var level = 0;
            var parentFolderId = null;
            var index = -1;

            if ($scope.state.selectedFolder != null) {
                level = $scope.state.selectedFolder.level + 1;
                parentFolderId = $scope.state.selectedFolder.id;
                index = $scope.folders.indexOf($scope.state.selectedFolder) + 1;
            }

            var folder = {
                id: 0,
                parentFolderId: parentFolderId,
                name: 'New Folder',
                level: level
            };
           
            if (index == -1) $scope.folders.push(folder);
            else $scope.folders.splice(index, 0, folder);

            $scope.state.selectedFolder = folder;
        };

        $scope.delete = function() {

            if ($scope.state.selectedFolder == null || $scope.state.selectedFolder.id == 0) return;

            folderService
                .deleteFolder($scope.state.selectedFolder)
                .then(function() {
                    $rootScope.$broadcast('notifyReload');
                    $scope.cancel();
                });
        };

        $scope.moveTo = function(targetId) {

            if ($scope.state.selectedFolder == null || $scope.state.selectedFolder.id == 0) return;

            folderService
                .moveFolder($scope.state.selectedFolder, targetId)
                .then(function() {
                    $rootScope.$broadcast('notifyReload');
                    $scope.cancel();
                });
        };
        
        initialize();

        function initialize() {

            appState.currentView = 'Folders';
            appState.isError = false;

            var folderId = $routeParams['folderId'];

            initializationService
                .initialize()
                .then(function() {
                    assignFolders(folderId);
                });
        }

        function assignFolders(folderId) {
            
            var folders = folderService.folders();

            $scope.state.targetFolders = folderService.folders();
            
            $scope.folders = folders
                .filter(function(item) {
                    return item.id > 0;
                })
                .map(function(item) {
                    return {
                        id: item.id,
                        name: item.name,
                        level: item.level,
                        parentFolderId: item.parentFolderId
                    };
                });

            $scope.state.selectedFolder = folderId == null ?
                $scope.folders[0] :
                (findFolderById($scope.folders, folderId) || $scope.folders[0]);
        }
        
        function findFolderById(folders, id) {
            var matches = folders.filter(function(item) {
                return item.id == id;
            });
            return matches.length == 0 ? null : matches[0];
        }
    }
});