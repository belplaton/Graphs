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
		while (openSet.TryDequeue(out var current, out _) && !current.Equals(to))
		{
			var neighbours = graphMap.GetNeighbors(current.x, current.y);
			for (var i = 0; i < neighbours.Count; i++)
			{
				var neighbour = neighbours[i];
				if (graphMap.TryGetDistance(current, neighbour, out var distance))
				{
					var alt = dist[current] + distance + 1;
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
		var length = dist[to];
		var node = to;
		path.Add(node);
		while (!node.Equals(from))
		{
			node = prev[node];
			path.Add(node);
		}
		
		path.Reverse();
		return (path, length);
	}
	
	public static (List<(int x, int y)> path, double length)? FindPathAStar(this GraphMap graphMap,
		(int x, int y) from, (int x, int y) to, DistanceMetric metric = DistanceMetric.Euclid)
    {
        var gScore = new Dictionary<(int x, int y), double>();
        var fScore = new Dictionary<(int x, int y), double>();
        var prev = new Dictionary<(int x, int y), (int x, int y)>();
        
        gScore[from] = 0.0;
        fScore[from] = GraphMap.GetHeuristicsDistance(from, to, metric);
        
        var openSet = new HashSet<(int x, int y)>();
        var openQueue = new PriorityQueue<(int x, int y), double>();
        var closedSet = new HashSet<(int x, int y)>();

        openSet.Add(from);
        openQueue.Enqueue(from, fScore[from]);

        while (openQueue.TryDequeue(out var current, out _) && !current.Equals(to))
        {
	        openSet.Remove(current);
	        closedSet.Add(current);
            var neighbours = graphMap.GetNeighbors(current.x, current.y);
            for (var i = 0; i < neighbours.Count; i++)
            {
                var neighbour = neighbours[i];
                if (!closedSet.Contains(neighbour) && graphMap.TryGetDistance(current, neighbour, out var distance))
                {
	                var alt = gScore[current] + distance + 1;
	                var neighbourGScore = gScore.GetValueOrDefault(neighbour, double.PositiveInfinity);
	                if (alt < neighbourGScore)
	                {
		                prev[neighbour] = current;
		                gScore[neighbour] = alt;
		                fScore[neighbour] = alt + GraphMap.GetHeuristicsDistance(neighbour, to, metric);
		                if (openSet.Add(neighbour))
		                {
			                openQueue.Enqueue(neighbour, fScore[neighbour]);
		                }
		                else
		                {
			                openQueue.Remove(neighbour, out _, out var prevScore);
			                openQueue.Enqueue(neighbour, Math.Min(prevScore, fScore[neighbour]));
		                }
	                }
                }
            }
        }

        if (!prev.ContainsKey(to) && !from.Equals(to))
        {
            return null;
        }
        
        var path = new List<(int, int)>();
        var length = gScore[to];
        var node = to;
        path.Add(node);
        while (!node.Equals(from))
        {
            node = prev[node];
            path.Add(node);
        }

        path.Reverse();
        return (path, length);
    }
}