namespace Flexy.Core
{
	[DefaultExecutionOrder(Int16.MinValue+1)]
    public sealed class MainContext : GameContext
    {
		[SerializeField] Boolean			_isSingleton;
		[SerializeField] GlobalContext		_globalContext;
		
		//very first Awake in scene thanks to DefaultExecutionOrder
		private new		void			Awake				( )		
		{
			var isDuplicate = CtxWithMyNameExists( );
			
			if ( isDuplicate )
			{
				DestroyImmediate( gameObject );
				return;
			}
			
			if( _global == null )
				Instantiate( _globalContext );
			
			transform.parent = null;
			
			base.Awake( );
			
			if( _isSingleton )
				DontDestroyOnLoad( gameObject );
		}
		private			Boolean			CtxWithMyNameExists	( ) 	
		{
			if( _global == null )
				return false;
			
			var ctx = GetCtx( gameObject );
				
			while ( ctx )
			{
				if ( ctx is MainContext tctx && tctx.Name == Name )
					return true;
					
				ctx = ctx._parent;
			}
				
			return default;
		}
	}
}
