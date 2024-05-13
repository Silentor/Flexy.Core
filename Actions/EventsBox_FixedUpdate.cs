using Flexy.Core;

namespace asd.Module.Action.TurnBased
{
	public class EventsBox_FixedUpdate : MonoBehaviour
	{
		[SerializeField]	FlexyEvent	_fixedUpdate;
		
		private void FixedUpdate		( ) => _fixedUpdate	.Raise( this );
	}
}