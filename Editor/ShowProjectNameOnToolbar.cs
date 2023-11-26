using UnityEditor;
using UnityEngine;

namespace Flexy.Core.Editor
{
	[InitializeOnLoad]
	public class SceneSwitchLeftButton
	{
		static SceneSwitchLeftButton()
		{
			UnityEditorTopToolbar.AddIMGUIContainerToLeftPocket( OnToolbarGUI );
		}

		static void OnToolbarGUI()
		{
			GUILayout.FlexibleSpace();

			var style = EditorStyles.label;
			style.richText = true;
		
			if( EditorGUIUtility.isProSkin )
				GUILayout.Label( $"<size=16><color=#888888><b>---=== {Application.productName} ===---</b></color></size>", style );
			else
				GUILayout.Label( $"<size=16><color=#000000><b>---=== {Application.productName} ===---</b></color></size>", style );
			
			GUILayout.FlexibleSpace();
		}
	}
}