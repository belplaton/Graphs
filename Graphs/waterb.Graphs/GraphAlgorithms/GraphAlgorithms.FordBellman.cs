namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	 public static Dictionary<TNode, double>? FindShortestPathsFordBellman<TNode, TData>(
        this IGraph<TNode, TData> graph, TNode startNode) where TNode : notnull
    {
        if (graph.Size == 0) return new Dictionary<TNode, double>();

        var startIndex = graph.GetIndex(startNode) ?? 
            throw new ArgumentException($"Node {startNode} is not exist in graph!");
        
        var dist = new double?[graph.Size];
        dist[startIndex] = 0;
        
        for (var i = 0; i < graph.Size - 1; i++)
        {
            var isRelaxed = false;
            for (var fromIndex = 0; fromIndex < graph.Size; fromIndex++)
            {
                if (!dist[fromIndex].HasValue) continue;
                for (var toIndex = 0; toIndex < graph.Size; toIndex++)
                {
                    if (graph[fromIndex][toIndex].HasValue)
                    {
                        var candidate = dist[fromIndex]!.Value + graph[fromIndex][toIndex]!.Value;
                        if (!dist[toIndex].HasValue || dist[toIndex]! > candidate)
                        {
                            dist[toIndex] = candidate;
                            isRelaxed = true;
                        }
                    }
                }
            }
            
            if (!isRelaxed) break;
        }
        
        // Check for negative cycle
        for (var fromIndex = 0; fromIndex < graph.Size; fromIndex++)
        {
            if (!dist[fromIndex].HasValue) continue;
            for (var toIndex = 0; toIndex < graph.Size; toIndex++)
            {
                if (graph[fromIndex][toIndex].HasValue)
                {
                    var candidate = dist[fromIndex]!.Value + graph[fromIndex][toIndex]!.Value;
                    if (!dist[toIndex].HasValue || dist[toIndex]! > candidate)
                    {   
                        return null;
                    }
                }
            }
        }
        
        var result = new Dictionary<TNode, double>(graph.Size);
        for (var i = 0; i < graph.Size; i++)
        {
            result[graph.Nodes[i]] = dist[i] ?? double.PositiveInfinity;
        }

        return result;
    }
}