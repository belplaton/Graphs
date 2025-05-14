namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
    /// <summary>
    /// FordFulkerson
    /// </summary>
	public static (TNode source, TNode sink, double maxFlow, List<RibData<TNode>> flow)? FindMaxFlowFordFulkerson<TNode, TData>(
        this IGraph<TNode, TData> graph) where TNode : notnull
    {
        int srcIndex = -1, sinkIndex = -1;
        for (var i = 0; i < graph.Size; i++)
        {
            bool hasIncoming = false, hasOutgoing = false;
            for (var j = 0; j < graph.Size; j++)
            {
                if (graph[j][i].HasValue && graph[j][i]!.Value != 0) hasIncoming = true;
                if (graph[i][j].HasValue && graph[i][j]!.Value != 0) hasOutgoing = true;
                if (hasIncoming && hasOutgoing) break;
            }

            switch (hasIncoming)
            {
                case false when hasOutgoing:
                {
                    if (srcIndex is not -1) throw new ArgumentException("Found more than one candidate for source.");
                    srcIndex = i;
                    break;
                }
                case true when !hasOutgoing:
                {
                    if (sinkIndex is not -1) throw new ArgumentException("Found more than one candidate for sink.");
                    sinkIndex = i;
                    break;
                }
            }
        }

        if (srcIndex is -1 || sinkIndex is -1)
            throw new ArgumentException("Can`t identify source or sink..");
        
        var residual = new double[graph.Size][];
        for (var i = 0; i < graph.Size; i++)
        {
            residual[i] = new double[graph.Size];
            for (var j = 0; j < graph.Size; j++)
                residual[i][j] = graph[i][j] ?? 0;
        }

        var maxFlow = 0.0;
        var stack = new Stack<int>();
        var parent = new int?[graph.Size];
        while (true)
        {
            Array.Fill(parent, null);
            parent[srcIndex] = srcIndex;
            stack.Clear();
            stack.Push(srcIndex);
            
            while (stack.Count > 0 && parent[sinkIndex] is null)
            {
                var currentIndex = stack.Pop();
                for (var vertexIndex = 0; vertexIndex < graph.Size; vertexIndex++)
                {
                    if (parent[vertexIndex] is null && residual[currentIndex][vertexIndex] > 0)
                    {
                        parent[vertexIndex] = currentIndex;
                        stack.Push(vertexIndex);
                    }
                }
            }
            
            if (parent[sinkIndex] is null) break;
            
            var delta = double.PositiveInfinity;
            for (var currentIndex = sinkIndex; currentIndex != srcIndex; currentIndex = parent[currentIndex]!.Value)
            {
                var parentIndex = parent[currentIndex]!.Value;
                delta = Math.Min(delta, residual[parentIndex][currentIndex]);
            }
            
            for (var currentIndex = sinkIndex; currentIndex != srcIndex; currentIndex = parent[currentIndex]!.Value)
            {
                var parentIndex = parent[currentIndex]!.Value;
                residual[parentIndex][currentIndex] -= delta; // through
                residual[currentIndex][parentIndex] += delta; // inverse
            }

            maxFlow += delta;
        }
        
        var flowEdges = new List<RibData<TNode>>();
        for (var fromIndex = 0; fromIndex < graph.Size; fromIndex++)
        {
            for (var toIndex = 0; toIndex < graph.Size; toIndex++)
            {
                if (!graph[fromIndex][toIndex].HasValue) continue;
                var used = graph[fromIndex][toIndex]!.Value - residual[fromIndex][toIndex];
                if (used > 0) flowEdges.Add(new RibData<TNode>(graph.Nodes[fromIndex], graph.Nodes[toIndex], used));
            }
        }
        
        return (graph.Nodes[srcIndex], graph.Nodes[sinkIndex], maxFlow, flowEdges);
    }
}