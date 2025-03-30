
namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public static List<List<TNode>>? FindConnectedComponents<TNode, TData>(this IGraph<TNode, TData> graph,
		ref HashSet<TNode>? visited, ref Stack<TNode>? stack)
	{
		if (graph.Size == 0) return null;
		
		(visited ??= new HashSet<TNode>()).Clear();
		(stack ??= new Stack<TNode>()).Clear();
		var components = new List<List<TNode>>();

		for (var i = 0; i < graph.Nodes.Count; i++)
		{
			var node = graph.Nodes[i];
			if (!visited.Contains(node))
			{
				var component = new List<TNode>();
				using var enumerator = new DFSEnumerator<TNode, TData>(graph, node, visited, stack);
				while (enumerator.MoveNext()) component.Add(enumerator.Current);
				components.Add(component);
			}
		}

		return components;
	}
	
	public static List<List<TNode>>? FindWeakConnectedComponents<TNode, TData>(this IGraph<TNode, TData> graph,
		ref HashSet<TNode>? visited, ref Stack<TNode>? stack)
	{
		if (graph.Size == 0) return null;
		
		(visited ??= new HashSet<TNode>()).Clear();
		(stack ??= new Stack<TNode>()).Clear();
		var components = new List<List<TNode>>();

		for (var i = 0; i < graph.Nodes.Count; i++)
		{
			var node = graph.Nodes[i];
			if (!visited.Contains(node))
			{
				var component = new List<TNode>();
				using var enumerator = new DFSEnumerator<TNode, TData>(graph, node, visited, stack, OnPrepareStackChangesWeakComponents);
				while (enumerator.MoveNext()) component.Add(enumerator.Current);
				components.Add(component);
			} 
		}

		return components;
		static void OnPrepareStackChangesWeakComponents(TNode current, IGraph<TNode, TData> graph, Stack<TNode> stack, HashSet<TNode> visited)
		{
			var currentIndex = graph.GetIndex(current)!.Value;
			for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
			{
				if ((graph[currentIndex][adjIndex].HasValue || graph[adjIndex][currentIndex].HasValue) &&
				    !visited.Contains(graph.Nodes[adjIndex]))
				{
					stack.Push(graph.Nodes[adjIndex]);
				}
			}
		}
	}

	public static List<List<TNode>>? FindStrongConnectedComponents<TNode, TData>(this IGraph<TNode, TData> graph,
		ref HashSet<TNode>? visited, ref Stack<TNode>? stack)
	{
		if (graph.Size == 0) return null;
		
		(visited ??= new HashSet<TNode>()).Clear();
		(stack ??= new Stack<TNode>()).Clear();

		var lastExitTime = 0;
		var nodesExitTime = new int[graph.Nodes.Count];
		var reversalStack = new Stack<TNode>();
		
		var prevNode = default(TNode);
		using var exitTimeEnumerator = new DFSEnumerator<TNode, TData>(graph, graph.Nodes[0], visited, stack);
		while (exitTimeEnumerator.MoveNext())
		{
			if () // проверять тут, что мы не скаканули в другое место.
			{
				nodesExitTime[graph.GetIndex(exitTimeEnumerator.Current)!.Value] = lastExitTime++;
				reversalStack.Push(exitTimeEnumerator.Current);
			}
			
			while (reversalStack.Count > 0)
				nodesExitTime[graph.GetIndex(reversalStack.Pop())!.Value] = lastExitTime++;
		}

		// sort exit times
		
		visited.Clear();
		stack.Clear();
		
		var components = new List<List<TNode>>();
		for (var i = 0; i < nodesExitTime.Length; i++)
		{
			var node = graph.Nodes[nodesExitTime[i]];
			if (!visited.Contains(node))
			{
				var component = new List<TNode>();
				using var componentEnumerator = new DFSEnumerator<TNode, TData>(graph, node,
					visited, stack, OnPrepareStackChangesStrongComponents);
				while (componentEnumerator.MoveNext()) component.Add(componentEnumerator.Current);
				components.Add(component);
			} 
		}

		return components;
		static void OnPrepareStackChangesStrongComponents(TNode current, IGraph<TNode, TData> graph, Stack<TNode> stack, HashSet<TNode> visited)
		{
			var currentIndex = graph.GetIndex(current)!.Value;
			for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
			{
				if (graph[adjIndex][currentIndex].HasValue && !visited.Contains(graph.Nodes[adjIndex]))
				{
					stack.Push(graph.Nodes[adjIndex]);
				}
			}
		}
	}

}