#nullable disable
using System.Collections;

namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public struct DFSEnumerator<TNode, TData> : IEnumerator<TNode>
	{
		private readonly IGraph<TNode, TData> _graph;
		private readonly TNode _startNode;
		private readonly Stack<TNode> _stack;
		private readonly HashSet<TNode> _visited;
		private readonly Action<TNode, IGraph<TNode, TData>, Stack<TNode>, HashSet<TNode>> _onPrepareStackChanges;
		private bool _isStarted;

		public DFSEnumerator(IGraph<TNode, TData> graph, TNode startNode,
			HashSet<TNode> visited, Stack<TNode> stack,
			Action<TNode, IGraph<TNode, TData>, Stack<TNode>, HashSet<TNode>> onPrepareStackChanges = null)
		{
			_graph = graph;
			_startNode = startNode;
			_stack = stack;
			_visited = visited;
			_onPrepareStackChanges = onPrepareStackChanges ?? OnPrepareStackChangesDefault;
			Current = default;
			_isStarted = false;
		}
		
		public DFSEnumerator(IGraph<TNode, TData> graph, TNode startNode,
			Action<TNode, IGraph<TNode, TData>, Stack<TNode>, HashSet<TNode>> onPrepareStackChanges = null)
			: this(graph, startNode, new HashSet<TNode>(), new Stack<TNode>(), onPrepareStackChanges)
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

				_stack.Push(_startNode);
				_isStarted = true;
			}

			while (_stack.Count > 0)
			{
				var candidate = _stack.Pop();
				if (_visited.Contains(candidate)) continue;

				Current = candidate;
				_visited.Add(candidate);

				_onPrepareStackChanges.Invoke(candidate, _graph, _stack, _visited);
				return true;
			}

			return false;
		}

		private static void OnPrepareStackChangesDefault(TNode current, IGraph<TNode, TData> graph,
			Stack<TNode> stack, HashSet<TNode> visited)
		{
			for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
			{
				if (graph[current][adjIndex].HasValue && !visited.Contains(graph.Nodes[adjIndex]))
				{
					stack.Push(graph.Nodes[adjIndex]);
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