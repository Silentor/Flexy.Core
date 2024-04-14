namespace Flexy.Core.Actions
{
	[Serializable]
	public class SetEnabled : FlexyActionSync
	{
		[SerializeField] MonoBehaviour	_target;
		[SerializeField] Boolean		_enabled;
		
		public override void Do( ref FCtx ctx )
		{
			_target.enabled = _enabled;
		}
	}
}