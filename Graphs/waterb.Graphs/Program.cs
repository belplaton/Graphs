using waterb.Graphs;

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
			.ParsePayloadInput<RibsListNumericGraphInputParser>(lines);
		
		var graph = builder.CreateGraph();
		
		Console.WriteLine(graph.PrepareGraphInfo());
		
		Console.WriteLine("any key to clos...");
		Console.ReadKey();
	}
}