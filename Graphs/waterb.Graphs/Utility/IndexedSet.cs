using System.Collections;

namespace waterb.Utility
{
	public enum IndexedSetOperation : byte
	{
		Add,
		Clear,
		Remove
	}
	
	public interface IIndexedSet<T> : IReadOnlyIndexedSet<T>
	{
		public bool TryAdd(T item);
		public void Add(T item);
		public bool Remove(T item);
		public void Clear();
	}
	
	public interface IReadOnlyIndexedSet<T> : IEnumerable<T>
	{
		public delegate void OnChanged(IndexedSetOperation op, T value);
		public event OnChanged OnChange;
		
		public int Count { get; }
		public bool TryGetIndex(T item, out int index);
		public T GetByIndex(int index);
		public T this[int index] { get; }
	}
	
	public class IndexedSet<T> : IIndexedSet<T>
	{
		private readonly Dictionary<T, int> _indexDict;
		private readonly List<T> _items;

		public event IReadOnlyIndexedSet<T>.OnChanged OnChange;
		public int Count => _items.Count;
		
		public IndexedSet(IEnumerable<T> src)
		{
			_indexDict = new Dictionary<T, int>(0);
			_items = new List<T>(0);
			
			foreach (var element in src)
			{
				_indexDict[element] = _items.Count;
				_items.Add(element);
			}
		}
		
		public IndexedSet(int capacity)
		{
			_indexDict = new Dictionary<T, int>(capacity);
			_items = new List<T>(capacity);
		}
		
		public IndexedSet() : this(0)
		{
			
		}

		public bool TryAdd(T item)
		{
			if (!_indexDict.ContainsKey(item))
			{
				_items.Add(item);
				_indexDict[item] = _items.Count - 1;
				OnChange?.Invoke(IndexedSetOperation.Add, item);
				return true;
			}

			return false;
		}

		public void Add(T item)
		{
			TryAdd(item);
		}

		public bool TryGetIndex(T item, out int index)
		{
			return _indexDict.TryGetValue(item, out index);
		}

		public T GetByIndex(int index)
		{
			return _items[index];
		}

		public T this[int index] => GetByIndex(index);

		public void RemoveByIndex(int index)
		{
			var removedItem = _items[index];
			var item = _items[^1];
			_items[index] = item;
			_indexDict[item] = index;
			_items.RemoveAt(_items.Count - 1);
			_indexDict.Remove(removedItem);
			OnChange?.Invoke(IndexedSetOperation.Remove, item);
		}

		public bool Remove(T item)
		{
			if (_indexDict.TryGetValue(item, out var index))
			{
				var lastIndex = _items.Count - 1;
				var lastItem = _items[lastIndex];
					
				_items[index] = lastItem;
				_indexDict[lastItem] = index;
					
				_items.RemoveAt(lastIndex);
				_indexDict.Remove(item);
				OnChange?.Invoke(IndexedSetOperation.Remove, item);
				return true;
			}

			return false;
		}

		public void Clear()
		{
			_items.Clear();
			_indexDict.Clear();
			OnChange?.Invoke(IndexedSetOperation.Clear, default);
		}

		public IReadOnlyList<T> GetItemsAsList() => _items;
		public IEnumerator<T> GetEnumerator()
		{
			for (var i = 0; i < Count; i++)
			{
				yield return _items[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}