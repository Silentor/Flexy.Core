namespace Flexy.Core;

public static class SingleExtensions
{
	public const Single ZeroTolerance = 1e-5f;
	
	public static Boolean	Near		( this Single value0, Single value1, Single epsilon = ZeroTolerance )
	{
		return Math.Abs( value0 - value1 ) <= epsilon;
	}
}