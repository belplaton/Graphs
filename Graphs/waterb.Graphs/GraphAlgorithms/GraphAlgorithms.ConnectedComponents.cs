
namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public static List<List<TNode>>? FindConnectedComponents<TNode, TData>(this IGraph<TNode, TData> graph,
		ref HashSet<TNode>? visited, ref Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack)
	{
		if (graph.Size == 0) return null;
		
		(visited ??= new HashSet<TNode>()).Clear();
		(stack ??= new Stack<DFSEnumerator<TNode, TData>.DFSNode>()).Clear();
		var components = new List<List<TNode>>();

		for (var i = 0; i < graph.Nodes.Count; i++)
		{
			var node = graph.Nodes[i];
			if (!visited.Contains(node))
			{
				var component = new List<TNode>();
				using var enumerator = new DFSEnumerator<TNode, TData>(graph, node, visited, stack);
				while (enumerator.MoveNext()) component.Add(enumerator.Current.node);
				components.Add(component);
			}
		}

		return components;
	}
	
	public static List<List<TNode>>? FindWeakConnectedComponents<TNode, TData>(this IGraph<TNode, TData> graph,
		ref HashSet<TNode>? visited, ref Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack)
	{
		if (graph.Size == 0) return null;
		
		(visited ??= new HashSet<TNode>()).Clear();
		(stack ??= new Stack<DFSEnumerator<TNode, TData>.DFSNode>()).Clear();
		var components = new List<List<TNode>>();

		for (var i = 0; i < graph.Nodes.Count; i++)
		{
			var node = graph.Nodes[i];
			if (!visited.Contains(node))
			{
				var component = new List<TNode>();
				using var enumerator = new DFSEnumerator<TNode, TData>(graph, node, visited, stack, OnPrepareStackChangesWeakComponents);
				while (enumerator.MoveNext()) component.Add(enumerator.Current.node);
				components.Add(component);
			} 
		}

		return components;
		static void OnPrepareStackChangesWeakComponents(DFSEnumerator<TNode, TData>.DFSNode current, 
			IGraph<TNode, TData> graph, Stack<DFSEnumerator<TNode, TData>.DFSNode> stack, HashSet<TNode> visited)
		{
			var currentIndex = graph.GetIndex(current.node)!.Value;
			for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
			{
				if ((graph[currentIndex][adjIndex].HasValue || graph[adjIndex][currentIndex].HasValue) &&
				    !visited.Contains(graph.Nodes[adjIndex]))
				{
					stack.Push(new DFSEnumerator<TNode, TData>.DFSNode(graph.Nodes[adjIndex], current.depth + 1));
				}
			}
		}
	}

	/// <summary>
	/// Algorithm Kosaraju
	/// </summary>
	public static List<List<TNode>>? FindStrongConnectedComponents<TNode, TData>(this IGraph<TNode, TData> graph,
		ref HashSet<TNode>? visited, ref Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack)
	{
		if (graph.Size == 0) return null;
		
		(visited ??= new HashSet<TNode>()).Clear();
		(stack ??= new Stack<DFSEnumerator<TNode, TData>.DFSNode>()).Clear();

		var lastExitTime = 0;
		var nodesExitTime = new (TNode node, int exitTime)[graph.Nodes.Count];
		var reversalStack = new Stack<DFSEnumerator<TNode, TData>.DFSNode>();
		
		using var exitTimeEnumerator = new DFSEnumerator<TNode, TData>(graph, graph.Nodes[0], visited, stack);
		do
		{
			var currentNode = exitTimeEnumerator.MoveNext() 
				? exitTimeEnumerator.Current : (DFSEnumerator<TNode, TData>.DFSNode?)null;
			
			while (reversalStack.Count > 0 && reversalStack.Peek().depth >= exitTimeEnumerator.Current.depth)
			{
				var data = reversalStack.Pop();
				nodesExitTime[graph.GetIndex(data.node)!.Value] = (data.node, lastExitTime++);
			}
			
			if (currentNode != null)
			{
				nodesExitTime[graph.GetIndex(currentNode.Value.node)!.Value] = (currentNode.Value.node, lastExitTime++);
				reversalStack.Push(currentNode.Value);
			}
			
		} while (reversalStack.Count > 0);

		Array.Sort(nodesExitTime, (a, b) => b.exitTime.CompareTo(a.exitTime));
		visited.Clear();
		stack.Clear();
		
		var components = new List<List<TNode>>();
		for (var i = 0; i < nodesExitTime.Length; i++)
		{
			var node = nodesExitTime[i].node;
			if (!visited.Contains(node))
			{
				var component = new List<TNode>();
				using var componentEnumerator = new DFSEnumerator<TNode, TData>(graph, node,
					visited, stack, OnPrepareStackChangesStrongComponents);
				while (componentEnumerator.MoveNext()) component.Add(componentEnumerator.Current.node);
				components.Add(component);
			} 
		}

		return components;
		static void OnPrepareStackChangesStrongComponents(DFSEnumerator<TNode, TData>.DFSNode current,
			IGraph<TNode, TData> graph, Stack<DFSEnumerator<TNode, TData>.DFSNode> stack, HashSet<TNode> visited)
		{
			var currentIndex = graph.GetIndex(current.node)!.Value;
			for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
			{
				if (graph[adjIndex][currentIndex].HasValue && !visited.Contains(graph.Nodes[adjIndex]))
				{
					stack.Push(new DFSEnumerator<TNode, TData>.DFSNode(graph.Nodes[adjIndex], current.depth + 1));
				}
			}
		}
	}
}