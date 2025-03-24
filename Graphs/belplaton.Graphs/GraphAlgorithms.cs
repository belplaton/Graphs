namespace belplaton.Graphs;

public static class GraphAlgorithms
{
	public static List<List<TNode>> FindConnectedComponents<TNode, TData>(IGraph<TNode, TData> graph)
	{
		var visited = new HashSet<TNode>();
		var components = new List<List<TNode>>();

		for (var i = 0; i < graph.Nodes.Count; i++)
		{
			var node = graph.Nodes[i];
			if (!visited.Contains(node))
			{
				var component = new List<TNode>();
				DFSUtil(graph, node, visited, component);
				components.Add(component);
			}
		}

		return components;
	}
	
	private static void DFSUtil<TNode, TData>(IGraph<TNode, TData> graph, TNode current, 
		HashSet<TNode> visited, List<TNode> component)
	{
		visited.Add(current);
		component.Add(current);

		for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
		{
			if (graph[current][adjIndex].HasValue && !visited.Contains(graph.Nodes[adjIndex]))
			{
				DFSUtil(graph, graph.Nodes[adjIndex], visited, component);
			}
		}
	}
}