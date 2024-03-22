#if UNITY_EDITOR
using UnityEditor.SceneManagement;

namespace Flexy.Core;

public static class EditorGameExtensions
{
	private static String				_latestEditorGamePath;
	private static UnityEngine.Object	_latestRequestSource;

	public static String				LatestEditorGamePath	=> _latestEditorGamePath;
	public static UnityEngine.Object	LatestRequestSource		=> _latestRequestSource;

	public static String	GetGamePath			( UnityEngine.Object ctx )	
	{
		IsGamePathChanged( ctx );
		return _latestEditorGamePath;
	}
	public static Boolean	IsGamePathChanged	( UnityEngine.Object ctx )	
	{
		if ( ctx == _latestRequestSource )
			return false;
		
		var gameAssetPath = ctx switch
		{
			ScriptableObject so => UnityEditor.AssetDatabase.GetAssetPath( so ),
			Component mb when mb.gameObject.scene.IsValid() => StageUtility.GetStage( mb.gameObject.scene ).assetPath,
			Component mb => UnityEditor.AssetDatabase.GetAssetPath( mb.gameObject.transform.root.gameObject )
		};
				
				
		_latestRequestSource = ctx;
		//path = _editorGamePath;		
		
		if ( _latestEditorGamePath != null && gameAssetPath.StartsWith( _latestEditorGamePath ) )
			return false;
		
		// Find new gd info for current selected object 
		var index	= gameAssetPath.LastIndexOf( "game.", StringComparison.InvariantCulture );
		if ( index == -1 )	
			index = gameAssetPath.LastIndexOf( "fun.", StringComparison.InvariantCulture );

		if ( index == -1 )
		{
			_latestEditorGamePath = "Assets";
		}
		else
		{
			var index2 = gameAssetPath.IndexOf("/", index, StringComparison.InvariantCulture);
			_latestEditorGamePath = gameAssetPath[..(index2)];
		}

		//path = _editorGamePath;
		return true;
	}
	public static void		Clear				( )							
	{
		_latestEditorGamePath			= default;
		_latestRequestSource	= default;
	}

}
#endif