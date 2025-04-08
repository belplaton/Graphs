namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public static List<RibData<TNode>>? BuildSpanningTreeDFS<TNode, TData>(
		this IGraph<TNode, TData> graph)
		where TNode : notnull
	{
		HashSet<TNode>? visited = [];
		Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack = [];
		return graph.BuildSpanningTreeDFS(ref visited, ref stack);
	}
	
	public static List<RibData<TNode>>? BuildSpanningTreeDFS<TNode, TData>(
		this IGraph<TNode, TData> graph, ref HashSet<TNode>? visited, 
		ref Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack)
		where TNode : notnull
	{
		if (graph.Size == 0) return null;
		
		(visited ??= new HashSet<TNode>()).Clear();
		(stack ??= new Stack<DFSEnumerator<TNode, TData>.DFSNode>()).Clear();

		var spanningTreeEdges = new List<RibData<TNode>>();
		using var enumerator = new DFSEnumerator<TNode, TData>(graph, graph.Nodes[0], visited, stack, 
			(current, g, s, v) => OnPrepareStackChanges(
				current, g, s, v, spanningTreeEdges));
		while (enumerator.MoveNext()) {}

		return spanningTreeEdges;
		
		static void OnPrepareStackChanges(DFSEnumerator<TNode, TData>.DFSNode current,
			IGraph<TNode, TData> graph, Stack<DFSEnumerator<TNode, TData>.DFSNode> stack, HashSet<TNode> visited,
			List<RibData<TNode>> spanningTreeEdges)
		{
			var currentIndex = graph.GetIndex(current.node)!.Value;
			for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
			{
				if (graph[adjIndex][currentIndex].HasValue && !visited.Contains(graph.Nodes[adjIndex]))
				{
					stack.Push(new DFSEnumerator<TNode, TData>.DFSNode(graph.Nodes[adjIndex], current.depth + 1));
					spanningTreeEdges.Add(new RibData<TNode>(
						current.node, graph.Nodes[adjIndex], graph[currentIndex][adjIndex]!.Value));
				}
			}
		}
	}
	
	/// <summary>
	/// Prim`s algorithm
	/// </summary>
	public static List<RibData<TNode>>? BuildMinimumSpanningTreePrim<TNode, TData>(
		this IGraph<TNode, TData> graph, ref HashSet<TNode>? visited,
		ref PriorityQueue<(TNode from, TNode to), double>? priorityQueue) where TNode : notnull
	{
		if (graph.Size == 0) return null;
		
		(visited ??= new HashSet<TNode>()).Clear();
		(priorityQueue ??= new PriorityQueue<(TNode from, TNode to), double>()).Clear();
		
		var spanningTreeEdges = new List<RibData<TNode>>();
		visited.Add(graph.Nodes[0]);
		for (var i = 0; i < graph.Size; i++)
		{
			if (graph[0][i].HasValue)
			{
				priorityQueue.Enqueue((graph.Nodes[0], graph.Nodes[i]), graph[0][i]!.Value);
			}
		}
		
		while (visited.Count < graph.Size && priorityQueue.TryDequeue(out var currentEdge, out var currentWeight))
		{
			if (!visited.Add(currentEdge.to)) continue;
			
			spanningTreeEdges.Add(new RibData<TNode>(currentEdge.from, currentEdge.to, currentWeight));
			var currentIndex = graph.GetIndex(currentEdge.to)!.Value;
			for (var i = 0; i < graph.Size; i++)
			{
				if (graph[currentIndex][i].HasValue && !visited.Contains(graph.Nodes[i]))
				{
					priorityQueue.Enqueue((currentEdge.to, graph.Nodes[i]), graph[currentIndex][i]!.Value);
				}
			}
		}
		
		return spanningTreeEdges;
	}

	/// <summary>
	/// Kruskal's algorithm
	/// </summary>
	public static List<RibData<TNode>>? BuildMinimumSpanningTreeKruskal<TNode, TData>(
		this IGraph<TNode, TData> graph) where TNode : notnull
	{
		if (graph.Size == 0) return null;

		var dsu = new DisjointSetUnion(graph.Size);
		var spanningTreeEdges = new List<RibData<TNode>>();
		var ribs = graph.GetIncidentRibs();
		ribs.Sort((a, b) => a.weight.CompareTo(b.weight));
		for (var i = 0; i < ribs.Count; i++)
		{
			var fromIndex = graph.GetIndex(ribs[i].fromNode)!.Value;
			var toIndex = graph.GetIndex(ribs[i].toNode)!.Value;
			if (dsu.Union(fromIndex, toIndex))
			{
				spanningTreeEdges.Add(ribs[i]);
				if (spanningTreeEdges.Count == graph.Size - 1) break; // Ribs count in MST is always less than vertexes
			}
		}
		
		return spanningTreeEdges;
	}
	
	/// <summary>
	/// Boruvka's algorithm
	/// </summary>
	public static List<RibData<TNode>>? BuildMinimumSpanningTreeBoruvka<TNode, TData>(
		this IGraph<TNode, TData> graph) where TNode : notnull
	{
		if (graph.Size == 0) return null;

		var dsu = new DisjointSetUnion(graph.Size);
		var spanningTreeEdges = new List<RibData<TNode>>();
		var cheapestEdges = new IndexedRibData?[graph.Size];
		var components = graph.Size;

		while (components > 1)
		{
			Array.Fill(cheapestEdges, null);
			Parallel.For(0, graph.Size, i =>
			{
				var nodeRoot = dsu.Find(i);
				for (var j = 0; j < graph.Size; j++)
				{
					if (graph[i][j].HasValue && i != j)
					{
						var neighborRoot = dsu.Find(j);
						if (nodeRoot == neighborRoot) continue;
						
						var currentEdgeWeight = graph[i][j]!.Value;
						lock (cheapestEdges)
						{
							if (cheapestEdges[nodeRoot] == null ||
							    cheapestEdges[nodeRoot]!.Value.weight > currentEdgeWeight)
							{
								cheapestEdges[nodeRoot] = new IndexedRibData(i, j, currentEdgeWeight);
							}
						}
					}
				}
			});
			
			for (var i = 0; i < graph.Size; i++)
			{
				var cheapestEdge = cheapestEdges[i];
				if (cheapestEdge.HasValue)
				{
					var fromRoot = dsu.Find(cheapestEdge.Value.fromNode);
					var toRoot = dsu.Find(cheapestEdge.Value.toNode);

					if (fromRoot != toRoot && dsu.Union(fromRoot, toRoot))
					{
						spanningTreeEdges.Add(cheapestEdge.Value.Convert(graph));
						components--;
					}
				}
			}
		}
		
		return spanningTreeEdges;
	}
}