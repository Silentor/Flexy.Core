using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

namespace Flexy.Core.Editor;

[InitializeOnLoad]
public static class TestRunPopup
{
	static TestRunPopup( )
	{
		UnityEditorTopToolbar.AddIMGUIContainerToRightPocket( OnTestRunGUI );
		//EditorSceneManager.sceneClosed		+= s		=> EditorPrefs.SetString( Test_Selected, null );
		EditorSceneManager.sceneOpened		+= (_, _)	=> EditorPrefs.SetString( Test_Selected, null );
	}

	private const String Test_Selected	= "Flexy.Core.TestRun: Selected";
	
	public static List<TestRunSource> TestRunSources = new ( );
	
	public static String	GetRunName		( )	=>  EditorPrefs.GetString( Test_Selected );
	
	private static void		OnTestRunGUI	( )							
	{
		var style = EditorStyles.label;
		style.richText = true;
		
		var testSelected = EditorPrefs.GetString( Test_Selected );
        
		if( String.IsNullOrWhiteSpace( testSelected ) )
			testSelected = "None";
		
		GUILayout.BeginHorizontal( GUI.skin.box );
		
		GUI.enabled = false;
		GUILayout.Label( "Test Run:" );
		GUI.enabled = true;
		
		if( EditorGUILayout.DropdownButton( new( $"{testSelected}" ), FocusType.Passive, EditorStyles.popup ) )
		{
			var menu = new GenericMenu();
			
			menu.AddItem( new( "None" ), false, SetTestRunName, null );
			
			foreach ( var testRunSource in TestRunSources )
			{
				try
				{
					var testRuns = testRunSource.GetTestRuns( ).ToArray( );
				
					if( !testRuns.Any( ) )
						continue;
					
					menu.AddSeparator("");

					menu.AddItem( new( $"- {testRunSource.Name} -" ), false, null );

					foreach ( var testRun in testRuns )
					{
						menu.AddItem( new( $" {testRun} " ), false, SetTestRunName, testRun );
					}
				}
				catch (Exception ex) { Debug.LogException(ex); }
			}
			
			menu.ShowAsContext( );
		}
		
		GUILayout.EndHorizontal( );
		
		GUILayout.FlexibleSpace( );
		
		static void		SetTestRunName	( Object userdata ) => EditorPrefs.SetString( Test_Selected, (String)userdata );
	}
	
	public record struct TestRunSource ( String Name, Func<IEnumerable<String>>	GetTestRuns );
}