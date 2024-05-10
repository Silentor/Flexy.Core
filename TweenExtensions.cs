using Flexy.Core.Tweens;

namespace Flexy.Core;

public static class TweenExtensions
{
	public static Builder<Single, SingleLerper>		To			( this FTween ts, Single from,	Single to,	Single duration, Ease ease = default )					=> Value<Single, SingleLerper>		( ts, from, to, duration, ease );
	public static Builder<Int32, Int32Lerper>		To			( this FTween ts, Int32 from,	Int32 to,	Single duration, Ease ease = default )					=> Value<Int32, Int32Lerper>		( ts, from, to, duration, ease );
	
	public static Builder<Single, SingleLerper>		Single		( this FTween ts, Single from , Single to, Single duration, Ease ease = default )					=> Value<Single, SingleLerper>		( ts, from, to, duration, ease );
	public static Builder<Vector3, Vector3Lerper>	Position	( this FTween ts, Transform tr, Vector3 from , Vector3 to, Single duration, Ease ease = default )	=> Value<Vector3, Vector3Lerper>	( ts, from, to, duration, ease ).BindTo( tr, static (tr, value) => tr.position = value );
	
	private static Builder<T, TLerp> Value<T, TLerp> ( this FTween ts, T from , T to, Single duration, Ease ease = default ) where T: unmanaged where TLerp: unmanaged, ITweenLerper<T> => new( from, to , duration, ease );
}