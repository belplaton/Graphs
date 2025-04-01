namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
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
        var color = new bool[graph.Size];
        var isValid = true;
        
        for (var i = 0; i < graph.Size; i++)
        {
            var node = graph.Nodes[i];
            if (!color[i])
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
                    
                    color[graph.GetIndex(enumerator.Current.node)!.Value] = enumerator.Current.depth % 2 == 0;
                }
            }
        }

        partA = new List<TNode>();
        partB = new List<TNode>();
        for (var i = 0; i < graph.Size; i++)
        {
            if (!color[i])
                partA.Add(graph.Nodes[i]);
            else
                partB.Add(graph.Nodes[i]);
        }

        return true;
        static void OnPrepareStackChanges(DFSEnumerator<TNode, TData>.DFSNode current, IGraph<TNode, TData> graph,
            Stack<DFSEnumerator<TNode, TData>.DFSNode> stack, HashSet<TNode> visited, bool[] color, ref bool isValid)
        {
            var currentIndex = graph.GetIndex(current.node)!.Value;
            for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
            {
                if (graph[adjIndex][currentIndex].HasValue && !visited.Contains(graph.Nodes[adjIndex]))
                {
                    stack.Push(new DFSEnumerator<TNode, TData>.DFSNode(graph.Nodes[adjIndex], current.depth + 1));
                }
                else if (color[adjIndex] != ((current.depth + 1) % 2 == 0))
                {
                    isValid = false;
                }
            }
        }
    }
}