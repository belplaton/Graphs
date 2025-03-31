namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
    public static (
        double?[][] dist,
        int[] degreesVector,
        double[] eccentricity,
        List<int> peripheralIndexes,
        List<int> centralIndexes)?
        FloydWarshall<TNode, TData>(this IGraph<TNode, TData> graph)
    {
        if (graph.Size == 0) return null;
        
        var dist = new double?[graph.Size][];
        var degreesVector = new int[graph.Size];
        
        for (var i = 0; i < graph.Size; i++)
        {
            dist[i] = new double?[graph.Size];
            for (var j = 0; j < graph.Size; j++)
            {
                degreesVector[i] += graph[i][j].HasValue ? 1 : 0;
                for (var k = 0; k < graph.Size; k++)
                {
                    var a = (dist[i][j] ?? graph[i][j]).GetWeight();
                    var b = (dist[i][k] ?? graph[i][k]).GetWeight();
                    var c = (dist[k][j] ?? graph[k][j]).GetWeight();
                    dist[i][j] = a > b + c ? b + c : a;
                }
            }
        }
        
        var eccentricity = new double[graph.Size];
        for (var i = 0; i < graph.Size; i++)
        {
            double maxDistance = 0;
            for (var j = 0; j < graph.Size; j++)
            {
                maxDistance = dist[i][j] > maxDistance && dist[i][j] < double.PositiveInfinity
                    ? dist[i][j]!.Value : maxDistance;
            }
            
            eccentricity[i] = maxDistance;
        }

        double diametr = 0;
        var radius = double.PositiveInfinity;
        for (var i = 0; i < graph.Size; i++)
        {
            if (eccentricity[i] > diametr) diametr = eccentricity[i];
            if (eccentricity[i] < radius) radius = eccentricity[i];
        }

        var peripheralIndexes = new List<int>();
        var centralIndexes = new List<int>();
        for (var i = 0; i < graph.Size; i++)
        {
            if (Math.Abs(eccentricity[i] - diametr) <= double.Epsilon) peripheralIndexes.Add(i);
            if (Math.Abs(eccentricity[i] - radius) <= double.Epsilon) centralIndexes.Add(i);
        }

        return (dist, degreesVector, eccentricity, peripheralIndexes, centralIndexes);
    }
}