/*******************************************************************************************************************************
 * AK.Chore.Presentation.Client.criterion-editor.criterion-editor.directive.js
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

/* criterionEditorDirective - Angular directive for the criterion-editor component.
 * @author Aashish Koirala
 */
define(['app'], function(app) {

    app.directive('choreCriterionEditor', choreCriterionEditorDirective);

    choreCriterionEditorDirective.$inject = ['directiveRecursionService'];
    
    function choreCriterionEditorDirective(directiveRecursionService) {
        return {
            restrict: 'E',
            require: 'ngModel',
            transclude: true,
            templateUrl: 'Client/criterion-editor/criterion-editor.html',
            scope: { criterion: '=ngModel' },
            compile: function(element) {
                return directiveRecursionService.compile(element, this.link);
            },
            
            link: function(scope) {

                scope.fieldNames = {
                    'StartDate': 'Start Date',
                    'StartTime': 'Start Time',
                    'EndDate': 'End Date',
                    'EndTime': 'End Time',
                    'State': 'State',
                    'Description': 'Description'
                };

                scope.operatorNames = {
                    'Equals': 'Equals',
                    'DoesNotEqual': 'Does Not Equal',
                    'LessThan': 'Is Less Than',
                    'GreaterThan': 'Is Greater Than',
                    'LessThanOrEquals': 'Is Less Than or Equal To',
                    'GreaterThanOrEquals': 'Is Greater Than or Equal To',
                    'In': 'Is One Of',
                    'Like': 'Contains Phrase'
                };

                scope.valueNames = {
                    'TodaysDate': 'Today',
                    'ThisWeeksStartDate': 'Start of This Week',
                    'ThisWeeksEndDate': 'End of This Week',
                    'Literal': 'A Value I Specify'
                };

                scope.conjunctionNames = {
                    'And': 'And',
                    'Or': 'Or',
                    'AndNot': 'And Not',
                    'OrNot': 'Or Not'
                };

                scope.recurrenceTypeNames = {
                    'equals': 'Recurs On Date',
                    'in': 'Recurs On One of Given Dates',
                    'beforeAfter': 'Recurs Between'
                };

                scope.applicableOperatorNames = function(fieldName) {
                    var operators = ['Equals', 'DoesNotEqual'];

                    if (['StartDate', 'StartTime', 'EndDate', 'EndTime'].indexOf(fieldName) >= 0)
                        operators.push('LessThan', 'GreaterThan', 'LessThanOrEquals', 'GreaterThanOrEquals');

                    operators.push('In');

                    if (fieldName == 'Description') operators.push('Like');

                    var operatorNames = {};

                    for (var key in scope.operatorNames) {
                        if (operators.indexOf(key) >= 0)
                            operatorNames[key] = scope.operatorNames[key];
                    }

                    return operatorNames;
                };

                scope.applicableValueNames = function(fieldName, operatorName) {
                    var values = [];

                    if ((fieldName == 'StartDate' || fieldName == 'EndDate') && operatorName != 'In')
                        values.push('TodaysDate', 'ThisWeeksStartDate', 'ThisWeeksEndDate');

                    values.push('Literal');

                    var valueNames = {};
                    for (var key in scope.valueNames) {
                        if (values.indexOf(key) >= 0)
                            valueNames[key] = scope.valueNames[key];
                    }

                    return valueNames;
                };

                scope.literalHint = function() {
                    if (scope.criterion.simple == null) return '';

                    var hint = '';
                    
                    var fieldName = scope.criterion.simple.fieldName;
                    var operatorName = scope.criterion.simple.operatorName;
                    
                    if (fieldName == 'StartDate' || fieldName == 'EndDate')
                        hint = 'MMM dd, yyyy';
                    else if (fieldName == 'StartTime' || fieldName == 'EndTime')
                        hint = 'HH:MM';

                    if (operatorName == 'In')
                        hint += ' (Separate values with "|")';

                    return hint.trim();
                };

                scope.setAsTrue = function () {
                    scope.criterion.type = 'true';
                    scope.criterion.simple = null;
                    scope.criterion.recurrence = null;
                    scope.criterion.complex = null;
                };

                scope.setAsSimple = function() {
                    scope.criterion.type = 'simple';
                    scope.criterion.simple = {
                        fieldName: 'End Date',
                        operatorName: 'Equals',
                        valueName: 'TodaysDate',
                        valueLiteral: ''
                    };
                    scope.criterion.complex = null;
                    scope.criterion.recurrence = null;
                };

                scope.setAsRecurrence = function() {
                    scope.criterion.type = 'recurrence';
                    scope.criterion.recurrence = {
                        type: 'equals',
                        equalsTo: {
                            valueName: 'TodaysDate',
                            valueLiteral: ''
                        }
                    };
                    scope.criterion.complex = null;
                    scope.criterion.simple = null;
                };
                
                scope.addConjunction = function(conjunctionName) {
                    var existingPart = { type: 'true', simple: null, complex: null, recurrence: null };
                    var truePart = { type: 'true', simple: null, complex: null, recurrence: null };

                    if (scope.criterion.type == 'simple')
                        existingPart = { type: 'simple', simple: scope.criterion.simple };
                    else if (scope.criterion.type == 'recurrence')
                        existingPart = { type: 'recurrence', recurrence: scope.criterion.recurrence };

                    scope.criterion = {
                        type: 'complex',
                        simple: null,
                        recurrence: null,
                        complex: {
                            criterion1: existingPart,
                            conjunctionName: conjunctionName,
                            criterion2: truePart
                        }
                    };
                };

                scope.removeCriterion1 = function () {
                    if (scope.criterion.type != 'complex') return;
                    
                    scope.criterion = scope.criterion.complex.criterion2;
                };

                scope.removeCriterion2 = function () {
                    if (scope.criterion.type != 'complex') return;

                    scope.criterion = scope.criterion.complex.criterion1;
                };
            }
        };
    }
});