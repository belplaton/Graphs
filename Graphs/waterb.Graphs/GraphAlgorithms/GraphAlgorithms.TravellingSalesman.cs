using System.Collections.Concurrent;

namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public readonly struct HamiltonianCycleAntColonyData
	{
		public readonly int    MaxIterations;
		public readonly double Alpha;          // influence of pheromones 
		public readonly double Beta;           // influence of heuristics (1/distance)
		public readonly double Evaporation;    // speed of evaporation (0..1)
		public readonly double Q;              // coefficient of spawned pheromones

		// ReSharper disable once ConvertToPrimaryConstructor
		public HamiltonianCycleAntColonyData(
			int maxIterations = 100,
			double alpha = 1.0,
			double beta = 5.0,
			double evaporation = 0.5,
			double q = 100.0)
		{
			MaxIterations = maxIterations;
			Alpha = alpha;
			Beta = beta;
			Evaporation = evaporation;
			Q = q;
		}

		public HamiltonianCycleAntColonyData() : this(250) { }
	}
	
	public static (double pathLength, List<RibData<TNode>> path)? FindHamiltonianCycleAntColony<TNode, TData>(
        this IGraph<TNode, TData> graph, HamiltonianCycleAntColonyData? settings = default)
        where TNode : notnull
	{
		settings ??= new HamiltonianCycleAntColonyData();
        switch (graph.Size)
        {
	        case 0: return (0, []);
	        case 1: return (0, [new RibData<TNode>(graph.Nodes[0], graph.Nodes[0], 0)]);
        }

        var antCount = graph.Size;
        var randLocal = new ThreadLocal<Random>(() => new Random(Random.Shared.Next()));
        
        var pheromone = new double[graph.Size][];
        for (var i = 0; i < graph.Size; i++)
        {
	        pheromone[i] = new double[graph.Size];
	        Array.Fill(pheromone[i], 1.0);
        }

        int[]? bestIdx = null;
        var bestLen = double.PositiveInfinity;
        for (var iter = 0; iter < settings.Value.MaxIterations; iter++)
        {
	        var bag = new ConcurrentBag<(int[] tour, double len)>();
	        Parallel.For(0, antCount, current =>
	        {
		        var random = randLocal.Value!;
		        var visited = new bool[graph.Size];
		        var tour = new int[graph.Size + 1];
		        var len = 0.0;
		        
		        visited[current] = true;
		        tour[0] = current;
		        
		        for (var step = 1; step < graph.Size; step++)
		        {
			        var sumProbability = 0.0;
			        var probabilities = new double[graph.Size];

			        for (var j = 0; j < graph.Size; j++)
			        {
				        if (visited[j] || !graph[current][j].HasValue) continue;
				        var currentRibWeight = graph[current][j]!.Value;

				        var tau = Math.Pow(
					        pheromone[current][j], settings.Value.Alpha );

				        var eta = Math.Pow(
					        1.0 / Math.Max(currentRibWeight, double.Epsilon), settings.Value.Beta);

				        probabilities[j] = tau * eta;
				        sumProbability += probabilities[j];
			        }

			        if (sumProbability == 0) { len = double.PositiveInfinity; break; }
			        var randomProbability = random.NextDouble() * sumProbability;
			        var tempProbability = 0.0;
			        var next = -1;
			        for (var j = 0; j < graph.Size; j++)
			        {
				        tempProbability += probabilities[j];
				        if (tempProbability >= randomProbability) { next = j; break; }
			        }
			        
			        if (next == -1 || !graph[current][next].HasValue) { len = double.PositiveInfinity; break; }
			        len += graph[current][next]!.Value;
			        current = next;
			        visited[current] = true;
			        tour[step] = current;
		        }
		        
		        var back = graph[current][tour[0]];
		        if (!back.HasValue) len = double.PositiveInfinity;
		        else len += back.Value; tour[^1] = tour[0];

		        if (!double.IsPositiveInfinity(len)) bag.Add((tour, len));
	        });
	        
	        for (var i = 0; i < graph.Size; i++)
	        {
		        for (var j = 0; j < graph.Size; j++)
		        {
			        pheromone[i][j] *= 1 - settings.Value.Evaporation;
			        if (pheromone[i][j] <= 0) pheromone[i][j] = 0.01;
		        }
	        }
	        
	        foreach (var (tour, len) in bag)
	        {
		        if (len < bestLen)
		        {
			        bestLen = len;
			        bestIdx = tour;
		        }

		        var delta = settings.Value.Q / len;
		        for (var fromIndex = 0; fromIndex < tour.Length - 1; fromIndex++)
		        {
			        var from = tour[fromIndex];
			        var to = tour[fromIndex + 1];
			        pheromone[from][to] += delta;
			        if (graph[to][from].HasValue) pheromone[to][from] += delta; // if digraph.
		        }
	        }
        }
        
        if (bestIdx is null) return null;
        var result = new List<RibData<TNode>>(bestIdx.Length);
        for (var i = 0; i < bestIdx.Length - 1; i++)
        {
	        var from = graph.Nodes[bestIdx[i]];
	        var to = graph.Nodes[bestIdx[i + 1]];
	        result.Add(new RibData<TNode>(from, to, graph[bestIdx[i]][bestIdx[i + 1]]!.Value));
        }

        return (bestLen, result);
    }
	
	public static (double pathLength, List<RibData<TNode>> path)? FindHamiltonianCycleBnB<TNode, TData>(
        this IGraph<TNode, TData> graph) where TNode : notnull
	{
		switch (graph.Size)
		{
			case 0: return (0, []);
			case 1: return (0, [new RibData<TNode>(graph.Nodes[0], graph.Nodes[0], 0)]);
		}
		
        var minOut = new double[graph.Size];
        for (var fromIndex = 0; fromIndex < graph.Size; fromIndex++)
        {
	        var currentMinOut = double.PositiveInfinity;
            for (var toIndex = 0; toIndex < graph.Size; toIndex++)
            {
	            if (graph[fromIndex][toIndex].HasValue && fromIndex != toIndex &&
	                graph[fromIndex][toIndex]!.Value < currentMinOut)
	            {
		            currentMinOut = graph[fromIndex][toIndex]!.Value;
	            }
            }
            
            if (double.IsPositiveInfinity(currentMinOut)) return null;
            minOut[fromIndex] = currentMinOut;
        }
        
        var bestLen = double.PositiveInfinity;
        int[]? bestPath = default;
        var currentPath = new int[graph.Size];
        var visited = new bool[graph.Size];

        visited[0] = true;
        currentPath[0] = 0;
        IterateBnBCycle(graph, 0, 0, minOut, currentPath, visited, ref bestLen, ref bestPath);
        
        if (bestPath is null) return null;
        var ribs = new List<RibData<TNode>>(graph.Size);
        for (var i = 0; i < graph.Size - 1; i++)
        {
	        var from = bestPath[i];
	        var to   = bestPath[i + 1];
	        ribs.Add(new RibData<TNode>(graph.Nodes[from], graph.Nodes[to], graph[from][to]!.Value));
        }

        ribs.Add(new RibData<TNode>(graph.Nodes[bestPath[^1]], graph.Nodes[bestPath[0]],
	        graph[bestPath[^1]][bestPath[0]]!.Value));

        return (bestLen, ribs);
        static void IterateBnBCycle(IGraph<TNode, TData> graph, int currentDepth, double currentLength,
	        double[] minOut, int[] currentPath, bool[] visited,
	        ref double bestLen, ref int[]? bestPath)
        {
	        var currentIndex = currentPath[currentDepth];
	        var lowerBound = currentLength + minOut[currentIndex] + minOut[currentPath[0]];
	        for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
	        {
		        if (currentIndex != adjIndex && graph[currentIndex][adjIndex].HasValue && !visited[adjIndex])
		        {
			        lowerBound += minOut[adjIndex];
		        }
	        }
	        
	        if (lowerBound >= bestLen) return;
	        currentPath[currentDepth] = currentIndex;
	        if (currentDepth < graph.Size - 1)
	        {
		        for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
		        {
			        if (currentIndex != adjIndex && graph[currentIndex][adjIndex].HasValue && !visited[adjIndex])
			        {
				        visited[adjIndex] = true;
				        currentPath[currentDepth + 1] = adjIndex;
				        IterateBnBCycle(graph, currentDepth + 1, currentLength + graph[currentIndex][adjIndex]!.Value,
					        minOut, currentPath, visited, ref bestLen, ref bestPath);
				        visited[adjIndex] = false;
			        }
		        }
	        }
	        else
	        {
		        var backWeight = graph[currentIndex][currentPath[0]];
		        if (backWeight.HasValue)
		        {
			        var totalLength = currentLength + backWeight.Value;
			        if (totalLength < bestLen)
			        {
				        bestLen = totalLength;
				        bestPath ??= new int[graph.Size];
				        Array.Copy(currentPath, bestPath, graph.Size);
			        }
		        }
	        }
        }
    }
}