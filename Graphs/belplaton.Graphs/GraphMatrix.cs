namespace belplaton.Graphs
{
	public sealed class GraphMatrix<TKey, TNode> : IGraph<TKey, TNode> where TKey : notnull
	{
		public int Size { get; }

		public int Capacity
		{
			get => _capacity;
			set
			{
				_nodes.
			}
		}
		
		public GraphSettings Settings { get; }
		public IReadOnlyList<TNode> Nodes => _nodes;

		private int _capacity;
		private readonly Dictionary<TKey, int> _keysToIndexes;
		private readonly TKey[] _keys;
		private readonly TNode[] _nodes;
		private readonly double?[,] _adjacencyMatrix;

		public GraphMatrix(int initialCapacity, GraphSettings? settings = default)
		{
			Size = initialCapacity;
			Settings = settings ?? default;
			_keysToIndexes = new Dictionary<TKey, int>(initialCapacity);
			_keys = new TKey[initialCapacity];
			_nodes = new TNode[initialCapacity];
			_adjacencyMatrix = new double?[initialCapacity, initialCapacity];
		}
		
		public (double?[,] adjacency, TKey[] keys, TNode[] nodeData) GetRawAdjacencyMatrix()
		{
			return (_adjacencyMatrix, _keys, _nodes);
		}

		public (List<(int index, double weight)> adjacency, TKey[] keys, TNode[] nodeData)? GetRawAdjacencyList(TKey key)
		{
			if (!_keysToIndexes.TryGetValue(key, out var fromIndex)) return null;
			
			var list = new List<(int index, double weight)>();
			for (var toIndex = 0; toIndex < Size; toIndex++)
			{
				var weight = _adjacencyMatrix[fromIndex, toIndex];
				if (weight.HasValue) list.Add((toIndex, weight.Value));
			}

			return (list, _keys, _nodes);
		}
		
		public TNode? GetNode(TKey key)
		{
			if (_keysToIndexes.TryGetValue(key, out var index)) return _nodes[index];
			return default;
		}
		public bool TrySetNode(TKey key, TNode node)
		{
			if (!_keysToIndexes.TryGetValue(key, out var index)) return false;
			_nodes[index] = node;
			return true;
		}
		
		public void SetNode(TKey key, TNode node)
		{
			if (!TrySetNode(key, node))
				throw new ArgumentException($"Key {key} is not presented in Graph!");
		}
		
		public bool? GetEdge(TKey from, TKey to)
		{
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex)) return null;
			if (!_keysToIndexes.TryGetValue(to, out var toIndex)) return null;
			return _adjacencyMatrix[fromIndex, toIndex].HasValue;
		}
		
		public double? GetWeight(TKey from, TKey to)
		{
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex)) return null;
			if (!_keysToIndexes.TryGetValue(to, out var toIndex)) return null;
			return _adjacencyMatrix[fromIndex, toIndex];
		}

		public bool TrySetEdgeData(TKey from, TKey to, double? weight)
		{
			if ((Settings & GraphSettings.IsWeighed) == 0) return false;
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex)) return false;
			if (!_keysToIndexes.TryGetValue(to, out var toIndex)) return false;

			_adjacencyMatrix[fromIndex, toIndex] = weight ?? 0;
			if ((Settings & GraphSettings.IsDirected) != 0)
				_adjacencyMatrix[toIndex, fromIndex] = weight ?? 0;
			return true;
		}
		
		public void SetEdgeData(TKey from, TKey to, double? weight)
		{
			if ((Settings & GraphSettings.IsWeighed) == 0 && weight.HasValue)
				throw new ArgumentException($"Graph is not weighed. Settings: {Settings}");
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex))
				throw new ArgumentException($"Key {from} is not presented in Graph!");
			if (!_keysToIndexes.TryGetValue(to, out var toIndex))
				throw new ArgumentException($"Key {to} is not presented in Graph!");

			_adjacencyMatrix[fromIndex, toIndex] = weight ?? 0;
			if ((Settings & GraphSettings.IsDirected) != 0)
				_adjacencyMatrix[toIndex, fromIndex] = weight ?? 0;
		}
		
		public bool TryClearEdge(TKey from, TKey to)
		{
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex)) return false;
			if (!_keysToIndexes.TryGetValue(to, out var toIndex)) return false;

			_adjacencyMatrix[fromIndex, toIndex] = null;
			if ((Settings & GraphSettings.IsDirected) != 0)
				_adjacencyMatrix[toIndex, fromIndex] = null;
			return true;
		}
		
		public void ClearEdge(TKey from, TKey to)
		{
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex))
				throw new ArgumentException($"Key {from} is not presented in Graph!");
			if (!_keysToIndexes.TryGetValue(to, out var toIndex))
				throw new ArgumentException($"Key {to} is not presented in Graph!");

			_adjacencyMatrix[fromIndex, toIndex] = null;
			if ((Settings & GraphSettings.IsDirected) != 0)
				_adjacencyMatrix[toIndex, fromIndex] = null;
		}
	}
}