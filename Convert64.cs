using Unity.Collections.LowLevel.Unsafe;

namespace Flexy.Core;

public static class Convert64
{
	public static T2	As<T1, T2>( T1 data )  => new Convert64Memory<T1>{ Data = data }.As<T2>( );
}

internal struct Convert64Memory<T1> 
{
	public	T1		Data;
	public	UInt64	_padding;
		
	public	T2		As<T2>() => UnsafeUtility.As<T1, T2>( ref Data ); 
}