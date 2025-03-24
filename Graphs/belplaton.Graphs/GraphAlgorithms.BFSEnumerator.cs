#nullable disable
using System.Collections;

namespace belplaton.Graphs;

public static partial class GraphAlgorithms
{
	public struct BFSEnumerator<TNode, TData> : IEnumerator<TNode>
	{
		private readonly IGraph<TNode, TData> _graph;
		private readonly TNode _startNode;
		private readonly Queue<TNode> _queue;
		private readonly HashSet<TNode> _visited;
		private bool _isStarted;

		public BFSEnumerator(IGraph<TNode, TData> graph, TNode startNode)
		{
			_graph = graph;
			_startNode = startNode;
			_queue = new Queue<TNode>();
			_visited = new HashSet<TNode>();
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

				_queue.Enqueue(_startNode);
				_visited.Add(_startNode);
				_isStarted = true;
			}

			if (_queue.Count == 0) return false;

			Current = _queue.Dequeue();

			for (var adjIndex = 0; adjIndex < _graph.Size; adjIndex++)
			{
				if (_graph[Current][adjIndex].HasValue && _visited.Add(_graph.Nodes[adjIndex]))
				{
					_queue.Enqueue(_graph.Nodes[adjIndex]);
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
}