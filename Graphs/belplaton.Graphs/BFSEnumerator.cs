#nullable disable
using System.Collections;

namespace belplaton.Graphs;

public struct BFSEnumerator<TNode, TData> : IEnumerator<TNode>
{
	private readonly IGraph<TNode, TData> _graph;
	private readonly int? _startNode;
	private readonly Queue<int> _queue;
	private readonly HashSet<int> _visited;
	private bool _isStarted;

	public BFSEnumerator(IGraph<TNode, TData> graph, TNode startNode)
	{
		_graph = graph;
		_startNode = graph.GetIndex(startNode);
		_queue = new Queue<int>();
		_visited = new HashSet<int>();
		Current = default;
		_isStarted = false;
	}
    
	public TNode Current { get; private set; }

	object IEnumerator.Current => Current;

	public bool MoveNext()
	{
		if (!_isStarted)
		{
			if (_startNode == null)
			{
				return false;
			}

			_queue.Enqueue(_startNode.Value);
			_visited.Add(_startNode.Value);
			_isStarted = true;
		}

		if (_queue.Count == 0) return false;
		
		var candidateIndex = _queue.Dequeue();
		Current = _graph.Nodes[candidateIndex];
		
		for (var adjIndex = 0; adjIndex < _graph.Size; adjIndex++)
		{
			if (_graph[candidateIndex][adjIndex].HasValue && _visited.Add(adjIndex))
			{
				_queue.Enqueue(adjIndex);
			}
		}
		
		return true;
	}

	public void Reset()
	{
		Current = default;
		_isStarted = false;
	}

	public void Dispose()
	{

	}
}