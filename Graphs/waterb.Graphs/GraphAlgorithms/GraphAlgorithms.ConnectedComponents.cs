
using System.Text;
using waterb.Utility;

namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public readonly struct ConnectedComponent<TNode> : IEquatable<ConnectedComponent<TNode>>
	{
		public IndexedSet<TNode> Nodes { get; }
		public int Count => Nodes.Count;
		public TNode this[int index] => Nodes[index];

		public ConnectedComponent()
		{
			Nodes = [];	
		}
		public ConnectedComponent(IndexedSet<TNode>? nodes = null)
		{
			Nodes = nodes ?? [];
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine("Vertices list in component:");
			sb.Append('[');
			for (var i = 0; i < Nodes.Count; i++)
			{
				sb.Append($"{Nodes[i]}");
				if (i + 1 < Nodes.Count) sb.Append(", ");
			}
			sb.Append("]");

			return sb.ToString();
		}

		public bool Equals(ConnectedComponent<TNode> other)
		{
			return Nodes.Equals(other.Nodes);
		}

		public override bool Equals(object? obj)
		{
			return obj is ConnectedComponent<TNode> other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Nodes.GetHashCode();
		}

        public static bool operator ==(ConnectedComponent<TNode> left, ConnectedComponent<TNode> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConnectedComponent<TNode> left, ConnectedComponent<TNode> right)
        {
            return !(left == right);
        }
    }
	
	// non-oriented graph only
	public static List<ConnectedComponent<TNode>>? FindConnectedComponents<TNode, TData>(this IGraph<TNode, TData> graph)
	{
		HashSet<TNode>? visited = null;
		Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack = null;
		return FindConnectedComponents(graph, ref visited, ref stack);
	}

	// non-oriented graph only
	public static List<ConnectedComponent<TNode>>? FindConnectedComponents<TNode, TData>(this IGraph<TNode, TData> graph,
		ref HashSet<TNode>? visited, ref Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack)
	{
		if (graph.Size == 0) return null;
		
		(visited ??= new HashSet<TNode>()).Clear();
		(stack ??= new Stack<DFSEnumerator<TNode, TData>.DFSNode>()).Clear();
		var components = new List<ConnectedComponent<TNode>>();

		for (var i = 0; i < graph.Size; i++)
		{
			var node = graph.Nodes[i];
			if (!visited.Contains(node))
			{
				var component = new ConnectedComponent<TNode>();
				using var enumerator = new DFSEnumerator<TNode, TData>(graph, node, visited, stack);
				while (enumerator.MoveNext()) component.Nodes.Add(enumerator.Current.Node);
				components.Add(component);
			}
		}

		return components;
	}
	
	// oriented graph only
	public static List<ConnectedComponent<TNode>>? FindWeakConnectedComponents<TNode, TData>(
		this IGraph<TNode, TData> graph) where TNode : notnull
	{
		if (graph.Size == 0) return null;
		
		var visited = new bool[graph.Size];
		var components = new List<ConnectedComponent<TNode>>();

		for (var uIndex = 0; uIndex < graph.Size; uIndex++)
		{
			if (!visited[uIndex])
			{
				var component = new ConnectedComponent<TNode>();
				Dfs(uIndex, graph, visited, ref component);
				components.Add(component);
			} 
		}

		return components;
		static void Dfs(int uIndex, IGraph<TNode, TData> graph,
			bool[] visited, ref ConnectedComponent<TNode> component)
		{
			visited[uIndex] = true;
			component.Nodes.Add(graph.Nodes[uIndex]);
			for (var vIndex = 0; vIndex < graph.Size; vIndex++)
			{
				if (!graph[uIndex][vIndex].HasValue && !graph[vIndex][uIndex].HasValue || visited[vIndex]) continue;
				
				Dfs(vIndex, graph, visited, ref component);
			}
		}
	}

	/// <summary>
	/// Algorithm Kosayraju
	/// </summary>
	public static List<ConnectedComponent<TNode>>? FindStrongConnectedComponents<TNode, TData>(
		this IGraph<TNode, TData> graph) where TNode : notnull
	{
		if (graph.Size == 0) return null;
		
		var visited = new bool[graph.Size];
		var order = new List<int>(graph.Size);
		
		var components = new List<ConnectedComponent<TNode>>();
		for (var uIndex = 0; uIndex < graph.Size; uIndex++)
		{
			if (!visited[uIndex]) DfsOrder(uIndex, graph, visited, order);
		}
		
		Array.Fill(visited, false);
		for (var i = order.Count - 1; i >= 0; i--)
		{
			var uIndex = order[i];
			if (!visited[uIndex])
			{
				var component = new ConnectedComponent<TNode>();
				DfsCollect(uIndex, graph, visited, ref component);
				components.Add(component);
			}
		}

		return components;
		static void DfsOrder(int uIndex, IGraph<TNode, TData> graph,
			bool[] visited, List<int> order)
		{
			visited[uIndex] = true;
			for (var vIndex = 0; vIndex < graph.Size; vIndex++)
			{
				if (!graph[uIndex][vIndex].HasValue || visited[vIndex]) continue;
		        
				DfsOrder(vIndex, graph, visited, order);
			}
	        
			order.Add(uIndex);
		}
		
		static void DfsCollect(int uIndex, IGraph<TNode, TData> graph,
			bool[] visited, ref ConnectedComponent<TNode> component)
		{
			visited[uIndex] = true;
			component.Nodes.Add(graph.Nodes[uIndex]);
			for (var vIndex = 0; vIndex < graph.Size; vIndex++)
			{
				if (!graph[vIndex][uIndex].HasValue || visited[vIndex]) continue;
		        
				DfsCollect(vIndex, graph, visited, ref component);
			}
		}
	}

	public readonly struct GraphJoints<TNode>
	{
		public List<TNode> Joints { get; }

		public GraphJoints()
		{
			Joints = new List<TNode>();	
		}
		public GraphJoints(List<TNode>? joints = null)
		{
			Joints = joints ?? new List<TNode>();
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("[");
			for (var i = 0; i < Joints.Count; i++)
			{
				sb.Append($"{Joints[i]}");
				if (i + 1 < Joints.Count) sb.Append(", ");
			}
			sb.Append("]");

			return sb.ToString();
		}
	}
	
	public readonly struct GraphBridges<TNode>
	{
		public List<(TNode fromNode, TNode toNode)> Bridges { get; }

		public GraphBridges()
		{
			Bridges = new List<(TNode fromNode, TNode toNode)>();	
		}
		public GraphBridges(List<(TNode fromNode, TNode toNode)>? bridges = null)
		{
			Bridges = bridges ?? new List<(TNode fromNode, TNode toNode)>();
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("[");
			for (var i = 0; i < Bridges.Count; i++)
			{
				sb.Append($"[{Bridges[i].fromNode} - {Bridges[i].toNode}]");
				if (i + 1 < Bridges.Count) sb.Append(", ");
			}
			sb.Append("]");

			return sb.ToString();
		}
	}
	
    public static (GraphJoints<TNode> joints, GraphBridges<TNode> bridges)? FindJointsAndBridges<TNode, TData>(
        this IGraph<TNode, TData> graph) where TNode : notnull
    {
        if (graph.Size == 0) return null;
        
        var currentTime = 0;
        var timeIn = new int[graph.Size];
        var timeLow = new int[graph.Size];
        var parent = new int?[graph.Size];
        var visited = new bool[graph.Size];

        var joints = new GraphJoints<TNode>();
        var bridges = new GraphBridges<TNode>();
        
        for (var uIndex = 0; uIndex < graph.Size; uIndex++)
        {
            if (!visited[uIndex])
            {
                parent[uIndex] = null;
                Dfs(uIndex, graph, visited, parent, timeIn, timeLow, joints, bridges, ref currentTime);
            }
        }

        return (joints, bridges);
        static void Dfs(int uIndex, IGraph<TNode, TData> graph,
	        bool[] visited, int?[] parent, int[] timeIn, int[] timeLow,
	        GraphJoints<TNode> joints, GraphBridges<TNode> bridges, ref int currentTime)
        {
	        visited[uIndex] = true;
	        timeIn[uIndex] = timeLow[uIndex] = currentTime++;
	        var children = 0;
	        for (var vIndex = 0; vIndex < graph.Size; vIndex++)
	        {
		        if (!graph[uIndex][vIndex].HasValue) continue;
		        
		        if (parent[uIndex] != null && vIndex == parent[uIndex]!.Value) continue;
		        if (!visited[vIndex])
		        {
			        parent[vIndex] = uIndex;
			        children++;
			        Dfs(vIndex, graph, visited, parent, timeIn, timeLow, joints, bridges, ref currentTime);
			        timeLow[uIndex] = Math.Min(timeLow[uIndex], timeLow[vIndex]);
			        
			        if (parent[uIndex].HasValue && timeLow[vIndex] >= timeIn[uIndex])
				        joints.Joints.Add(graph.Nodes[uIndex]);
			        if (timeLow[vIndex] > timeIn[uIndex])
				        bridges.Bridges.Add((graph.Nodes[uIndex], graph.Nodes[vIndex]));
		        }
		        else
		        {
			        timeLow[uIndex] = Math.Min(timeLow[uIndex], timeIn[vIndex]);
		        }
	        }
	        
	        if (!parent[uIndex].HasValue && children > 1) joints.Joints.Add(graph.Nodes[uIndex]);
        }
    }
}