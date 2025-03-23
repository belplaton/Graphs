namespace belplaton.Graphs;

#nullable disable
public sealed class GraphMatrix<TNode, TData> : IGraph<TNode, TData> where TNode : notnull
{
	public int Size
	{
		get => _size;
		private set
		{
			if (_size != value)
			{
				_size = value;
				if (value >= _capacity)
				{
					if (_capacity == 0) Capacity = 4;
					else
					{
						while (_capacity < value)
						{
							_capacity <<= 1;
						}
					}
				}
			}
		}
	}
	public int Capacity
	{
		get => _capacity;
		private set
		{
			lock (_capacityLock)
			{
				_capacity = value;
				if (_nodes != null) Array.Resize(ref _nodes, value);
				else _nodes = new TNode[value];

				if (_nodeData != null) Array.Resize(ref _nodeData, value);
				else _nodeData = new TData[value];

				var tempAdjacencyMatrix = new double?[value, value];
				for (var i = 0; i < Size; i++)
				{
					for (var j = 0; j < Size; j++)
					{
						tempAdjacencyMatrix[i, j] = _adjacencyMatrix[i, j];
					}
				}

				_adjacencyMatrix = tempAdjacencyMatrix;
			}
		}
	}
	public GraphSettings Settings { get; }
	public IReadOnlyList<TNode> Nodes => _nodes;
	public IReadOnlyList<TData> NodeData => _nodeData;

	private readonly object _capacityLock = new();
	private readonly object _operationsLock = new();

	private int _size;
	private int _capacity;
	private TNode[] _nodes;
	private TData[] _nodeData;
	private double?[,] _adjacencyMatrix;
	private readonly Dictionary<TNode, int> _keysToIndexes;

	public GraphMatrix(int initialCapacity, GraphSettings? settings = default)
	{
		Size = 0;
		Settings = settings ?? default;
		Capacity = initialCapacity;
		_keysToIndexes = new Dictionary<TNode, int>(initialCapacity);
	}
		
#nullable enable
	public double?[,] GetRawAdjacencyMatrix()
	{
		return _adjacencyMatrix;
	}
	public List<(int index, double weight)>? GetRawAdjacencyList(TNode node)
	{
		lock (_operationsLock)
		{
			if (!_keysToIndexes.TryGetValue(node, out var fromIndex)) return null;

			var list = new List<(int index, double weight)>();
			for (var toIndex = 0; toIndex < Size; toIndex++)
			{
				var weight = _adjacencyMatrix[fromIndex, toIndex];
				if (weight.HasValue) list.Add((toIndex, weight.Value));
			}

			return list;
		}
	}
		
	public TData? GetData(TNode key)
	{
		lock (_operationsLock)
		{
			if (_keysToIndexes.TryGetValue(key, out var index)) return _nodeData[index];
			return default;
		}
	}
	public void SetData(TNode node, TData data)
	{
		if (!TryAddNode(node, data))
		{
			lock (_operationsLock)
			{
				_nodeData[_keysToIndexes[node]] = data;
			}
		}
	}
	public bool TryAddNode(TNode node, TData data)
	{
		lock (_operationsLock)
		{
			var index = Size;
			if (_keysToIndexes.TryAdd(node, index))
			{
				Size++;
				_nodes[index] = node;
				_nodeData[index] = data;
				
				return true;
			}

			return false;
		}
	}
	public void AddNode(TNode node, TData data)
	{
		if (!TryAddNode(node, data))
			throw new ArgumentException($"Node with {node} is already exists in Graph!");
	}
	public bool RemoveNode(TNode node)
	{
		lock (_operationsLock)
		{
			if (_keysToIndexes.Remove(node, out var index))
			{
				Size--;
				if (index != Size)
				{
					_nodes[index] = _nodes[Size];
					_nodeData[index] = _nodeData[Size];
					for (var i = 0; i < Size; i++)
					{
						_adjacencyMatrix[i, index] = _adjacencyMatrix[i, Size];
						_adjacencyMatrix[index, i] = _adjacencyMatrix[Size, i];
					}
				}

				_keysToIndexes[_nodes[index]] = index;
				return true;
			}

			return false;
		}
	}
	
	public bool? GetEdge(TNode from, TNode to)
	{
		lock (_operationsLock)
		{
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex)) return null;
			if (!_keysToIndexes.TryGetValue(to, out var toIndex)) return null;
			return _adjacencyMatrix[fromIndex, toIndex].HasValue;
		}
	}
	public double? GetWeight(TNode from, TNode to)
	{
		lock (_operationsLock)
		{
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex)) return null;
			if (!_keysToIndexes.TryGetValue(to, out var toIndex)) return null;
			return _adjacencyMatrix[fromIndex, toIndex];
		}
	}
	public bool TrySetEdgeData(TNode from, TNode to, double? weight)
	{
		lock (_operationsLock)
		{
			if ((Settings & GraphSettings.IsWeighted) == 0) return false;
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex)) return false;
			if (!_keysToIndexes.TryGetValue(to, out var toIndex)) return false;

			_adjacencyMatrix[fromIndex, toIndex] = weight ?? 0;
			if ((Settings & GraphSettings.IsDirected) == 0)
				_adjacencyMatrix[toIndex, fromIndex] = weight ?? 0;
			return true;
		}
	}
	public void SetEdgeData(TNode from, TNode to, double? weight)
	{
		lock (_operationsLock)
		{
			if ((Settings & GraphSettings.IsWeighted) == 0 && weight.HasValue)
				throw new ArgumentException($"Graph is not weighed. Settings: {Settings}");
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex))
				throw new ArgumentException($"Key {from} is not presented in Graph!");
			if (!_keysToIndexes.TryGetValue(to, out var toIndex))
				throw new ArgumentException($"Key {to} is not presented in Graph!");

			_adjacencyMatrix[fromIndex, toIndex] = weight ?? 0;
			if ((Settings & GraphSettings.IsDirected) == 0)
				_adjacencyMatrix[toIndex, fromIndex] = weight ?? 0;
		}
	}
	public bool TryClearEdge(TNode from, TNode to)
	{
		lock (_operationsLock)
		{
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex)) return false;
			if (!_keysToIndexes.TryGetValue(to, out var toIndex)) return false;

			_adjacencyMatrix[fromIndex, toIndex] = null;
			if ((Settings & GraphSettings.IsDirected) == 0)
				_adjacencyMatrix[toIndex, fromIndex] = null;
			return true;
		}
	}
	public void ClearEdge(TNode from, TNode to)
	{
		lock (_operationsLock)
		{
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex))
				throw new ArgumentException($"Key {from} is not presented in Graph!");
			if (!_keysToIndexes.TryGetValue(to, out var toIndex))
				throw new ArgumentException($"Key {to} is not presented in Graph!");

			_adjacencyMatrix[fromIndex, toIndex] = null;
			if ((Settings & GraphSettings.IsDirected) == 0)
				_adjacencyMatrix[toIndex, fromIndex] = null;
		}
	}
}