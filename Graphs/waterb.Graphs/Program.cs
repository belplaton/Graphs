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
			.ParsePayloadInput<RibsListNumericGraphInputParser>(lines, false);
		
		var graph = builder.CreateGraph();
		
		Console.WriteLine(graph.PrepareGraphInfo());

		HashSet<int>? visited = null;
		Stack<GraphAlgorithms.DFSEnumerator<int, int>.DFSNode>? stack = null;
		var result = graph.FindJointsAndBridges(ref visited, ref stack);
		if (result != null)
		{
			Console.WriteLine(result.Value.joints.ToString());
			Console.WriteLine(result.Value.bridges.ToString());

		}
		
		Console.WriteLine("any key to clos...");
		Console.ReadKey();
	}
}