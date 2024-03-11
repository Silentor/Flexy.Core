namespace Flexy.Core
{
	[DefaultExecutionOrder(Int16.MaxValue-1)]
	[RequireComponent(typeof(GlobalContext))]
	public sealed class GlobalContextLastUpdates : MonoBehaviour
	{
		private GlobalContext _ctx; 
		
		private void Awake		( ) => _ctx = GetComponent<GlobalContext>( );
		
		private void FixedUpdate( ) => _ctx.FixedUpdateLast( );
		private void Update		( ) => _ctx.UpdateLast( );
		private void LateUpdate	( ) => _ctx.LateUpdateLast( );
	}
}
