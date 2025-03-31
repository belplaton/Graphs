
using System.Text;

namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public readonly struct ConnectedComponent<TNode>
	{
		public List<TNode> Nodes { get; }

		public ConnectedComponent()
		{
			Nodes = new List<TNode>();	
		}
		public ConnectedComponent(List<TNode>? nodes = null)
		{
			Nodes = nodes ?? new List<TNode>();
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			for (var i = 0; i < Nodes.Count; i++)
			{
				sb.Append($"{Nodes[i]}");
				if (i + 1 < Nodes.Count) sb.Append(" - ");
			}

			return sb.ToString();
		}
	}
	
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
				while (enumerator.MoveNext()) component.Nodes.Add(enumerator.Current.node);
				components.Add(component);
			}
		}

		return components;
	}
	
	public static List<ConnectedComponent<TNode>>? FindWeakConnectedComponents<TNode, TData>(this IGraph<TNode, TData> graph,
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
				using var enumerator = new DFSEnumerator<TNode, TData>(graph, node, visited, stack, OnPrepareStackChangesWeakComponents);
				while (enumerator.MoveNext()) component.Nodes.Add(enumerator.Current.node);
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
	/// Algorithm Kosayraju
	/// </summary>
	public static List<ConnectedComponent<TNode>>? FindStrongConnectedComponents<TNode, TData>(this IGraph<TNode, TData> graph,
		ref HashSet<TNode>? visited, ref Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack)
	{
		if (graph.Size == 0) return null;
		
		(visited ??= new HashSet<TNode>()).Clear();
		(stack ??= new Stack<DFSEnumerator<TNode, TData>.DFSNode>()).Clear();

		var lastExitTime = 0;
		var nodesExitTime = new (TNode node, int exitTime)?[graph.Size];
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

		Array.Sort(nodesExitTime, (a, b) => (b?.exitTime ?? int.MinValue).CompareTo(a?.exitTime ?? int.MinValue));
		visited.Clear();
		stack.Clear();
		
		var components = new List<ConnectedComponent<TNode>>();
		for (var i = 0; i < nodesExitTime.Length; i++)
		{
			if (nodesExitTime[i].HasValue)
			{
				var node = nodesExitTime[i]!.Value.node;
				if (!visited.Contains(node))
				{
					var component = new ConnectedComponent<TNode>();
					using var componentEnumerator = new DFSEnumerator<TNode, TData>(graph, node,
						visited, stack, OnPrepareStackChangesStrongComponents);
					while (componentEnumerator.MoveNext()) component.Nodes.Add(componentEnumerator.Current.node);
					components.Add(component);
				}
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
			for (var i = 0; i < Joints.Count; i++)
			{
				sb.Append($"{Joints[i]}");
				if (i + 1 < Joints.Count) sb.Append(", ");
			}

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
			for (var i = 0; i < Bridges.Count; i++)
			{
				sb.Append($"[{Bridges[i].fromNode} - {Bridges[i].toNode}]");
				if (i + 1 < Bridges.Count) sb.Append(", ");
			}

			return sb.ToString();
		}
	}
	
	public static (GraphJoints<TNode> joints, GraphBridges<TNode> bridges)? FindJointsAndBridges<TNode, TData>(
		this IGraph<TNode, TData> graph, ref HashSet<TNode>? visited,
		ref Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack) where TNode : notnull
	{
		if (graph.Size == 0) return null;
		
		(visited ??= new HashSet<TNode>()).Clear();
		(stack ??= new Stack<DFSEnumerator<TNode, TData>.DFSNode>()).Clear();

		var lastEnterTime = 0;
		var nodesInfo = new (int lowTime, int discoverTime, int? parentIndex)[graph.Size];
		var reversalStack = new Stack<(int currentIndex, int neigbourIndex)>();
		var checkedJoints = new HashSet<TNode>();
		
		var joints = new GraphJoints<TNode>();
		var bridges = new GraphBridges<TNode>();
		
		for (var i = 0; i < graph.Size; i++)
		{
			if (!visited.Contains(graph.Nodes[i]))
			{
				using var enumerator = new DFSEnumerator<TNode, TData>(graph, graph.Nodes[i], visited, stack, 
					(current, g, s, v) => OnPrepareStackChanges(
						current, g, s, v, ref lastEnterTime, nodesInfo, reversalStack, joints));
				while (enumerator.MoveNext()) { }
				while (reversalStack.Count > 0)
				{
					var pair = reversalStack.Pop();
					nodesInfo[pair.currentIndex] = (
						lowTime: Math.Min(nodesInfo[pair.currentIndex].lowTime, nodesInfo[pair.neigbourIndex].lowTime),
						nodesInfo[pair.currentIndex].discoverTime,
						nodesInfo[pair.currentIndex].parentIndex);

					if (nodesInfo[pair.neigbourIndex].lowTime > nodesInfo[pair.currentIndex].discoverTime)
					{
						bridges.Bridges.Add((graph.Nodes[pair.currentIndex], graph.Nodes[pair.neigbourIndex]));
					}
					
					if (!checkedJoints.Contains(graph.Nodes[pair.currentIndex]) &&
					    nodesInfo[pair.currentIndex].parentIndex != null &&
					    nodesInfo[pair.neigbourIndex].lowTime >= nodesInfo[pair.currentIndex].discoverTime)
					{
						joints.Joints.Add(graph.Nodes[pair.currentIndex]);
						checkedJoints.Add(graph.Nodes[pair.currentIndex]);
					}
				}
			}
		}

		return (joints, bridges);
		static void OnPrepareStackChanges(
            DFSEnumerator<TNode, TData>.DFSNode current, IGraph<TNode, TData> graph, 
            Stack<DFSEnumerator<TNode, TData>.DFSNode> stack, HashSet<TNode> visited, 
            ref int lastEnterTime,
            (int lowTime, int discoverTime, int? parentIndex)[] nodesInfo,
            Stack<(int currentIndex, int neighbourIndex)> reversalStack,
            GraphJoints<TNode> joints)
        {
            var currentNodeIndex = graph.GetIndex(current.node)!.Value;
            if (!visited.Contains(current.node))
            {
	            nodesInfo[currentNodeIndex] = (lowTime: lastEnterTime, discoverTime: lastEnterTime,
		            nodesInfo[currentNodeIndex].parentIndex);
	            lastEnterTime++;
            }

            var childrensCount = 0;
            for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
            {
	            if (graph[current.node][adjIndex].HasValue)
	            {
		            var neighbor = graph.Nodes[adjIndex];
		            if (adjIndex == nodesInfo[currentNodeIndex].parentIndex) continue;
		            if (visited.Contains(neighbor))
		            {
			            nodesInfo[currentNodeIndex] = (
				            lowTime: Math.Min(nodesInfo[currentNodeIndex].lowTime, nodesInfo[adjIndex].discoverTime),
				            nodesInfo[currentNodeIndex].discoverTime,
				            nodesInfo[currentNodeIndex].parentIndex);
		            }
		            else
		            {
			            nodesInfo[adjIndex] = (nodesInfo[adjIndex].lowTime, nodesInfo[adjIndex].discoverTime, currentNodeIndex);
			            stack.Push(new DFSEnumerator<TNode, TData>.DFSNode(neighbor, current.depth + 1));
			            reversalStack.Push((currentNodeIndex, adjIndex));
			            childrensCount++;
		            }
	            }
            }
            
            if (nodesInfo[currentNodeIndex].parentIndex == null && childrensCount > 1)
            {
	            joints.Joints.Add(current.node);
            }
        }
	}
}