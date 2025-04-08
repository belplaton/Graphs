using waterb.Graphs;
using waterb.Graphs.GraphAlgorithms;
using waterb.Graphs.MapAlgorithms;

internal static class Program
{
	public static void Main(string[] args)
	{
		const string filePath = "graph_input.txt";
		var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			"Downloads", filePath);
		if (!File.Exists(path))
		{
			Console.WriteLine($"file'{path}' is not found!");
			return;
		}

		Console.WriteLine("any key to clos...");
		Console.ReadKey();
	}
}

/*

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
		
		*/