#nullable disable
using System.Collections;

namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public struct BFSEnumerator<TNode, TData> : IEnumerator<TNode>
	{
		private readonly IGraph<TNode, TData> _graph;
		private readonly TNode _startNode;
		private readonly Queue<TNode> _queue;
		private readonly HashSet<TNode> _visited;
		private readonly Action<TNode, IGraph<TNode, TData>, Queue<TNode>, HashSet<TNode>> _onPrepareQueueChanges;
		private bool _isStarted;

		public BFSEnumerator(IGraph<TNode, TData> graph, TNode startNode,
			HashSet<TNode> visited, Queue<TNode> queue,
			Action<TNode, IGraph<TNode, TData>, Queue<TNode>, HashSet<TNode>> onPrepareQueueChanges = null)
		{
			_graph = graph;
			_startNode = startNode;
			_queue = queue;
			_visited = visited;
			_onPrepareQueueChanges = onPrepareQueueChanges ?? OnPrepareQueueChangesDefault;
			Current = default;
			_isStarted = false;
		}
		
		public BFSEnumerator(IGraph<TNode, TData> graph, TNode startNode,
			Action<TNode, IGraph<TNode, TData>, Queue<TNode>, HashSet<TNode>> onPrepareQueueChanges = null)
			: this(graph, startNode, new HashSet<TNode>(), new Queue<TNode>(), onPrepareQueueChanges)
		{

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

			if (_queue.Count == 0)
			{
				Current = default;
				return false;	
			}
			
			Current = _queue.Dequeue();

			_onPrepareQueueChanges.Invoke(Current, _graph, _queue, _visited);
			return true;
		}
		
		private static void OnPrepareQueueChangesDefault(TNode current, IGraph<TNode, TData> graph,
			Queue<TNode> queue, HashSet<TNode> visited)
		{
			for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
			{
				if (graph[current][adjIndex].HasValue && visited.Add(graph.Nodes[adjIndex]))
				{
					queue.Enqueue(graph.Nodes[adjIndex]);
				}
			}
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