#nullable disable
using System.Collections;

namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public struct DFSEnumerator<TNode, TData, TDFSNode> : IDFSEnumerator<TNode, TData, TDFSNode>
		where TDFSNode : struct, IDFSEnumerator<TNode, TData, TDFSNode>.IDFSNode
	{
		private readonly IGraph<TNode, TData> _graph;
		private readonly TNode _startNode;
		private readonly Stack<TDFSNode> _stack;
		private readonly HashSet<TNode> _visited;
		private readonly Action<TDFSNode, IGraph<TNode, TData>, Stack<TDFSNode>, HashSet<TNode>> _onPrepareStackChanges;
		private bool _isStarted;

		public DFSEnumerator(IGraph<TNode, TData> graph, TNode startNode,
			HashSet<TNode> visited, Stack<TDFSNode> stack,
			Action<TDFSNode, IGraph<TNode, TData>, Stack<TDFSNode>, HashSet<TNode>> onPrepareStackChanges = null)
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
			Action<TDFSNode, IGraph<TNode, TData>, Stack<TDFSNode>, HashSet<TNode>> onPrepareStackChanges = null)
			: this(graph, startNode, new HashSet<TNode>(), new Stack<TDFSNode>(),
				onPrepareStackChanges)
		{

		}

		public TDFSNode Current { get; private set; }

		object IEnumerator.Current => Current;

		public bool MoveNext()
		{
			if (!_isStarted)
			{
				if (_startNode == null)
				{
					return false;
				}

				_stack.Push(new TDFSNode { Node = _startNode, Depth = 0 });
				_isStarted = true;
			}

			while (_stack.Count > 0)
			{
				var candidate = _stack.Pop();
				_onPrepareStackChanges.Invoke(candidate, _graph, _stack, _visited);
				if (_visited.Add(candidate.Node))
				{
					Current = candidate;
					return true;
				}
			}

			Current = default;
			return false;
		}

		private static void OnPrepareStackChangesDefault(TDFSNode current, IGraph<TNode, TData> graph,
			Stack<TDFSNode> stack, HashSet<TNode> visited)
		{
			for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
			{
				if (graph[current.Node][adjIndex].HasValue && !visited.Contains(graph.Nodes[adjIndex]))
				{
					stack.Push(new TDFSNode { Node = graph.Nodes[adjIndex], Depth = current.Depth + 1 });
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
	
	public struct DFSEnumerator<TNode, TData> : IDFSEnumerator<TNode, TData, DFSEnumerator<TNode, TData>.DFSNode>
	{
		public readonly struct DFSNode() : IDFSEnumerator<TNode, TData, DFSNode>.IDFSNode
		{
			public TNode Node { get; init; } = default;
			public int Depth { get; init; } = -1;

			public DFSNode(TNode node, int depth) : this()
			{
				Node = node;
				Depth = depth;
			}
		}

		private DFSEnumerator<TNode, TData, DFSNode> _dfsEnumeratorInternal;
		public DFSEnumerator(IGraph<TNode, TData> graph, TNode startNode,
			HashSet<TNode> visited, Stack<DFSNode> stack,
			Action<DFSNode, IGraph<TNode, TData>, Stack<DFSNode>, HashSet<TNode>> onPrepareStackChanges = null)
		{
			_dfsEnumeratorInternal = new DFSEnumerator<TNode, TData, DFSNode>(
				graph, startNode, visited, stack, onPrepareStackChanges);
		}
		
		public DFSEnumerator(IGraph<TNode, TData> graph, TNode startNode,
			Action<DFSNode, IGraph<TNode, TData>, Stack<DFSNode>, HashSet<TNode>> onPrepareStackChanges = null)
			: this(graph, startNode, [], new Stack<DFSNode>(), onPrepareStackChanges)
		{

		}

		public DFSNode Current => _dfsEnumeratorInternal.Current;
		object IEnumerator.Current => Current;

		public bool MoveNext() => _dfsEnumeratorInternal.MoveNext();
		public void Reset() => _dfsEnumeratorInternal.Reset();
		public void Dispose() => _dfsEnumeratorInternal.Reset();
	}
}