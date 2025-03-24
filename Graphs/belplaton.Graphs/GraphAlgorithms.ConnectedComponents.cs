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
				using var enumerator = new DFSEnumerator<TNode, TData>(graph, node, visited);
				while (enumerator.MoveNext()) { component.Add(enumerator.Current); }
				components.Add(component);
			}
		}

		return components;
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