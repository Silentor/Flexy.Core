namespace Flexy.Core;

public static class MonoBehExt
{
	public static T 	OrNull<T>( this T obj ) where T: UnityEngine.Object => (obj) ? obj : null;
}