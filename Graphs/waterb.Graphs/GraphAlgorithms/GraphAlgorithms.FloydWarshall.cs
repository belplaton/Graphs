namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
    public static FloydWarshallData? FloydWarshall<TNode, TData>(this IGraph<TNode, TData> graph)
    {
        if (graph.Size == 0) return null;

        var data = new FloydWarshallData(graph.Size);
        for (var k = 0; k < graph.Size; k++)
        {
            for (var i = 0; i < graph.Size; i++)
            {
                data.degreesVector[k] += graph[k][i].HasValue ? 1 : 0;
                for (var j = 0; j < graph.Size; j++)
                {
                    var a = i != j ? (data.dist[i][j] ?? graph[i][j]).GetWeight() : 0;
                    var b = i != k ? (data.dist[i][k] ?? graph[i][k]).GetWeight() : 0;
                    var c = k != j ? (data.dist[k][j] ?? graph[k][j]).GetWeight() : 0;
                    data.dist[i][j] = a > b + c ? b + c : a;
                }
            }
        }
        
        for (var i = 0; i < graph.Size; i++)
        {
            double maxDistance = 0;
            for (var j = 0; j < graph.Size; j++)
            {
                maxDistance = data.dist[i][j] > maxDistance && data.dist[i][j] < double.PositiveInfinity
                    ? data.dist[i][j]!.Value : maxDistance;
            }
            
            data.eccentricity[i] = maxDistance;
        }

        data.diameter = 0;
        data.radius = double.PositiveInfinity;
        for (var i = 0; i < graph.Size; i++)
        {
            if (data.eccentricity[i] > data.diameter) data.diameter = data.eccentricity[i];
            if (data.eccentricity[i] < data.radius) data.radius = data.eccentricity[i];
        }
        
        for (var i = 0; i < graph.Size; i++)
        {
            if (Math.Abs(data.eccentricity[i] - data.diameter) <= double.Epsilon) data.peripheralIndexes.Add(i);
            if (Math.Abs(data.eccentricity[i] - data.radius) <= double.Epsilon) data.centralIndexes.Add(i);
        }

        return data;
    }
}