using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;

namespace Flexy.Core;

public static class Reinterpret
{
	public static			TTo			As<TTo, TFrom>		( TFrom data )		where TTo : unmanaged where TFrom : unmanaged	
	{
		return new Convert64Memory<TFrom>{ Data = data }.As<TTo>( );
	}
	public static			ref TTo		AsRef<TTo, TFrom>	( ref TFrom data )	where TTo : unmanaged where TFrom : unmanaged	
	{ 
		Assert.IsTrue( UnsafeUtility.SizeOf<TFrom>() >= UnsafeUtility.SizeOf<TTo>() );
		
		return ref UnsafeUtility.As<TFrom, TTo>( ref data );
	}
}

internal struct Convert64Memory<T1> 
{
	public	T1		Data;
	public	UInt64	_padding;
		
	public	T2		As<T2>() => UnsafeUtility.As<T1, T2>( ref Data ); 
}