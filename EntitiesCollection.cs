using System.Collections.Generic;


public class EntitiesCollection<T>
{
	private readonly List<T> _entities = new List<T>();
	private readonly HashSet<T> _hashSet = new HashSet<T>();

	public int Count => _entities.Count;
	public T this[int index] => _entities[index];

	public bool Add( T element )
	{
		if( _hashSet.Contains( element ) ) return false;
		_entities.Add( element );
		_hashSet.Add( element );
		return true;
	}

	public void Remove( T element )
	{
		if( !_hashSet.Contains( element ) ) return;
		_entities.Remove( element );
		_hashSet.Remove( element );
	}
}