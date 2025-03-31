namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
    public static (FloydWarshallData<TNode> data, double?[][] dist, int?[][] next)? 
        PrepareFloydWarshallData<TNode, TData>(this IGraph<TNode, TData> graph)
        where TNode : notnull
    {
        if (graph.Size == 0) return null;

        var data = new FloydWarshallData<TNode>();
        var dist = new double?[graph.Size][];
        var next = new int?[graph.Size][];
        for (var i = 0; i < graph.Size; i++)
        {
            dist[i] = new double?[graph.Size];
            next[i] = new int?[graph.Size];
        }
        
        for (var k = 0; k < graph.Size; k++)
        {
            for (var i = 0; i < graph.Size; i++)
            {
                data.nodesDegrees[graph.Nodes[k]] = data.nodesDegrees.GetValueOrDefault(graph.Nodes[k], 0) + 
                    (graph[k][i].HasValue ? 1 : 0);
                for (var j = 0; j < graph.Size; j++)
                {
                    var a = i != j ? (dist[i][j] ?? graph[i][j]).GetWeight() : 0;
                    var b = i != k ? (dist[i][k] ?? graph[i][k]).GetWeight() : 0;
                    var c = k != j ? (dist[k][j] ?? graph[k][j]).GetWeight() : 0;
                    dist[i][j] = a > b + c ? b + c : a;
                    next[i][j] = a > b + c ? next[i][k] ?? k : next[i][j] ?? j;
                }
            }
        }
        
        for (var i = 0; i < graph.Size; i++)
        {
            double maxDistance = 0;
            for (var j = 0; j < graph.Size; j++)
            {
                maxDistance = dist[i][j] > maxDistance && dist[i][j] < double.PositiveInfinity
                    ? dist[i][j]!.Value : maxDistance;
            }
            
            data.nodesEccentricity[graph.Nodes[i]] = maxDistance;
        }

        data.diameter = 0;
        data.radius = double.PositiveInfinity;
        for (var i = 0; i < graph.Size; i++)
        {
            if (data.nodesEccentricity[graph.Nodes[i]] > data.diameter)
                data.diameter = data.nodesEccentricity[graph.Nodes[i]];
            if (data.nodesEccentricity[graph.Nodes[i]] < data.radius)
                data.radius = data.nodesEccentricity[graph.Nodes[i]];
        }
        
        for (var i = 0; i < graph.Size; i++)
        {
            if (Math.Abs(data.nodesEccentricity[graph.Nodes[i]] - data.diameter) <= double.Epsilon)
                data.peripheralNodes.Add(graph.Nodes[i]);
            if (Math.Abs(data.nodesEccentricity[graph.Nodes[i]] - data.radius) <= double.Epsilon)
                data.centralNodes.Add(graph.Nodes[i]);
        }

        return (data, dist, next);
    }
    
    public static bool TryReconstructPath(int startIndex, int endIndex, int?[][] next, ref List<int> result)
    {
        result.Clear();
        while (startIndex != endIndex)
        {
            if (next[startIndex][endIndex].HasValue)
            {
                startIndex = next[startIndex][endIndex]!.Value;
                result.Add(startIndex);
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public static Dictionary<ConnectedComponent<TNode>, FloydWarshallData<TNode>>? ComputeComponentsData
        <TNode, TData>(this IGraph<TNode, TData> graph) 
        where TNode : notnull
    {
        var fwDataPair = graph.PrepareFloydWarshallData();
        if (fwDataPair == null) return null;
        
        HashSet<TNode>? visited = null;
        Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack = null;
        var components = graph.FindConnectedComponents(ref visited, ref stack);
        if (components == null) return null;

        var componentsData = new Dictionary<ConnectedComponent<TNode>, FloydWarshallData<TNode>>();
        for (var i = 0; i < components.Count; i++)
        {
            var component = components[i];
            var currentData = new FloydWarshallData<TNode>();
            
            for (var j = 0; j < component.Nodes.Count; j++)
            {
                var nodeIndex = graph.GetIndex(component.Nodes[j])!.Value;
                double maxDistance = 0;
                for (var k = 0; k < component.Nodes.Count; k++)
                {
                    var otherIndex = graph.GetIndex(component.Nodes[k])!.Value;
                    currentData.nodesDegrees[graph.Nodes[nodeIndex]] = 
                        currentData.nodesDegrees.GetValueOrDefault(graph.Nodes[nodeIndex], 0) +
                        (graph[nodeIndex][otherIndex].HasValue && nodeIndex != otherIndex ? 1 : 0);
                    maxDistance = fwDataPair.Value.dist[nodeIndex][otherIndex] > maxDistance && 
                        fwDataPair.Value.dist[nodeIndex][otherIndex] < double.PositiveInfinity
                        ? fwDataPair.Value.dist[nodeIndex][otherIndex]!.Value : maxDistance;
                }

                currentData.nodesEccentricity[component.Nodes[j]] = maxDistance;
            }
            
            currentData.diameter = 0;
            currentData.radius = double.PositiveInfinity;
            for (var j = 0; j < component.Nodes.Count; j++)
            {
                if (currentData.nodesEccentricity[component.Nodes[j]] > currentData.diameter)
                    currentData.diameter = currentData.nodesEccentricity[component.Nodes[j]];
                if (currentData.nodesEccentricity[component.Nodes[j]] < currentData.radius)
                    currentData.radius = currentData.nodesEccentricity[component.Nodes[j]];
            }
        
            for (var j = 0; j < component.Nodes.Count; j++)
            {
                var nodeIndex = graph.GetIndex(component.Nodes[j])!.Value;
                if (Math.Abs(currentData.nodesEccentricity[component.Nodes[j]] - currentData.diameter) <= double.Epsilon)
                    currentData.peripheralNodes.Add(component.Nodes[j]);
                if (Math.Abs(currentData.nodesEccentricity[component.Nodes[j]] - currentData.radius) <= double.Epsilon)
                    currentData.centralNodes.Add(component.Nodes[j]);
            }

            componentsData[component] = currentData;
        }

        return componentsData;
    }
}