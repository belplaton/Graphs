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
			.SetSettings(GraphSettings.IsDirected)
			.ParsePayloadInput<RibsListNumericGraphInputParser>(lines);
		
		var graph = builder.CreateGraph();
		
		Console.WriteLine(graph.PrepareGraphInfo());

		HashSet<int>? visited = null;
		Stack<GraphAlgorithms.DFSEnumerator<int, int>.DFSNode>? stack = null;
		var comps = graph.FindStrongConnectedComponents(ref visited, ref stack);
		for (var i = 0; comps != null && i < comps.Count; i++)
		{
			Console.WriteLine(comps[i].ToString());
		}
		
		Console.WriteLine("any key to clos...");
		Console.ReadKey();
	}
}