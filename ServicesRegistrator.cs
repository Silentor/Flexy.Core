namespace Flexy.Core
{
	public class ServicesRegistrator : MonoBehaviour
	{
		[SerializeField]	Boolean		FullHierarchy;
			
		private void Awake( )
		{
			var ctx = GameContext.GetCtx( this );
				
			ctx.AddServicesFromObject( gameObject, FullHierarchy );
		}
	}
}