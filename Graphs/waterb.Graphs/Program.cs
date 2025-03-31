using waterb.Graphs;
using waterb.Graphs.GraphAlgorithms;

internal static class Program
{
	public static void Main(string[] args)
	{
		const string filePath = "graph_input.txt";
		if (!File.Exists(filePath))
		{
			Console.WriteLine($"file'{filePath}' is not found!");
			return;
		}
		
		var lines = File.ReadAllLines(filePath);
		var builder = new NumericGraphBuilder<int>();
		builder
			.SetSettings(GraphSettings.IsDirected | GraphSettings.IsWeighted)
			.ParsePayloadInput<RibsListNumericGraphInputParser>(lines, true);
		
		var graph = builder.CreateGraph();
		
		//Console.WriteLine(graph.PrepareGraphInfo());

		HashSet<int>? visited = null;
		PriorityQueue<(int from, int to), double>? queue = null;
		var result = graph.BuildMinimumSpanningTreePrim(ref visited, ref queue);
		for (var i = 0; result != null && i < result.Count; i++)
		{
			Console.Write($"{result[i]}, ");
		}
		
		Console.WriteLine("any key to clos...");
		Console.ReadKey();
	}
}