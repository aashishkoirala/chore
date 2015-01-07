/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.tasks.folder-picker.directive.js
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

/* folderPickerDirective - Directive for the "folder-picker" component.
 * @author Aashish Koirala
 */
define(['app', 'folders/folder.service'], function (app) {

    choreFolderPickerDirective.$inject = ['folderService', 'appState'];
    app.directive('choreFolderPicker', choreFolderPickerDirective);

    function choreFolderPickerDirective(folderService, appState) {
        return {
            restrict: 'E',
            templateUrl: 'Client/tasks/folder-picker.html',
            link: function (scope) {

                scope.isDropDownOpen = false;
                scope.isApplyClicked = false;
                scope.previousSelectedFolderIds = [];
                
                scope.folders = function() {
                    return folderService.folders();
                };

                scope.isWorking = function() {
                    return appState.isWorking;
                };
                
                scope.selectedFolderLabel = function () {
                    
                    var folders = folderService.folders() || [];
                    
                    var selectedFolderCount = folders.filter(function(item) {
                        return item.isSelected;
                    }).length;

                    if (selectedFolderCount == 0 || selectedFolderCount == folders.length)
                        return 'All folders selected';

                    return selectedFolderCount + ' folders selected';
                };

                scope.applyFolderSelection = function () {

                    var selectedFolderIds = scope.folders()
                        .filter(function(item) {
                            return item.isSelected;
                        })
                        .map(function(item) {
                            return item.id;
                        });

                    folderService.selectedFolderIds(selectedFolderIds);
                    folderService
                        .applyFolderSelection()
                        .then(function () {
                            scope.$parent.$broadcast('reloadTasks');
                            scope.isApplyClicked = true;
                            scope.isDropDownOpen = false;
                        }, function() {
                            scope.isDropDownOpen = false;
                        });
                };
                
                scope.handleDropDownToggle = function (isOpen) {

                    if (isOpen) {
                        
                        scope.previousSelectedFolderIds = [];                        
                        var selectedFolderIds = folderService.selectedFolderIds() || [];

                        selectedFolderIds.forEach(function(id) {
                            scope.previousSelectedFolderIds.push(id);
                        });
                        
                        return;
                    }
                    
                    if (scope.isApplyClicked) {
                        scope.isApplyClicked = false;
                        return;
                    }

                    folderService.selectedFolderIds(scope.previousSelectedFolderIds);
                };

                scope.openDropDown = function($event) {
                    $event.preventDefault();
                    $event.stopPropagation();
                    scope.isDropDownOpen = true;
                };
            }
        };
    }
});