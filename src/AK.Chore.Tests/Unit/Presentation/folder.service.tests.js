/*******************************************************************************************************************************
 * AK.Chore.Tests.Unit.Presentation.folder.service.tests.js
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

/// <reference path="angular-mocks.js" />
/// <reference path="jasmine\jasmine.js" />

'use strict';

/* Tests for folderService.
 * @author Aashish Koirala
 */
define(['app', 'ngMock', 'api.service', 'folder.service'], function (app) {

    app.value('links', []);

    describe('folderService', function() {

        beforeEach(module('app'));

        it('loads folders, makes them available via folders() and sets selectedFolderIds() when loadFolders() is called', function(done) {
            inject(function($q, $rootScope, apiService, folderService) {

                apiService.getFolders = function() {
                    var deferred = $q.defer();
                    deferred.resolve([{ id: 1, name: 'Test1', folders: [{ id: 2, name: 'Test2', folders: [] }] }]);
                    return deferred.promise;
                };

                apiService.getSelectedFolders = function() {
                    var deferred = $q.defer();
                    deferred.resolve([{ id: 1 }]);
                    return deferred.promise;
                };

                folderService.loadFolders().then(function() {
                    expect(folderService.folders()).not.toBeNull();
                    expect(folderService.folders().length).toBe(2);
                    expect(folderService.folders()[0].id).toBe(1);
                    expect(folderService.folders()[0].name).toBe('Test1');
                    expect(folderService.folders()[1].id).toBe(2);
                    expect(folderService.folders()[1].name).toBe('Test2');
                    expect(folderService.selectedFolderIds()).not.toBeNull();
                    expect(folderService.selectedFolderIds().length).toBe(1);
                    expect(folderService.selectedFolderIds()[0]).toBe(1);
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('loads folders again only if not loaded when loadFoldersIfNeeded() is called', function(done) {
            inject(function($q, $rootScope, folderService) {

                folderService._folders = [];
                spyOn(folderService, 'loadFolders').and.callFake(function() {
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });

                folderService.loadFoldersIfNeeded().then(function() {
                    expect(folderService.loadFolders).not.toHaveBeenCalled();
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls setSelectedFolders() with selectedFolderIds on the API when applyFolderSelection is called', function(done) {
            inject(function($q, $rootScope, apiService, folderService) {

                apiService.setSelectedFolders = function() {
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'setSelectedFolders').and.callThrough();

                folderService.selectedFolderIds([1, 2, 3]);
                folderService.applyFolderSelection().then(function() {

                    expect(apiService.setSelectedFolders).toHaveBeenCalledWith([{ id: 1 }, { id: 2 }, { id: 3 }]);
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('returns folder path when getFolderPath is called for existing Id', function() {
            inject(function($q, $rootScope, folderService) {

                folderService.folders([{ id: 1, fullPath: 'TestPath' }]);
                var path = folderService.getFolderPath(1);

                expect(path).toBe('TestPath');
            });
        });

        it('returns Unknown when getFolderPath is called for non-existing Id', function() {
            inject(function($q, $rootScope, folderService) {

                folderService.folders([{ id: 1, fullPath: 'TestPath' }]);
                var path = folderService.getFolderPath(2);

                expect(path).toBe('Unknown');
            });
        });

        it('calls addFolder() on the API and calls loadFolders() when done when addFolder() is called', function(done) {
            inject(function($q, $rootScope, apiService, folderService) {

                apiService.addFolder = function(folder) {
                    console.log(folder);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'addFolder').and.callThrough();
                spyOn(folderService, 'loadFolders').and.callFake(function() {
                    var deferred = $q.defer();
                    deferred.resolve([]);
                    return deferred.promise;
                });

                var testFolder = { id: 1, name: 'TestFolder' };

                folderService.addFolder(testFolder).then(function() {

                    expect(apiService.addFolder).toHaveBeenCalledWith(testFolder);
                    expect(folderService.loadFolders).toHaveBeenCalled();
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls updateFolder() on the API when updateFolder() is called', function(done) {
            inject(function($q, $rootScope, apiService, folderService) {

                folderService.folders([]);

                apiService.updateFolder = function(folder) {
                    console.log(folder);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'updateFolder').and.callThrough();

                var testFolder = { id: 1, name: 'TestFolder' };

                folderService.updateFolder(testFolder).then(function() {

                    expect(apiService.updateFolder).toHaveBeenCalledWith(testFolder);
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls deleteFolder() on the API and calls loadFolders() when done when deleteFolder() is called', function(done) {
            inject(function($q, $rootScope, apiService, folderService) {

                apiService.deleteFolder = function(id) {
                    console.log(id);
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'deleteFolder').and.callThrough();
                spyOn(folderService, 'loadFolders').and.callFake(function() {
                    var deferred = $q.defer();
                    deferred.resolve([]);
                    return deferred.promise;
                });

                var testFolder = { id: 1, name: 'TestFolder' };

                folderService.deleteFolder(testFolder).then(function() {

                    expect(apiService.deleteFolder).toHaveBeenCalledWith(1);
                    expect(folderService.loadFolders).toHaveBeenCalled();
                    done();
                });

                $rootScope.$apply();
            });
        });

        it('calls moveFolder() on the API and calls loadFolders() when done when moveFolder() is called', function(done) {
            inject(function($q, $rootScope, apiService, folderService) {

                apiService.moveFolder = function(folder, targetFolderId) {
                    console.log({ folder: folder, targetFolderId: targetFolderId });
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                };

                spyOn(apiService, 'moveFolder').and.callThrough();
                spyOn(folderService, 'loadFolders').and.callFake(function() {
                    var deferred = $q.defer();
                    deferred.resolve([]);
                    return deferred.promise;
                });

                var testFolder = { id: 1, name: 'TestFolder' };

                folderService.moveFolder(testFolder, 2).then(function() {

                    expect(apiService.moveFolder).toHaveBeenCalledWith({ folderId: 1, moveToFolderId: 2 });
                    expect(folderService.loadFolders).toHaveBeenCalled();
                    done();
                });

                $rootScope.$apply();
            });
        });
    });
});