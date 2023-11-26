using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flexy.Core.Editor
{
	[InitializeOnLoad]
	public static class UnityEditorTopToolbar
	{
		static UnityEditorTopToolbar( )
		{
			EditorApplication.update -= EditorUpdate;
			EditorApplication.update += EditorUpdate;
		}

		public static void AddIMGUIContainerToLeftPocket	( Action onGUI ) => AddVisualElementToLeftPocket	( new IMGUIContainer(()=>ImguiUI(onGUI)){ style = { flexGrow = 1}} );
		public static void AddIMGUIContainerToRightPocket	( Action onGUI ) => AddVisualElementToRightPocket	( new IMGUIContainer(()=>ImguiUI(onGUI)){ style = { flexGrow = 1}} );
		public static void AddVisualElementToLeftPocket		( VisualElement e )
		{
			var ve = new VisualElement( ){ style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
			LeftPocket.Add( ve );
			ve.Add( e );
		}
		public static void AddVisualElementToRightPocket	( VisualElement e )
		{
			RightPocket.Add( e );
		}

		private static readonly VisualElement LeftPocket	= new( ){ style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
		private static readonly VisualElement RightPocket	= new( ){ style = { flexGrow = 1, flexDirection = FlexDirection.Row } };

		private static ScriptableObject _editorToolbarPanel;		
		
		private static void ImguiUI		( Action onGUI )	
		{
			GUILayout.BeginHorizontal( );
			try						{ onGUI( ); }
			catch (Exception ex)	{ Debug.LogException( ex ); }
			GUILayout.EndHorizontal( );
		}
		private static void EditorUpdate( )					
		{
			if ( _editorToolbarPanel != null ) 
				return;
			
			var toolbars		= Resources.FindObjectsOfTypeAll( typeof(UnityEditor.Editor).Assembly.GetType( "UnityEditor.Toolbar" ) );
			_editorToolbarPanel	= toolbars.Length > 0 ? (ScriptableObject) toolbars[0] : null;

			if ( _editorToolbarPanel == null ) 
				return;
			
			var root		= _editorToolbarPanel.GetType( ).GetField( "m_Root", BindingFlags.NonPublic | BindingFlags.Instance );
			var rootElement	= root.GetValue( _editorToolbarPanel ) as VisualElement;
			
			rootElement.Q( "ToolbarZoneLeftAlign" )	.Add( LeftPocket );
			rootElement.Q( "ToolbarZoneRightAlign" ).Add( RightPocket );
		}
	}
}
