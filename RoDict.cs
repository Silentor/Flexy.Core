using System.Collections;

namespace Flexy.Core
{
	public struct RoDict<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private RoDict(IDictionary<TKey, TValue> dict) => _dict = dict;

		private readonly IDictionary<TKey, TValue> _dict;

		public	Int32				Count		=> _dict.Count;
		public	Boolean				IsReadOnly	=> _dict.IsReadOnly;
		
		public	ICollection<TKey>	Keys		=> _dict.Keys;
		public	ICollection<TValue>	Values		=> _dict.Values;
		
		public	TValue				this [ TKey key ]
		{
			get => _dict[key];
			set => throw new NotImplementedException( );
		}
		
		public	Boolean		Contains		( KeyValuePair<TKey, TValue> item )	=> _dict.Contains( item );
		public	Boolean		ContainsKey		( TKey key )						=> _dict.ContainsKey( key );
		public	Boolean		TryGetValue		( TKey key, out TValue value )		=> _dict.TryGetValue( key, out value );
		
		public		IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ( )	=> _dict.GetEnumerator( );
		IEnumerator	IEnumerable.GetEnumerator ( )								=> GetEnumerator( );
		
		void	ICollection<KeyValuePair<TKey, TValue>>.CopyTo	( KeyValuePair<TKey, TValue>[] array, Int32 arrayIndex )	=> _dict.CopyTo( array, arrayIndex );
		
		public static implicit operator RoDict<TKey, TValue>( Dictionary<TKey, TValue> dict ) => new( dict );
		
		
		void	ICollection<KeyValuePair<TKey, TValue>>.Add		( KeyValuePair<TKey, TValue> item )						=> throw new NotImplementedException( );
		void	ICollection<KeyValuePair<TKey, TValue>>.Clear	( )														=> throw new NotImplementedException( );
		Boolean	ICollection<KeyValuePair<TKey, TValue>>.Remove	( KeyValuePair<TKey, TValue> item )						=> throw new NotImplementedException( );

		void	IDictionary<TKey, TValue>.Add		( TKey key, TValue value )	=> throw new NotImplementedException( );
		Boolean	IDictionary<TKey, TValue>.Remove	( TKey key )				=> throw new NotImplementedException( );
	}
}