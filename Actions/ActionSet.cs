namespace Flexy.Core.Actions;

[Serializable]
public class ActionSet : FlexyAction
{
	[SerializeField]		Boolean			_sync;
	[SerializeReference]	FlexyAction[]	_set;
	
	
	public override void Do(ref FCtx ctx)
	{
		if( !_sync )
		{
			DoAsync( ctx ).Forget( );
		}
		else
		{
			foreach ( var action in _set )
				action.Do( ref ctx );
		}
	}
	
	public override async UniTask DoAsync(FCtx ctx)
	{
		if( _sync )
		{
			Do( ref ctx );
		}
		else
		{
			foreach ( var action in _set )
				await action.DoAsync( ctx );
		}
	}
	
	public override void Sample( ref FCtx ctx, Single time01 ) { Do( ref ctx ); }
}