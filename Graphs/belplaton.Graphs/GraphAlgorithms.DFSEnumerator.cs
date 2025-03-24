#nullable disable
using System.Collections;

namespace belplaton.Graphs;

public static partial class GraphAlgorithms
{
	public struct DFSEnumerator<TNode, TData> : IEnumerator<TNode>
	{
		private readonly IGraph<TNode, TData> _graph;
		private readonly TNode _startNode;
		private readonly Stack<TNode> _stack;
		private readonly HashSet<TNode> _visited;
		private bool _isStarted;

		public DFSEnumerator(IGraph<TNode, TData> graph, TNode startNode)
		{
			_graph = graph;
			_startNode = startNode;
			_stack = new Stack<TNode>();
			_visited = new HashSet<TNode>();
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

				_stack.Push(_startNode);
				_isStarted = true;
			}

			while (_stack.Count > 0)
			{
				var candidate = _stack.Pop();
				if (_visited.Contains(candidate)) continue;

				Current = candidate;
				_visited.Add(candidate);

				for (var adjIndex = 0; adjIndex < _graph.Size; adjIndex++)
				{
					if (_graph[candidate][adjIndex].HasValue && !_visited.Contains(_graph.Nodes[adjIndex]))
					{
						_stack.Push(_graph.Nodes[adjIndex]);
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
}