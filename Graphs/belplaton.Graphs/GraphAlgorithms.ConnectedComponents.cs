namespace belplaton.Graphs;

public static partial class GraphAlgorithms
{
	public static List<List<TNode>> FindConnectedComponents<TNode, TData>(IGraph<TNode, TData> graph)
	{
		var stack = new Stack<TNode>();
		var visited = new HashSet<TNode>();
		var components = new List<List<TNode>>();

		for (var i = 0; i < graph.Nodes.Count; i++)
		{
			var node = graph.Nodes[i];
			if (!visited.Contains(node))
			{
				var component = new List<TNode>();
				using var enumerator = new DFSEnumerator<TNode, TData>(graph, node, stack, visited);
				while (enumerator.MoveNext()) { component.Add(enumerator.Current); }
				components.Add(component);
			}
			
			stack.Clear();
		}

		return components;
	}
	
	// TODO:
	public static List<List<TNode>> FindWeakConnectedComponents<TNode, TData>(IGraph<TNode, TData> graph)
	{
		var stack = new Stack<TNode>();
		var visited = new HashSet<TNode>();
		var components = new List<List<TNode>>();

		for (var i = 0; i < graph.Nodes.Count; i++)
		{
			var node = graph.Nodes[i];
			if (!visited.Contains(node))
			{
				var component = new List<TNode>();
				// smthng here
				using var enumerator = new DFSEnumerator<TNode, TData>(graph, node, stack, visited);
				while (enumerator.MoveNext())
				{
					component.Add(enumerator.Current);
				}
				
				components.Add(component);
			}
		}

		return components;
		
	}
}