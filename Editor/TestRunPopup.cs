using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
	[RuntimeInitializeOnLoadMethod]
	static void CleanUpRun( ) => IsTestLaunched_InThisSession = default;

	private const String	Test_Selected	= "Flexy.Core.TestRun: Selected";
	public static Boolean	IsTestLaunched_InThisSession;
	
	private static List<TestRunSource> TestRunSources = new ( );
	
	public static void		AddTestProvider			( String providerName, Func<IEnumerable<String>> testsCollectionProvider )
	{
		TestRunSources.Add( new( providerName, testsCollectionProvider ) );	
	}
	public static String	GetTestNameToLaunch		( String providerName )	
	{
		if( IsTestLaunched_InThisSession )
			return null;
		
		var testSelected = EditorPrefs.GetString( Test_Selected ); 
		
		if( testSelected.StartsWith( providerName + ": " ) )
		{
			IsTestLaunched_InThisSession = true;
			return testSelected[(providerName.Length+2)..];
		}
		
		return null;
	}
	private static void		OnTestRunGUI			( )	
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
						menu.AddItem( new( $" {testRun} " ), false, SetTestRunName, (testRunSource.Name, testRun) );
					}
				}
				catch (Exception ex) { Debug.LogException(ex); }
			}
			
			menu.ShowAsContext( );
		}
		
		GUILayout.EndHorizontal( );
		
		GUILayout.FlexibleSpace( );
		
		static void		SetTestRunName	( Object userdata )
		{
			if( userdata == null )
			{
				EditorPrefs.SetString( Test_Selected, null );
			}
			else
			{
				var pair = ((String prefix, String testName))userdata;
				EditorPrefs.SetString( Test_Selected, $"{pair.prefix}: {pair.testName}" );
			}
		}
	}

	private record struct TestRunSource ( String Name, Func<IEnumerable<String>> GetTestRuns );
}