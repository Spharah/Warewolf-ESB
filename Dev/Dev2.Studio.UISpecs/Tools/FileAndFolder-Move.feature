﻿Feature: FileAndFolder-Move
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Move
Scenario:Move Tool Large View And Invalid Variables Expected Error On Done Button
	Given I have Warewolf running
	And all tabs are closed	
	And I click "EXPLORER,UI_localhost_AutoID"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Move Tool From Tool Box
	Given I send "Move" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLMOVE" onto "WORKSURFACE,StartSymbol"
	#Opening Move Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner)"
	#BuG12561 Passing Invalid Recordset Variable in  File or Folder Field And Expected Validation on Done button
	Given I type "[[rec@(1).a]]" in "WORKSURFACE,Move(MoveDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given "WORKSURFACE,UI_Error1_AutoID" is visible
	Given "WORKSURFACE,UI_Error2_AutoID" is visible
	#Correcting File or Folder Field bad variable and expected no error on done button
	Given I type "[[rec(1).a]]" in "WORKSURFACE,Move(MoveDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given "WORKSURFACE,UI_Error1_AutoID" is visible
	#Passing Invalid Recordset Variable in  Destination Field And Expected Validation on Done button
    Given I type "[[rec(1).%a]]" in "WORKSURFACE,Move(MoveDesigner),LargeViewContent,UI__Destinationtxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	#Correcting Destination Field bad variable and expected no error on done button
	Given I type "[[rec(1).a]]" in "WORKSURFACE,Move(MoveDesigner),LargeViewContent,UI__Destinationtxt_AutoID"
    And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is invisible within "1" seconds
	#Opening Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner)"
	#Expecting error when click on done with username only without password
	Given I type "TestingMove" in "WORKSURFACE,Move(MoveDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	And I send "{TAB}" to "WORKSURFACE,Move(MoveDesigner),LargeViewContent,UI__UserNametxt_AutoID"
	And I send "Password" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner),DoneButton"
	Given "WORKSURFACE,Move(MoveDesigner),SmallViewContent" is visible
	#Expecting error when click on done with username only without password in destination side
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner)"
	Given I send "{TAB}" to "WORKSURFACE,Move(MoveDesigner),LargeViewContent,UI__Destinationtxt_AutoID"
	And I send "Testwareusername" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner),DoneButton"
	Given "WORKSURFACE,UI_Error0_AutoID" is visible
	Given I send "{TAB}{TAB}" to "WORKSURFACE,Move(MoveDesigner),LargeViewContent,UI__Destinationtxt_AutoID"
	And I send "Password2" to ""
	And I click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner),DoneButton"
	Given "WORKSURFACE,Move(MoveDesigner),SmallViewContent" is visible



	
Scenario:Move Tool Testing Tab Order and UiRepondingFine as expected
	Given I have Warewolf running
	And all tabs are closed	
	Given I click "EXPLORERCONNECTCONTROL"
	Given I click "U_UI_ExplorerServerCbx_AutoID_localhost"
	And I click "RIBBONNEWENDPOINT"
	#Dragging Copy Tool From Tool Box
	Given I send "Move" to "TOOLBOX,PART_SearchBox"
    Given I drag "TOOLMOVE" onto "WORKSURFACE,StartSymbol"
	##Opening Copy Large View
	Given I double click "WORKFLOWDESIGNER,Unsaved 1(FlowchartDesigner),Move(MoveDesigner)"
	##Passing Data Into the tool by using Tabs
    And I send "[[rec(1).a]]{TAB}" to "WORKSURFACE,Move(MoveDesigner),LargeViewContent,UI__FileOrFoldertxt_AutoID"
	And I send "Source@Username{TAB}" to ""
    And I send "Password{TAB}" to ""
	And I send "[[rec(2).a]]{TAB}" to ""
	And I send "Destination{TAB}" to ""
    And I send "Password{TAB}{SPACE}{TAB}" to ""
	And I send "[[Result]]" to ""
	Given "WORKSURFACE,Move(MoveDesigner),LargeViewContent,UI__Resulttxt_AutoID" contains text "[[Result]]" 













