namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
    /// <summary>
    /// FordFulkerson
    /// </summary>
	public static (double maxFlow, List<RibData<TNode>> flow)? FindMaxFlowFordFulkerson<TNode, TData>(
        this IGraph<TNode, TData> graph, TNode source, TNode sink) where TNode : notnull
    {
        if (EqualityComparer<TNode>.Default.Equals(source, sink)) return (double.PositiveInfinity, []);
        
        var srcIndex = graph.GetIndex(source) ?? throw new ArgumentException($"Source {source} is not exist in graph!");
        var sinkIndex = graph.GetIndex(sink) ?? throw new ArgumentException($"Sink {sink} is not exist in graph!");
        
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
        
        return (maxFlow, flowEdges);
    }
}