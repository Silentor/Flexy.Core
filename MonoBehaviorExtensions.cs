namespace Flexy.Core;

public static class MonoBehaviorExtensions
{
	public static T 	OrNull<T>( this T obj ) where T: UnityEngine.Object => (obj) ? obj : null;
}