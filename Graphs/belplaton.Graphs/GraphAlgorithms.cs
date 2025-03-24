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
	
	/*
	private static void DFSUtil<TNode, TData>(IGraph<TNode, TData> graph, TNode current, 
		HashSet<TNode> visited, List<TNode> component)
	{
		visited.Add(current);
		component.Add(current);

		for (var adjIndex = 0; adjIndex < _graph.Size; adjIndex++)
		{
			if (_graph[candidate][adjIndex].HasValue && !_visited.Contains(_graph.Nodes[adjIndex]))
			{
				_stack.Push(_graph.Nodes[adjIndex]);
			}

		}

		var adjacency = graph.GetAdjacencyVertexes(current);
		if (adjacency == null)
			return;

		foreach (var (adjIndex, _) in adjacency)
		{
			var adjacentNode = graph.Nodes[adjIndex];
			if (!visited.Contains(adjacentNode))
			{
				DFSUtil(graph, adjacentNode, visited, component);
			}
		}
	}
	*/
}