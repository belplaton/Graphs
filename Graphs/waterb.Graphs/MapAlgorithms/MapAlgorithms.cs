namespace waterb.Graphs.MapAlgorithms;

public static partial class MapAlgorithms
{
	public static (List<(int x, int y)> path, double length)? FindPathDijkstra(this GraphMap graphMap, (int x, int y) from, (int x, int y) to)
	{
		var dist = new Dictionary<(int x, int y), double>();
		var prev = new Dictionary<(int x, int y), (int x, int y)>();
		
		dist[from] = 0;
		var openSet = new PriorityQueue<(int x, int y), double>();
		openSet.Enqueue(from, 0.0);
		while (openSet.TryDequeue(out var current, out var weight) && !current.Equals(to))
		{
			var neighbours = graphMap.GetNeighbors(current.x, current.y);
			for (var i = 0; i < neighbours.Count; i++)
			{
				var neighbour = neighbours[i];
				if (graphMap.TryGetDistance(current, neighbour, out var distance))
				{
					var alt = dist[current] + distance;
					if (alt < dist.GetValueOrDefault(neighbour, double.PositiveInfinity))
					{
						dist[neighbour] = alt;
						prev[neighbour] = current;
						openSet.Enqueue(neighbour, alt);
					}
				}
			}
		}
		
		if (!prev.ContainsKey(to) && !from.Equals(to))
		{
			return null;
		}
		
		var path = new List<(int, int)>();
		var node = to;
		path.Add(node);
		while (!node.Equals(from))
		{
			node = prev[node];
			path.Add(node);
		}
		
		path.Reverse();
		return (path, dist[to] + path.Count - 1);
	}
	
	public static (List<(int x, int y)> path, double length)? FindPathAStar(this GraphMap graphMap,
		(int x, int y) from, (int x, int y) to, DistanceMetric metric = DistanceMetric.Euclid)
    {
        var gScore = new Dictionary<(int x, int y), double>();
        var fScore = new Dictionary<(int x, int y), double>();
        var prev = new Dictionary<(int x, int y), (int x, int y)>();
        
        gScore[from] = 0.0;
        fScore[from] = GraphMap.GetHeuristicsDistance(from, to, metric);
        
        var openSet = new PriorityQueue<(int x, int y), double>();
        var closedSet = new HashSet<(int x, int y)>();
        openSet.Enqueue(from, fScore[from]);

        while (openSet.TryDequeue(out var current, out var weight) && !current.Equals(to))
        {
	        closedSet.Add(current);
            var neighbours = graphMap.GetNeighbors(current.x, current.y);
            for (var i = 0; i < neighbours.Count; i++)
            {
                var neighbour = neighbours[i];
                if (graphMap.TryGetDistance(current, neighbour, out var distance))
                {
	                var alt = gScore[current] + distance;
	                if (alt < gScore.GetValueOrDefault(neighbour, double.PositiveInfinity) || !closedSet.Contains(neighbour))
	                {
		                prev[neighbour] = current;
		                gScore[neighbour] = alt;
		                fScore[neighbour] = alt + GraphMap.GetHeuristicsDistance(neighbour, to, metric);
		                if (!closedSet.Contains(neighbour)) openSet.Enqueue(neighbour, fScore[neighbour]);
	                }
                }
            }
        }

        if (!prev.ContainsKey(to) && !from.Equals(to))
        {
            return null;
        }
        
        var path = new List<(int, int)>();
        var node = to;
        path.Add(node);
        while (!node.Equals(from))
        {
            node = prev[node];
            path.Add(node);
        }
        
        return (path, gScore[to] + path.Count - 1);
    }
}