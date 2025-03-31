using System.Text;
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
			.ParsePayloadInput<RibsListNumericGraphInputParser>(lines, false);
		
		var graph = builder.CreateGraph();
		
		//Console.WriteLine(graph.PrepareGraphInfo());

		/*
		HashSet<int>? visited = null;
		Stack<GraphAlgorithms.DFSEnumerator<int, int>.DFSNode>? stack = null;
		var result = graph.BuildSpanningTreeDFS(ref visited, ref stack, graph.Nodes[0]);
		for (var i = 0; result != null && i < result.Count; i++)
		{
			Console.Write($"{result[i]}, ");
		}
		*/
		
		var result = graph.FloydWarshall();
		Console.Write(result);

		Console.WriteLine("any key to clos...");
		Console.ReadKey();
	}
}