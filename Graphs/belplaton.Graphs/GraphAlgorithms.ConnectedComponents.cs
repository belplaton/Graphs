namespace belplaton.Graphs;

public static partial class GraphAlgorithms
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
				DFSUtil(node, component);
				components.Add(component);
			}
		}

		return components;

		void DFSUtil(TNode current, List<TNode> component)
		{
			visited.Add(current);
			component.Add(current);

			for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
			{
				if (graph[current][adjIndex].HasValue && !visited.Contains(graph.Nodes[adjIndex]))
				{
					DFSUtil(graph.Nodes[adjIndex], component);
				}
			}
		}
	}
	
	public static List<List<TNode>> FindWeakConnectedComponents<TNode, TData>(IGraph<TNode, TData> graph)
	{
		var visited = new HashSet<TNode>();
		var components = new List<List<TNode>>();

		for (var i = 0; i < graph.Nodes.Count; i++)
		{
			var node = graph.Nodes[i];
			if (!visited.Contains(node))
			{
				var component = new List<TNode>();
				//smtng here
				components.Add(component);
			}
		}

		return components;
	}
}