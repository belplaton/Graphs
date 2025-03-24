#nullable disable
using System.Collections;

namespace belplaton.Graphs;

public struct DFSEnumerator<TGraph, TNode, TData> : IEnumerator<TNode>
	where TGraph : IGraph<TNode, TData>
{
	private readonly TGraph _graph;
	private readonly int? _startNode;
	private readonly Stack<int> _stack;
	private readonly HashSet<int> _visited;
	private bool _isStarted;

	public DFSEnumerator(TGraph graph, TNode startNode)
	{
		_graph = graph;
		_startNode = graph.GetIndex(startNode);
		_stack = new Stack<int>();
		_visited = new HashSet<int>();
		Current = default;
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

			_stack.Push(_startNode.Value);
			_isStarted = true;
		}
		
		while (_stack.Count > 0)
		{
			var candidateIndex = _stack.Pop();
			if (_visited.Contains(candidateIndex)) continue;
            
			Current = _graph.Nodes[candidateIndex];
			_visited.Add(candidateIndex);
			
			for (var adjIndex = 0; adjIndex < _graph.Size; adjIndex++)
			{
				if (_graph[candidateIndex][adjIndex].HasValue && !_visited.Contains(adjIndex))
				{
					_stack.Push(adjIndex);
				}
			}
            
			return true;
		}
        
		return false;
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