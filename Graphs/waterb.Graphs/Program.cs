using waterb.Graphs;
using waterb.Graphs.MapAlgorithms;

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
		if (GraphMapParser.TryCreateGraphMap(lines, out var map))
		{
			var path = map!.FindPathAStar((0, 0), (14, 14), DistanceMetric.Manhattan);
			if (path != null)
			{
				Console.WriteLine($"Metrics: {DistanceMetric.Manhattan}.");
				Console.Write(map?.PrepareMapInfoWithRoute(path));
				Console.WriteLine();
			}
		}

		Console.WriteLine("any key to clos...");
		Console.ReadKey();
	}
}