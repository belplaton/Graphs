using System.Collections.Concurrent;

namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public readonly struct HamiltonianCycleAntColonyData
	{
		public readonly int    MaxIterations;
		public readonly 
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
	        Parallel.For(0, antCount, ant =>
	        {
		        var random = randLocal.Value!;
		        var visited = new bool[graph.Size];
		        var tour = new int[graph.Size];
		        var len = 0.0;

		        var current = random.Next(graph.Size);
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
        var result = new List<RibData<TNode>>(bestIdx.Length + 1);
        for (var i = 0; i < bestIdx.Length - 1; i++)
        {
	        var from = graph.Nodes[bestIdx[i]];
	        var to = graph.Nodes[bestIdx[i + 1]];
	        result.Add(new RibData<TNode>(from, to, graph[bestIdx[i]][bestIdx[i + 1]]!.Value));
        }

        bestLen += graph[bestIdx[^1]][bestIdx[0]]!.Value;
        result.Add(new RibData<TNode>(
		    graph.Nodes[bestIdx[^1]], graph.Nodes[bestIdx[0]],
	        graph[bestIdx[^1]][bestIdx[0]]!.Value));
        return (bestLen, result);
    }
}