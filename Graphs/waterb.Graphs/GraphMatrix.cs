using System.Collections;
using System.Text;

namespace waterb.Graphs;

#nullable disable
public sealed class GraphMatrix<TNode, TData> : IGraph<TNode, TData>
{
	public int Size
	{
		get
		{
			lock (_operationsLock)
			{
				return _size;
			}
		}
		private set
		{
			if (_size != value)
			{
				if (value >= _capacity)
				{
					if (_capacity == 0) Capacity = 4;
					else
					{
						while (_capacity < value)
						{
							Capacity <<= 1;
						}
					}
				}
				
				_size = value;
			}
		}
	}
	public int Capacity
	{
		get => _capacity;
		private set
		{
			_capacity = value;
			if (_nodes != null) Array.Resize(ref _nodes, value);
			else _nodes = new TNode[value];

			if (_nodeData != null) Array.Resize(ref _nodeData, value);
			else _nodeData = new TData[value];

			var tempAdjacencyMatrix = new double?[value][];
			for (var i = 0; i < value; i++)
			{
				tempAdjacencyMatrix[i] = new double?[value];
				if (i < _size)
				{
					for (var j = 0; j < _size; j++)
					{
						tempAdjacencyMatrix[i][j] = _adjacencyMatrix[i][j];
					}
				}
			}

			_adjacencyMatrix = tempAdjacencyMatrix;
		}
	}
	
	public GraphSettings Settings { get; }
	public IReadOnlyList<TNode> Nodes
	{
		get
		{
			lock (_operationsLock)
			{
				return _nodes;
			}
		}
	}
	public IReadOnlyList<TData> NodeData
	{
		get
		{
			lock (_operationsLock)
			{
				return _nodeData;
			}
		}
	}
	
	private readonly object _operationsLock = new();

	private int _size;
	private int _capacity;
	private TNode[] _nodes;
	private TData[] _nodeData;
	private double?[][] _adjacencyMatrix;
	private readonly Dictionary<TNode, int> _keysToIndexes;
	
	public GraphMatrix(int initialCapacity = 4, GraphSettings settings = GraphSettings.None)
	{
		Size = 0;
		Settings = settings;
		Capacity = initialCapacity;
		_keysToIndexes = new Dictionary<TNode, int>(initialCapacity);
	}
	
	public GraphMatrix(GraphSettings settings = GraphSettings.None) : this(4, settings: settings)
	{

	}
		
#nullable enable
	
	public IReadOnlyList<IReadOnlyList<double?>> GetRawAdjacencyMatrix()
	{
		lock (_operationsLock)
		{
			return _adjacencyMatrix;
		}
	}

	public List<RibData<TNode>> GetIncidentRibs()
	{
		lock (_operationsLock)
		{
			var edges = new List<RibData<TNode>>();
			for (var fromIndex = 0; fromIndex < _size; fromIndex++)
			{
				for (var toIndex = 0; toIndex < _size; toIndex++)
				{
					var weight = _adjacencyMatrix[fromIndex][toIndex];
					if (weight.HasValue)
					{
						if ((Settings & GraphSettings.IsDirected) != 0 || fromIndex < toIndex)
						{
							edges.Add(new RibData<TNode>(_nodes[fromIndex], _nodes[toIndex], weight.Value));
						}

					}
				}
			}

			return edges;
		}
	}
	public List<RibData<TNode>>? GetIncidentRibs(TNode node)
	{
		lock (_operationsLock)
		{
			if (!_keysToIndexes.TryGetValue(node, out var fromIndex)) return null;
			
			var edges = new List<RibData<TNode>>();
			for (var toIndex = 0; toIndex < _size; toIndex++)
			{
				var weight = _adjacencyMatrix[fromIndex][toIndex];
				if (weight.HasValue)
				{
					edges.Add(new RibData<TNode>(_nodes[fromIndex], _nodes[toIndex], weight.Value));
				}
			}

			return edges;
		}
	}

	public IReadOnlyList<double?> this[TNode node]
	{
		get
		{
			lock (_operationsLock)
			{
				if (!_keysToIndexes.TryGetValue(node, out var index))
					throw new ArgumentException($"Node with key={node} is not presented in Graph!");
				return _adjacencyMatrix[index];
			}
		}
	}

	public IReadOnlyList<double?> this[int index]
	{
		get
		{
			lock (_operationsLock)
			{
				if (index >= Size)
					throw new AggregateException($"Index {index} is out of range {Size}!");
				return _adjacencyMatrix[index];
			}
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
	public int? GetIndex(TNode key)
	{
		lock (_operationsLock)
		{
			if (_keysToIndexes.TryGetValue(key, out var index)) return index;
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
			var index = _size;
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
			throw new ArgumentException($"Node with key={node} is already exists in Graph!");
	}
	public bool RemoveNode(TNode node)
	{
		lock (_operationsLock)
		{
			if (_keysToIndexes.Remove(node, out var index))
			{
				Size--;
				if (index != _size)
				{
					_nodes[index] = _nodes[_size];
					_nodeData[index] = _nodeData[_size];
					for (var i = 0; i < _size; i++)
					{
						_adjacencyMatrix[i][index] = _adjacencyMatrix[i][_size];
						_adjacencyMatrix[index][i] = _adjacencyMatrix[_size][i];
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
			return _adjacencyMatrix[fromIndex][toIndex].HasValue;
		}
	}
	public double? GetWeight(TNode from, TNode to)
	{
		lock (_operationsLock)
		{
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex)) return null;
			if (!_keysToIndexes.TryGetValue(to, out var toIndex)) return null;
			return _adjacencyMatrix[fromIndex][toIndex];
		}
	}
	public bool TrySetEdge(TNode from, TNode to, double? weight = 1)
	{
		lock (_operationsLock)
		{
			if ((Settings & GraphSettings.IsWeighted) == 0) return false;
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex)) return false;
			if (!_keysToIndexes.TryGetValue(to, out var toIndex)) return false;

			_adjacencyMatrix[fromIndex][toIndex] = weight ?? 1;
			if ((Settings & GraphSettings.IsDirected) == 0)
				_adjacencyMatrix[toIndex][fromIndex] = weight ?? 1;
			return true;
		}
	}
	public void SetEdge(TNode from, TNode to, double? weight = 1)
	{
		lock (_operationsLock)
		{
			if ((Settings & GraphSettings.IsWeighted) == 0 && weight.HasValue)
				throw new ArgumentException($"Graph is not weighed. Settings: {Settings}");
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex))
				throw new ArgumentException($"Node with key={from} is not presented in Graph!");
			if (!_keysToIndexes.TryGetValue(to, out var toIndex))
				throw new ArgumentException($"Node with key={to} is not presented in Graph!");

			_adjacencyMatrix[fromIndex][toIndex] = weight ?? 1;
			if ((Settings & GraphSettings.IsDirected) == 0)
				_adjacencyMatrix[toIndex][fromIndex] = weight ?? 1;
		}
	}
	public bool TryClearEdge(TNode from, TNode to)
	{
		lock (_operationsLock)
		{
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex)) return false;
			if (!_keysToIndexes.TryGetValue(to, out var toIndex)) return false;

			_adjacencyMatrix[fromIndex][toIndex] = null;
			if ((Settings & GraphSettings.IsDirected) == 0)
				_adjacencyMatrix[toIndex][fromIndex] = null;
			return true;
		}
	}
	public void ClearEdge(TNode from, TNode to)
	{
		lock (_operationsLock)
		{
			if (!_keysToIndexes.TryGetValue(from, out var fromIndex))
				throw new ArgumentException($"Node with key={from} is not presented in Graph!");
			if (!_keysToIndexes.TryGetValue(to, out var toIndex))
				throw new ArgumentException($"Node with key={to} is not presented in Graph!");

			_adjacencyMatrix[fromIndex][toIndex] = null;
			if ((Settings & GraphSettings.IsDirected) == 0)
				_adjacencyMatrix[toIndex][fromIndex] = null;
		}
	}

	public string PrepareGraphInfo()
	{
		lock (_operationsLock)
		{
			var sb = new StringBuilder();
			sb.AppendLine($"GraphMatrix: Size={_size}, Settings={Settings}");
			
			sb.AppendLine("Nodes: ");
			for (var i = 0; i < _size; i++)
			{
				sb.Append($"{_nodes[i],4:0}[{i}]");
			}
			
			sb.AppendLine();
			sb.AppendLine("Adjacency Matrix:");
			sb.Append("     ");
			for (var i = 0; i < _size; i++)
			{
				sb.Append($"{i,8:0.##}");
			}
			
			sb.AppendLine();
			for (var i = 0; i < _size; i++)
			{
				sb.Append($"{i,4}:");
				for (var j = 0; j < _size; j++)
				{
					var value = _adjacencyMatrix[i][j];
					if (value.HasValue) sb.Append($"{value.Value,8:0.##}");
					else sb.Append($"{"-",8}");
				}
				sb.AppendLine();
			}
        
			return sb.ToString();
		}
	}
}