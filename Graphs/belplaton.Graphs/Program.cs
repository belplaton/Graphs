using belplaton.Graphs;

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
		builder.ParsePayloadInput<RibsListNumericGraphInputParser>(lines);
		
		var graph = builder.CreateGraph();
		
		Console.WriteLine(graph.ToString());
		
		Console.WriteLine("any key to clos...");
		Console.ReadKey();
	}
}