namespace Flexy.Core.Actions
{
	[Serializable]
	public class TweenLocalPosition : FlexyActionAsync
	{
		[SerializeField] Transform	_transform;
		[SerializeField] Vector3	_destination;
		[SerializeField] Single		_duration;
		
		public override async UniTask DoAsync( FCtx ctx )
		{
			//_transform.DOLocalMove( _destination, _duration ).Play( );
			
			await UniTask.Delay( TimeSpan.FromSeconds( _duration ) );
		}
	}
}