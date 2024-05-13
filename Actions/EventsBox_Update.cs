using Flexy.Core;

namespace asd.Module.Action.TurnBased
{
	public class EventsBox_Update : MonoBehaviour
	{
		[SerializeField]	FlexyEvent	_update;
		
		private void Update		( ) => _update	.Raise( this );
	}
}