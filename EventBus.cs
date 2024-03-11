using System.Collections;
using System.Linq;

namespace Flexy.Core
{
	public class EventBusHub 
    {
		private Dictionary<Type, EventBusBase> _busses = new( 64 );
		
		public void				SendEvent<T>		( T ev, String name = null, Boolean delayed = false ) where T : struct, IEvent	
		{
			if( !_busses.TryGetValue( typeof(T), out var bus ) )
				_busses.Add( typeof(T), (bus = new EventBus<T>() ) );
			
			var typedBus = (EventBus<T>)bus;
			typedBus.SendEvent( name, ev, delayed );
		}
		public IEnumerable<T>	ReceiveEvents<T>	( String name = null )	where T : struct, IEvent	
		{
			if( !_busses.TryGetValue( typeof(T), out var bus ) )
				_busses.Add( typeof(T), (bus = new EventBus<T>() ) );
			
			var typedBus = (EventBus<T>)bus;
			return typedBus.ReceiveEvents( name );
		}
		internal void			ClearOldEvents		( )						
		{
			foreach (var (k,v) in _busses)
				v.Clear( );
		}
		
		public async UniTask<T>					WaitForEvent<T>		( String name = null, PlayerLoopTiming timing = PlayerLoopTiming.Update )	where T : struct, IEvent	
		{
			if( !_busses.TryGetValue( typeof(T), out var bus ) )
				_busses.Add( typeof(T), (bus = new EventBus<T>() ) );
			
			var typedBus = (EventBus<T>)bus;
			
			while( !typedBus.ReceiveEvents( name ).Any( ) )
				await UniTask.DelayFrame( 1, timing );

			return typedBus.ReceiveEvents( name ).First( );
		}
		public async UniTask<IEnumerable<T>>	WaitForEvents<T>	( String name = null, PlayerLoopTiming timing = PlayerLoopTiming.Update )	where T : struct, IEvent	
		{
			if( !_busses.TryGetValue( typeof(T), out var bus ) )
				_busses.Add( typeof(T), (bus = new EventBus<T>() ) );
			
			var typedBus = (EventBus<T>)bus;
			
			while( !typedBus.ReceiveEvents( name ).Any( ) )
				await UniTask.DelayFrame( 1, timing );

			return typedBus.ReceiveEvents( name );
		}
		
#if UNITY_EDITOR
		public void DrawBusesUI	( )		
		{
			foreach ( var pair in _busses )
			{
				GUILayout.Label( $"{pair.Key}" );
			}
		}
#endif
	}
	
	public class EventBus<T> : EventBusBase where T : struct, IEvent
	{
		private readonly List<(String,T)> _events			= new ( );
		private readonly List<(String,T)> _eventsDelayed	= new ( );

		public				Enumerator	ReceiveEvents	( String name )	=> new( name, _events );
		
		public				void		SendEvent		( String name, T ev, Boolean delay )	
		{
			(delay ? _eventsDelayed : _events).Add( (name, ev) );
		}
		internal override	void		Clear			( )										
		{
			_events.Clear( );
			_events.AddRange( _eventsDelayed );
			_eventsDelayed.Clear( );
		}
		
		public struct Enumerator : IEnumerator<T>, IEnumerable<T>
		{
			public Enumerator( String name, List<(String,T)> events )
			{
				_name			= name;
				_events			= events;
				_currentIndex	= -1;
			}
			
			private String				_name;
			private List<(String,T)>	_events;
			private Int32				_currentIndex;
			
			public T				Current			=> _events[_currentIndex].Item2;
			public IEnumerator<T>	GetEnumerator	( ) => this;
			public void				Dispose			( ) { }
			public void				Reset			( ) => _currentIndex = -1;
			public Boolean			MoveNext		( )
			{
				_currentIndex++;
				for ( ; _currentIndex < _events.Count; _currentIndex++ )
				{
					if( _events[_currentIndex].Item1 == _name )
						return true;
				}
				
				return false;
			}
			
			Object		IEnumerator.Current			=> Current;
			IEnumerator	IEnumerable.GetEnumerator	( ) => GetEnumerator( );
		}
	}
	
	public interface IEvent { } 
	public abstract class EventBusBase { internal abstract void Clear( ); }
}
