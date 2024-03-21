using Flexy.JsonXs;
using UnityEditor;

namespace Flexy.Core;

[Serializable]
public struct LocString : ISerializeAsString
{
	public static Func<String, String> LocalizeFunc;
	
	[SerializeField] String _key;

	public static implicit operator String( LocString str ) => str.ToString( );
	public override String ToString( ) => LocalizeFunc == null ? _key : LocalizeFunc?.Invoke( _key );
	
	String	ISerializeAsString.ToString		( )				=> "'"+_key;
	void	ISerializeAsString.FromString	( String data )	=>_key = data;
}

#if UNITY_EDITOR
public class UIStringDrawer : PropertyDrawer
{
	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
	{
		var prop = property.FindPropertyRelative( "_key" );
		EditorGUI.PropertyField( position, prop, label );
	}
}
#endif