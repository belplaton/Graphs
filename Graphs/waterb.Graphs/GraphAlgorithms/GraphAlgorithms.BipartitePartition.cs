namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
    public static bool TryGetBipartitePartition<TNode, TData>(this IGraph<TNode, TData> graph,
        out List<TNode>? partA, out List<TNode>? partB) where TNode : notnull
    {
        HashSet<TNode>? visited = [];
        Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack = [];
        return graph.TryGetBipartitePartition(ref visited, ref stack, out partA, out partB);
    }

    public static bool TryGetBipartitePartition<TNode, TData>(this IGraph<TNode, TData> graph,
        ref HashSet<TNode>? visited, ref Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack,
        out List<TNode>? partA, out List<TNode>? partB) where TNode : notnull
    {
        if (graph.Size == 0)
        {
            partA = new List<TNode>();
            partB = new List<TNode>();
            return true;
        }
        
        (visited ??= new HashSet<TNode>()).Clear();
        (stack ??= new Stack<DFSEnumerator<TNode, TData>.DFSNode>()).Clear();
        var color = new bool?[graph.Size];
        var isValid = true;
        
        for (var i = 0; i < graph.Size; i++)
        {
            var node = graph.Nodes[i];
            if (!color[i].HasValue)
            {
                using var enumerator = new DFSEnumerator<TNode, TData>(graph, node, visited, stack, 
                    (current, g, s, v) => OnPrepareStackChanges(current, g, s, v, color, ref isValid));
                while (enumerator.MoveNext())
                {
                    if (!isValid)
                    {
                        partA = null;
                        partB = null;
                        return false;
                    }
                    
                    color[graph.GetIndex(enumerator.Current.Node)!.Value] = enumerator.Current.Depth % 2 == 0;
                }
            }
        }

        partA = new List<TNode>();
        partB = new List<TNode>();
        for (var i = 0; i < graph.Size; i++)
        {
            if (color[i]!.Value)
                partA.Add(graph.Nodes[i]);
            else
                partB.Add(graph.Nodes[i]);
        }

        return true;
        static void OnPrepareStackChanges(DFSEnumerator<TNode, TData>.DFSNode current, IGraph<TNode, TData> graph,
            Stack<DFSEnumerator<TNode, TData>.DFSNode> stack, HashSet<TNode> visited, bool?[] color, ref bool isValid)
        {
            var currentIndex = graph.GetIndex(current.Node)!.Value;
            for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
            {
                if (graph[adjIndex][currentIndex].HasValue)
                {
                    if (!visited.Contains(graph.Nodes[adjIndex]))
                    {
                        stack.Push(new DFSEnumerator<TNode, TData>.DFSNode(graph.Nodes[adjIndex], current.Depth + 1));
                    }
                    else if (color[adjIndex].HasValue && color[adjIndex] != ((current.Depth + 1) % 2 == 0))
                    {
                        isValid = false;
                    }
                }
            }
        }
    }

    public static List<RibData<TNode>>? FindMaxBipartiteMatching<TNode, TData>(this IGraph<TNode, TData> graph)
        where TNode : notnull
    {
        if (!graph.TryGetBipartitePartition(out var partA, out var partB)) return null;

        var lessPart = partA!.Count < partB!.Count ? partA : partB;
        var result = new List<RibData<TNode>>();

        var matching = new Dictionary<TNode, TNode>();
        var visited = new HashSet<TNode>();
        for (var i = 0; i < lessPart.Count; i++)
        {
            visited.Clear();
            TryFindAugmentingPath(graph, lessPart[i], visited, matching);
        }

        for (var i = 0; i < graph.Size; i++)
        {
            var node = graph.Nodes[i];
            if (matching.TryGetValue(node, out var matchedNode))
            {
                result.Add(new RibData<TNode>(node, matchedNode, graph.GetWeight(node, matchedNode)!.Value));
            }
        }
        
        return result.Count > 0 ? result : null;
    }
    
    private static bool TryFindAugmentingPath<TNode, TData>(IGraph<TNode, TData> graph, 
        TNode startNode, HashSet<TNode> visited, Dictionary<TNode, TNode> matching) where TNode : notnull
    {
        if (!visited.Add(startNode)) return false;
        for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
        {
            var adjNode = graph.Nodes[adjIndex];
            if (graph[startNode][adjIndex].HasValue)
            {
                if (!matching.ContainsKey(adjNode) ||
                    TryFindAugmentingPath(graph, matching[adjNode], visited, matching))
                {
                    matching[adjNode] = startNode;
                    return true;
                }
            }
        }
        
        return false;
    }
}