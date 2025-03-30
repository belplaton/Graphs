#nullable disable
using System.Collections;

namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public struct DFSEnumerator<TNode, TData> : IEnumerator<DFSEnumerator<TNode, TData>.DFSNode>
	{
		public readonly struct DFSNode
		{
			public readonly TNode node;
			public readonly int depth = -1;

			public DFSNode(TNode node, int depth)
			{
				this.node = node;
				this.depth = depth;
			}
		}
		
		private readonly IGraph<TNode, TData> _graph;
		private readonly TNode _startNode;
		private readonly Stack<DFSNode> _stack;
		private readonly HashSet<TNode> _visited;
		private readonly Action<DFSNode, IGraph<TNode, TData>, Stack<DFSNode>, HashSet<TNode>> _onPrepareStackChanges;
		private bool _isStarted;

		public DFSEnumerator(IGraph<TNode, TData> graph, TNode startNode,
			HashSet<TNode> visited, Stack<DFSNode> stack,
			Action<DFSNode, IGraph<TNode, TData>, Stack<DFSNode>, HashSet<TNode>> onPrepareStackChanges = null)
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
			Action<DFSNode, IGraph<TNode, TData>, Stack<DFSNode>, HashSet<TNode>> onPrepareStackChanges = null)
			: this(graph, startNode, new HashSet<TNode>(), new Stack<DFSNode>(),
				onPrepareStackChanges)
		{

		}

		public DFSNode Current { get; private set; }

		object IEnumerator.Current => Current;

		public bool MoveNext()
		{
			if (!_isStarted)
			{
				if (_startNode == null)
				{
					return false;
				}

				_stack.Push(new DFSNode(_startNode, 0));
				_isStarted = true;
			}

			while (_stack.Count > 0)
			{
				var candidate = _stack.Pop();
				
				if (_visited.Contains(candidate.node)) continue;

				Current = candidate;
				
				_visited.Add(candidate.node);
				_onPrepareStackChanges.Invoke(candidate, _graph, _stack, _visited);
				return true;
			}

			Current = default;
			return false;
		}

		private static void OnPrepareStackChangesDefault(DFSNode current, IGraph<TNode, TData> graph,
			Stack<DFSNode> stack, HashSet<TNode> visited)
		{
			for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
			{
				if (graph[current.node][adjIndex].HasValue && !visited.Contains(graph.Nodes[adjIndex]))
				{
					stack.Push(new DFSNode(graph.Nodes[adjIndex], current.depth + 1));
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